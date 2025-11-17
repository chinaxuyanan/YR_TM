using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 表示日志记录的完整信息
    /// 用于在日志生成与输出之间传递日志内容
    /// </summary>
    public class LogMessage
    {
        //日志生成的时间戳
        public DateTime Timestamp { get; set; } = DateTime.Now; 

        //日志级别
        public LogLevel Level { get; set; }

        //产生日志的模块或类名称
        public string LoggerName { get; set; } = string.Empty;

        //日志的主要内容
        public string Message { get; set; } = string.Empty;

        //如果日志由异常触发，这里保存异常对象
        public Exception Exception { get; set; }

        //记录产生日志的线程 ID （用于多线程环境下追踪）
        public string ThreadId { get; set; } = string.Empty;

        //附加上下文信息（如：用户ID、请求ID、Session等）
        public IDictionary<string, object> Context { get; set; }

        //重写ToString()，便于调试和快速查看日志对象内容
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] [{LoggerName}] {Message}" +
               (Exception != null ? $" Exception: {Exception.Message}" : string.Empty);
        }
    }
}
