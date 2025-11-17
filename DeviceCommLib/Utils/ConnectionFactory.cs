using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceCommLib.Interfaces;
using DeviceCommLib.Tcp;
using DeviceCommLib.Serial;

namespace DeviceCommLib.Utils
{
    /// <summary>
    /// 通信对象创建工厂
    /// 根据配置返回对应的连接类型实例
    /// </summary>
    public static class ConnectionFactory
    {
        public static IDeviceConnection CreateConnection(string type, string address, int port = 0)
        {
            switch (type.ToLower())
            {
                case "tcp": return new TcpClientConnection(address, port);
                case "serial": return new SerialConnection(address, port);
                default: throw new ArgumentException($"未知连接类型：{type}");
            }
        }
    }
}
