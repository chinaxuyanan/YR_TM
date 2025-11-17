using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommLib.Interfaces
{
    /// <summary>
    /// 定义通用设备连接接口
    /// 所有通信类（tcp、串口、以太网等）都应实现该接口
    /// </summary>
    /// <author>xuyanan</author>
    public interface IDeviceConnection: IDisposable
    {
        /// <summary>
        /// 获取连接状态
        /// </summary>
        bool IsConnected { get; }

        ///<summary>
        /// 建立连接
        /// </summary>
        void Connect();

        ///<summary>
        ///关闭连接
        /// </summary>
        void Disconnect();

        ///<summary>
        ///发送数据
        /// </summary>
        void Send(byte[] data);

        ///<summary>
        ///接收数据事件
        /// </summary>
        event Action<byte[]> OnDataReceived;

        ///<summary>
        ///连接事件
        /// </summary>
        event Action<bool> OnConnectionChanged;

        ///<summary>
        ///异常事件
        /// </summary>
        event Action<string> OnError;
    }
}
