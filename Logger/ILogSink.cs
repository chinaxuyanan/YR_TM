using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 定义日志输出目标接口（singk）
    /// </summary>
    public interface ILogSink
    {
        ///<summary>
        ///同步写入日志（少用，仅用于调试或者控制台输出）
        /// </summary>
        /// <param>日志消息</param>
        void Write(LogMessage message);

        ///<summary>
        ///异步写入日志，推荐在文件、UI等耗时输出场景使用
        /// </summary>
        Task WriteAsync(LogMessage message);
    }
}
