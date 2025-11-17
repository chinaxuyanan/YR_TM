using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 日志实现类
    /// 负责接收日志消息、异步写入日志队列、分发到各个输出目标
    /// </summary>
    public class Logger : ILogger, IDisposable
    {
        private readonly string _name;
        private readonly LogLevel _minLevel;
        private readonly IEnumerable<ILogSink> _sinks;
        private readonly ILogFormatter _formatter;

        //阻塞队列，用于存储待写入的日志
        private readonly BlockingCollection<LogMessage> _logQueue = new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>());

        //日志后台写入线程
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        //保存后台Task，确保退出时能等待
        private readonly Task _workerTask;

        /// <summary>
        /// 创建新的logger实例
        /// </summary>
        /// <param name="name">日志器名称（类名或模块名）</param>
        /// <param name="minLevel">最小输出日志级别</param>
        /// <param name="sinks">输出目标集合</param>
        /// <param name="formatter">日志格式化器</param>
        public Logger(string name, LogLevel minLevel, IEnumerable<ILogSink> sinks, ILogFormatter formatter)
        {
            _name = name;
            _minLevel = minLevel;
            _sinks = sinks;
            _formatter = formatter;

            //启动后台日志线程(改成不会递归写日志)
            _workerTask = Task.Factory.StartNew(
                () => ProcessLogQueue(),
                _cancellation.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );

            //启动后台日志线程
            //Task.Factory.StartNew(ProcessLogQueue,
            //    _cancellation.Token,
            //    TaskCreationOptions.LongRunning,
            //    TaskScheduler.Default);
        }

        /// <summary>
        /// 日志主逻辑：将日志加入队列，后台线程负责写入
        /// </summary>
        public void Log(LogLevel level, string message, Exception ex = null)
        {
            //如果日志级别低于设定级别，则忽略
            if (level < _minLevel) return;

            var log = new LogMessage
            {
                Timestamp = DateTime.Now,
                Level = level,
                LoggerName = _name,
                Message = message,
                Exception = ex,
                ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(),
                Context = null
            };

            if (!_logQueue.IsAddingCompleted)
            {
                //TryAdd 不阻塞，避免暴涨
                _logQueue.TryAdd(log);
            }
            //_logQueue.Add(log);
        }

        #region - 快捷日志方法
        public void Debug(string message) => Log(LogLevel.Debug, message);

        public void Error(string message, Exception ex = null) => Log(LogLevel.Error, message, ex);

        public void Fatal(string message, Exception ex = null) => Log(LogLevel.Fatal, message, ex);

        public void Info(string message) => Log(LogLevel.Info, message);

        public void Warn(string message) => Log(LogLevel.Warn, message);
        #endregion

        /// <summary>
        /// 后台线程方法：持续从队列中取出日志，分发到所有Sink
        /// </summary>
        private async Task ProcessLogQueue()
        {
            try
            {
                foreach (var log in _logQueue.GetConsumingEnumerable(_cancellation.Token))
                {
                    foreach (var sink in _sinks)
                    {
                        try
                        {
                            await sink.WriteAsync(log).ConfigureAwait(false);
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine("sink.WriteAsync Failed");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }

            //foreach (var log in _logQueue.GetConsumingEnumerable(_cancellation.Token))
            //{
            //    try
            //    {
            //        //遍历所有输出目标，执行异步写入
            //        foreach (var sink in _sinks)
            //        {
            //            await sink.WriteAsync(log);
            //        }
            //    }catch (Exception ex)
            //    {
            //        Error("logger 写入日志失败", ex);
            //    }
            //}
        }
        public void Dispose()
        {
            try
            {
                _logQueue.CompleteAdding();
                _cancellation.Cancel();

                _workerTask.Wait(2000);
            }
            catch
            {

            }

            //_logQueue.CompleteAdding();
            //_cancellation.Cancel();
        }


    }
}
