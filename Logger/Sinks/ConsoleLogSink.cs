using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Sinks
{
    /// <summary>
    /// 控制台输出类
    /// 实现ILogSink接口，用于将日志内容输出到控制台
    /// </summary>
    public class ConsoleLogSink : ILogSink
    {
        private readonly ILogFormatter _formatter;
        private readonly object _lock = new object();

        //构造函数
        public ConsoleLogSink(ILogFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// 同步写入日志到控制台
        /// </summary>
        public void Write(LogMessage message)
        {
            lock (_lock)
            {
                SetConsoleColor(message.Level);
                Console.WriteLine(_formatter.Format(message));
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 异步写入到控制台
        /// </summary>
        /// <param name="message"></param>
        public Task WriteAsync(LogMessage message)
        {
            return Task.Run(() => Write(message));
        }

        ///<summary>
        ///根据日志级别设置不同的控制台颜色
        /// </summary>
        /// <param name="level">日志级别</param>
        private void SetConsoleColor(LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.DarkGray; break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray; break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Green; break;
                case LogLevel.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red; break;
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.Magenta; break;
                default:
                    Console.ResetColor();
                    break;
            }
        }
    }
}
