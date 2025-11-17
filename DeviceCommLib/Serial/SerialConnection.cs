using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DeviceCommLib.Base;

namespace DeviceCommLib.Serial
{
    /// <summary>
    /// 串口连接实现类
    /// 支持事件驱动的数据接收
    /// </summary>
    public class SerialConnection : DeviceBase
    {
        private SerialPort _port;

        public SerialConnection(string portName, int baudRate = 9600)
        {
            _port = new SerialPort(portName, baudRate);
            _port.DataReceived += (s, e) =>
            {
                byte[] buffer = new byte[_port.BytesToRead];
                _port.Read(buffer, 0, buffer.Length);
                RaiseDataReceived(buffer);
            };
        }
        public override void Connect()
        {
            try
            {
                _port.Open();
                IsConnected = true;
                RaiseConnectionChanged(true);
            }catch(Exception ex)
            {
                RaiseError($"串口连接失败：{ex.Message}");
                IsConnected = false;
                RaiseConnectionChanged(false);

                //调用基类重连机制
                if(AutoReconnect)
                    AttemptReconnect();
            }
        }

        public override void Disconnect()
        {
            if (IsConnected)
            {
                _port.Close();
                IsConnected = false;
                RaiseConnectionChanged(false);
            }
        }

        public override void Send(byte[] data)
        {
            if (!IsConnected) return;
            try { _port.Write(data, 0, data.Length); }
            catch(Exception ex) { RaiseError($"串口发送数据失败：{ex.Message}"); }
        }
    }
}
