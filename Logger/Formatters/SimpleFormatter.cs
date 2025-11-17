using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Formatters
{
    /// <summary>
    /// 简单格式化器
    /// 格式：[时间] [级别] [线程ID] [Logger名称] 消息
    /// </summary>
    public class SimpleFormatter : ILogFormatter
    {
        /// <summary>
        /// 将LogMessage格式化为刻度字符串
        /// </summary>
        /// <param name="message">日志对象消息</param>
        /// <returns>格式化后的字符串</returns>
        public string Format(LogMessage message)
        {
            var sb = new StringBuilder();

            //时间戳部分
            sb.Append('[')
                .Append(message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append("] ");

            //日志级别
            sb.Append('[')
                .Append(message.Level.ToString().ToUpper())
                .Append("]");

            //线程信息
            if (!string.IsNullOrEmpty(message.ThreadId))
            {
                sb.Append("[T:")
                    .Append(message.ThreadId)
                    .Append("]");
            }

            //Logger名称
            if (!string.IsNullOrEmpty(message.LoggerName))
            {
                sb.Append('[')
                    .Append(message.LoggerName)
                    .Append("]");
            }

            //主消息内容
            sb.Append(message.Message);

            //如果有异常，追加详细信息
            if(message.Exception != null)
            {
                sb.AppendLine()
                    .Append("Exception: ")
                    .Append(message.Exception.Message)
                    .AppendLine()
                    .Append(message.Exception.StackTrace);
            }
            return sb.ToString();
        }
    }
}
