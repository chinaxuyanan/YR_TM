using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceCommLib.Interfaces;

namespace DeviceCommLib.Base
{
    /// <summary>
    /// 通信设备基类，包含通用事件、自动重连、状态管理等
    /// </summary>
    public abstract class DeviceBase : IDeviceConnection
    {
        public bool IsConnected {  get; protected set; }
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectIntervalMs { get; set; } = 3000;

        public event Action<byte[]> OnDataReceived;
        public event Action<bool> OnConnectionChanged;
        public event Action<string> OnError;

        private bool _isReconnecting = false;

        protected void RaiseDataReceived(byte[] data) => OnDataReceived?.Invoke(data);

        protected void RaiseConnectionChanged(bool status) => OnConnectionChanged?.Invoke(status);

        protected void RaiseError(string message) => OnError?.Invoke(message);

        public virtual void Dispose() => Disconnect();

        //默认的重连处理
        public void AttemptReconnect()
        {
            if(_isReconnecting) return;

            _isReconnecting = true;
            Task.Delay(ReconnectIntervalMs).ContinueWith(t =>
            {
                if (!IsConnected)
                    Connect();
                _isReconnecting = false;
            });
        }

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract void Send(byte[] data);
    }
}
