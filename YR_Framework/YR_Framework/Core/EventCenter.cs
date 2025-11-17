using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace YR_Framework.Core
{
    /// <summary>
    /// 全局事件中心、用于模块间通信
    /// </summary>
    public static class EventCenter
    {
        //事件储存表：每种事件类型对应一组委托
        private static readonly ConcurrentDictionary<Type, List<WeakReference>> _subscribers = new ConcurrentDictionary<Type, List<WeakReference>>();

        //UI线程同步上下文
        private static SynchronizationContext _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

        private static readonly object _lock = new object();

        ///<summary>
        ///初始化UI线程上下文（建议在Program.Main里调用）
        /// </summary>
        public static void InitializeUIContext()
        {
            lock (_lock)
            {
                if(_uiContext == null)
                    _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
            }
        }

        /// <summary>
        /// 订阅一个事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler, Control autoUnsubscribeOwner = null)
        {
            if (handler == null) return;

            var list = _subscribers.GetOrAdd(typeof(T), _ => new List<WeakReference>());

            lock (list)
            {
                if(!list.Any(wr => wr.IsAlive && wr.Target == (object)handler))
                    list.Add(new WeakReference(handler));
            }

            //自动解除订阅
            if(autoUnsubscribeOwner != null)
            {
                autoUnsubscribeOwner.Disposed += (_, __) => Unsubscribe(handler);
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;

            if(!_subscribers.TryGetValue(typeof(T), out var list)) return;

            lock (list)
            {
                list.RemoveAll(wr => !wr.IsAlive || wr.Target.Equals(handler));
            }
        }

        /// <summary>
        /// 发布事件，自动切回UI线程
        /// </summary>
        public static void Publish<T>(T message)
        {
            if(!_subscribers.TryGetValue(typeof(T), out var list)) return;

            List<Action<T>> liveHandlers;

            lock (list)
            {
                liveHandlers = list.Where(wr => wr.IsAlive).Select(wr => wr.Target).OfType<Action<T>>().ToList();

                //清除无效引用
                list.RemoveAll(wr => !wr.IsAlive);
            }

            foreach (var handler in liveHandlers)
            {
                var context = _uiContext ?? new WindowsFormsSynchronizationContext();

                if(SynchronizationContext.Current == context)
                {
                    //已在UI线程
                    SafeInvoke(handler, message);
                }
                else
                {
                    //回到UI线程执行
                    _uiContext.Post(_ => SafeInvoke(handler, message), null);
                }
            }
        }

        private static void SafeInvoke<T>(Action<T> handler, T arg)
        {
            try
            {
                handler(arg);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"[EventCenter]事件执行异常: {ex.Message}");
            }
        }
    }
}
