using DeviceCommLib.Interfaces;
using DeviceCommLib.Serial;
using DeviceCommLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceCommLib.Implementation
{
    public class ElectricScrewdrivers
    {
        private IDeviceConnection _eletricScrewdriver;

        private string ReceiveMessage;

        private Action<string> HandleReceiveAction;

        public Action<string, bool, int> RunningInfoAction;

        private string EndMarkReceive = ""; //接受数据结束符

        private string SendMessage;

        public bool ConnectStatus { get; set; }

        public bool IsHex = true;

        public ElectricScrewdrivers(string portName,int port)
        {
            _eletricScrewdriver = ConnectionFactory.CreateConnection("serial", portName,port);

      
            _eletricScrewdriver.OnDataReceived +=OnDataReceived;

            _eletricScrewdriver.OnError += OnError;

            _eletricScrewdriver.OnConnectionChanged += OnConnectionChanged; ;

            HandleReceiveAction = InitializeReceive;
        }

        private void InitializeReceive(string message)
        {

        }
        private void OnConnectionChanged(bool obj)
        {
            ConnectStatus = obj;
        }

        public bool Connect()
        {
            if (_eletricScrewdriver.IsConnected)
            {
                return true;
            }
            _eletricScrewdriver?.Connect();
            return _eletricScrewdriver.IsConnected;
        
        }

        ManualResetEvent write_Event = new ManualResetEvent(false);
        ManualResetEvent read_Event = new ManualResetEvent(false);
        public bool ReadMethod(ushort id, ushort address, ushort length, out int[] readReceiveArray)
        {
            HandleReceiveAction = ReadReceiveMessage;
            readReceiveArray = new int[length];
            SendMessage = ReadRegisterEncode(id, address, length);
            byte[] send = ModbusCrcHelper.HexStringToBytes(SendMessage);
            read_Event.Reset();
            _eletricScrewdriver.Send(send);
            if (read_Event.WaitOne(2000))
            {
                readReceiveArray = ReceiveMessageArray;
                return true;
            }
            else
            {
                return false;
            }

        }
        private void WriteReceiveMessage(string message)
        {

            if (message == SendMessage)
            {
                write_Event.Set();
            }
        }
        public bool WriteMethod(ushort id, ushort address, ushort value)
        {
            HandleReceiveAction = WriteReceiveMessage;
            SendMessage = WriteRegisterEncode(id, address, value);
            byte[] send = ModbusCrcHelper.HexStringToBytes(SendMessage);
            _eletricScrewdriver.Send(send);
            write_Event.Reset();
            if (write_Event.WaitOne(2000))
            {
                RunningInfoActionMessage($"Local-->Screwdriver-{address}-{value}-成功", false, InforLevel.INFO);
                return true;
            }
            else
            {
                RunningInfoActionMessage($"Local-->Screwdriver-{address}-{value}-失败", false, InforLevel.INFO);
                return false;
            }

        }

        public string WriteRegisterEncode(ushort id, ushort address, int value)
        {
            List<string> data = new List<string>();
            data.Add(id.ToString("00"));
            data.Add("06");
            string add = address.ToString("X4");
            data.Add(add);
            string va = value.ToString("X4");
            data.Add(va);
            //进行CRC校验
            string data2 = ModbusCrcHelper.CalculateModbusCrcString(data);
            return data2;
        }
        private int[] ReceiveMessageArray;
        public void ReadReceiveMessage(string message)
        {
            int receivedatalength = Convert.ToInt32(message.Substring(4, 2), 16);

            string receiveData = message.Substring(6, receivedatalength);
            ReceiveMessageArray = new int[receivedatalength];
            for (int i = 0; i < receivedatalength; i++)
            {
                ReceiveMessageArray[i] = Convert.ToInt16(receiveData.Substring(i * 2, 2), 16);
            }
            read_Event.Set();
        }

        public string ReadRegisterEncode(ushort id, ushort address, ushort length)
        {
            List<string> data = new List<string>();
            data.Add(id.ToString("00")); ;
            data.Add("03");
            string add = address.ToString("X4");
            data.Add(add);
            string len = length.ToString("X4");
            data.Add(len);

            //进行CRC校验
            string data2 = ModbusCrcHelper.CalculateModbusCrcString(data);

            return data2;

        }
        private void OnError(string mes)
        {
            RunningInfoActionMessage($"{DateTime.Now.ToString()}错误：{mes}", true, InforLevel.ERROR);
        }

        private void OnDataReceived(byte[] data)
        {
            //处理收到的数据
            //ReceiveMessage = ReceiveMessage + Encoding.ASCII.GetString(data);
            if (IsHex)
            {
                //ReceiveMessage = ReceiveMessage + HexEncoding.decode(data);

                ReceiveMessage = ReceiveMessage + BitConverter.ToString(data).Replace("-", "").ToUpper();
            }
            else
            {
                ReceiveMessage = ReceiveMessage + Encoding.ASCII.GetString(data);
            }
            if (ReceiveMessage != null)
            {
                if (string.IsNullOrEmpty(EndMarkReceive))
                {
                    if (HandleReceiveAction != null)
                    {
                        HandleReceiveAction(ReceiveMessage);
                        ReceiveMessage = "";
                    }
                }
                else if (ReceiveMessage.Contains(EndMarkReceive) && ReceiveMessage.EndsWith(EndMarkReceive))
                {
                    List<string> MessgeList = new List<string>();
                    while (ReceiveMessage != "")
                    {
                        int i = ReceiveMessage.IndexOf(EndMarkReceive);
                        string m = ReceiveMessage.Substring(0, i);
                        ReceiveMessage = ReceiveMessage.Substring(EndMarkReceive.Length + m.Length, ReceiveMessage.Length - EndMarkReceive.Length - m.Length);
                        if (m != "")
                            MessgeList.Add(m);
                    }
                    while (MessgeList.Count > 0)
                    {
                        HandleReceiveAction(MessgeList[0]);
                        MessgeList.Remove(MessgeList[0]);
                    }
                }
                else
                {
                    RunningInfoActionMessage($"{Encoding.ASCII}末尾没有结束符号", true, InforLevel.ERROR);
                }
            }
        }

        private void RunningInfoActionMessage(string message, bool IsShow, int leval)
        {
            if (RunningInfoAction != null)
            {
                RunningInfoAction(message, IsShow, leval);
            }
        }
    }
    public class ModbusCrcHelper
    {
        /// <summary>
        /// 对16进制字符串列表进行Modbus CRC校验，返回完整的16进制字符串（包含CRC校验码）
        /// </summary>
        /// <param name="hexStrings">16进制字符串列表</param>
        /// <returns>包含CRC校验码的完整16进制字符串</returns>
        public static string CalculateModbusCrcString(List<string> hexStrings)
        {
            if (hexStrings == null || hexStrings.Count == 0)
                return string.Empty;

            // 1. 将16进制字符串列表转换为字节数组
            string hexString = string.Concat(hexStrings);

            if (hexString.Length % 2 != 0)
                throw new ArgumentException("16进制字符串长度必须为偶数");

            byte[] dataBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < dataBytes.Length; i++)
            {
                string hexByte = hexString.Substring(i * 2, 2);
                dataBytes[i] = Convert.ToByte(hexByte, 16);
            }

            // 2. 计算Modbus CRC16校验码
            ushort crc = 0xFFFF;

            foreach (byte b in dataBytes)
            {
                crc ^= b;

                for (int i = 0; i < 8; i++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;

                    if (lsb)
                        crc ^= 0xA001;
                }
            }

            // 3. 获取CRC字节（小端模式：低字节在前，高字节在后）
            byte crcLow = (byte)(crc & 0xFF);
            byte crcHigh = (byte)((crc >> 8) & 0xFF);

            // 4. 构建完整的16进制字符串
            StringBuilder resultBuilder = new StringBuilder();

            // 添加原始数据
            foreach (string hex in hexStrings)
            {
                resultBuilder.Append(hex);
            }

            // 添加CRC校验码（小端模式）
            resultBuilder.Append(crcLow.ToString("X2"));
            resultBuilder.Append(crcHigh.ToString("X2"));

            return resultBuilder.ToString();
        }

        /// <summary>
        /// 返回带空格的格式化16进制字符串（便于阅读）
        /// </summary>
        public static string CalculateModbusCrcStringFormatted(List<string> hexStrings)
        {
            string result = CalculateModbusCrcString(hexStrings);

            // 每2个字符插入一个空格
            StringBuilder formatted = new StringBuilder();
            for (int i = 0; i < result.Length; i += 2)
            {
                if (i > 0) formatted.Append(" ");
                formatted.Append(result.Substring(i, 2));
            }

            return formatted.ToString();
        }

        /// <summary>
        /// 将16进制字符串转换为字节数组
        /// </summary>
        public static byte[] HexStringToBytes(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return new byte[0];

            if (hexString.Length % 2 != 0)
                throw new ArgumentException("16进制字符串长度必须为偶数");

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string hexByte = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(hexByte, 16);
            }

            return bytes;
        }

    }
    static class HexEncoding
    {
        public static byte[] Encoding(string data)
        {
            data = data.Replace(" ", "");

            if (data.Length % 2 == 1)
            {
                data = "0" + data;
            }
            List<string> SendDataList = new List<string>();
            for (int i = 0; i < data.Length; i = i + 2)
            {
                SendDataList.Add(data.Substring(i, 2));
            }
            byte[] bytes = new byte[SendDataList.Count];
            for (int j = 0; j < bytes.Length; j++)
            {
                bytes[j] = (byte)(Convert.ToInt32(SendDataList[j], 16));
            }
            return bytes;
        }


        public static string decode(byte[] ReDatas)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }
            return sb.ToString();
        }


        public const string PATTERN = @"([^A-Fa-f0-9]|\s+?)+";
        public static bool IsIllegalHexadecimal(string hex)
        {
            return !System.Text.RegularExpressions.Regex.IsMatch(hex, PATTERN);
        }
    }
    public static class InforLevel
    {
        public static int INFO = 0;
        public static int DEBUG = 1;
        public static int ERROR = 2;
        public static int WARN = 3;
    }
}
