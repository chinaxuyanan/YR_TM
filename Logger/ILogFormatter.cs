using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 定义日志格式化器接口
    /// 格式化器用于将结构化的日志对象（LogMessage）
    /// 转化为最终输出的字符串格式
    /// </summary>
    public interface ILogFormatter
    {
        ///<summary>
        ///将日志消息对象格式化为字符串
        /// </summary>
        /// <param name="message">日志消息对象</param>
        /// <returns>格式化后的字符串</returns>
        string Format(LogMessage message);
    }
}
