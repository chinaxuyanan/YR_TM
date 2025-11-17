using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using DeviceCommLib.Base;


namespace DeviceCommLib.Tcp
{
    /// <summary>
    /// tcp 客户端连接实现类
    /// 支持自动重连、数据接收线程
    /// </summary>
    public class TcpClientConnection : DeviceBase
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;

        public TcpClientConnection(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public override void Connect()
        {
            try
            {
                _client = new TcpClient(_ip, _port);
                _stream = _client.GetStream();
                IsConnected = true;
                RaiseConnectionChanged(true);

                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                _receiveThread.Start();
            }
            catch(Exception ex)
            {
                RaiseError($"TCP连接失败：{ex.Message}");
                IsConnected = false;
                RaiseConnectionChanged(false);

                //调用基类的重连机制
                if(AutoReconnect)
                    AttemptReconnect();
            }
        }

        private void ReceiveLoop()
        {
            try
            {
                while (IsConnected)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        byte[] data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        RaiseDataReceived(data);
                    }
                    else
                    {
                        Disconnect();
                        break;
                    }
                }
            }catch(Exception ex)
            {
                RaiseError($"TCP接受数据异常：{ex.Message}");
                Disconnect();
            }
        }

        public override void Send(byte[] data)
        {
            if (!IsConnected) return;
            try
            {
                _stream.Write(data, 0, data.Length);
            }catch(Exception ex)
            {
                RaiseError($"TCP发送数据失败：{ex.Message}");
            }
        }

        public override void Disconnect()
        {
            IsConnected = false;
            RaiseConnectionChanged(false);

            _stream?.Close();
            _client?.Close();
        }
    }
}
