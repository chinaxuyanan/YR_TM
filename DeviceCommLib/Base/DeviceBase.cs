using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceCommLib.Interfaces;
using Logger;

namespace DeviceCommLib.Base
{
    /// <summary>
    /// 通信设备基类，包含通用事件、自动重连、状态管理等
    /// </summary>
    public abstract class DeviceBase : IDeviceConnection
    {
        private ILogger logger = LogManager.GetLogger("DeviceBase");
        public bool IsConnected {  get; protected set; }
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectIntervalMs { get; set; } = 3000;
        public int MaxReconnectAttempts { get; set; } = 5;

        public event Action<byte[]> OnDataReceived;
        public event Action<bool> OnConnectionChanged;
        public event Action<string> OnError;

        private int currentReconnectAttempts = 0;
        private bool _isReconnecting = false;
        private static readonly object reconnectLock = new object();

        protected void RaiseDataReceived(byte[] data) => OnDataReceived?.Invoke(data);

        protected void RaiseConnectionChanged(bool status) => OnConnectionChanged?.Invoke(status);

        protected void RaiseError(string message)
        {
            logger.Error($"Error om {this.GetType().Name}: {message}");
            OnError?.Invoke(message);
        } 

        public virtual void Dispose() => Disconnect();

        //默认的重连处理
        public void AttemptReconnect()
        {
            if(_isReconnecting || currentReconnectAttempts >= MaxReconnectAttempts) return;

            lock (reconnectLock)
            {
                _isReconnecting = true;
                currentReconnectAttempts++;
                Task.Delay(ReconnectIntervalMs).ContinueWith(t =>
                {
                    if(currentReconnectAttempts >= MaxReconnectAttempts)
                    {
                        RaiseError("Max reconnect attempts reached.");
                        _isReconnecting = false;
                        return;
                    }

                    bool reconnectSuccess = TryReconnect();
                    if (reconnectSuccess)
                    {
                        currentReconnectAttempts = 0;
                        RaiseConnectionChanged(true);
                    }
                    else
                    {
                        RaiseError("Reconnect failed.");
                    }
                    _isReconnecting = false;
                });
            }

            _isReconnecting = true;
            Task.Delay(ReconnectIntervalMs).ContinueWith(t =>
            {
                if (!IsConnected)
                    Connect();
                _isReconnecting = false;
            });
        }

        private bool TryReconnect()
        {
            return true;
        }

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract void Send(byte[] data);
    }
}
