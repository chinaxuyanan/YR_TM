using DeviceCommLib.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommLib.Ethernet
{
    public class EthernetConnection : DeviceBase
    {
        private readonly string _ip;
        private readonly int _port;
        private UdpClient _udpClient;
        private IPEndPoint _endPoint;

        public EthernetConnection(string ip, int port)
        {
            _ip = ip;
            _port = port;
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
        }
        public override void Connect()
        {
            try
            {
                _udpClient.Connect(_endPoint);
                IsConnected = true;
                RaiseConnectionChanged(true);
            }catch (Exception ex)
            {
                RaiseError($"以太网连接失败：{ex.Message}");
                IsConnected = false;
                RaiseConnectionChanged(false);

                if (AutoReconnect)
                    AttemptReconnect();
            }
        }

        public override void Disconnect()
        {
            _udpClient?.Close();
            IsConnected = false;
            RaiseConnectionChanged(false);
        }

        public override void Send(byte[] data)
        {
            if (!IsConnected) return;
            try { _udpClient.Send(data, data.Length); } catch (Exception ex) { RaiseError($"以太网发送数据失败：{ex.Message}"); }
        }
    }
}
