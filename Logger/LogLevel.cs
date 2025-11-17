using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 定义日志级别枚举，用于控制日志输出
    /// 日志级别枚举，表示日志的重要程度
    /// </summary>
    public enum LogLevel
    {
        //最详细的日志级别，用于开发调试
        Trace = 0,

        //调试信息级别，用于记录程序运行的调试信息
        Debug = 1,

        //普通信息级别，用于程序正常运行状态
        Info = 2,

        //警告级别，出现潜在问题或异常情况，但程序仍可继续运行
        Warn = 3,

        //错误级别，出现严重问题，可能影响程序功能，但不会导致奔溃
        Error = 4,

        //致命错误，出现程序中断或奔溃的严重问题
        Fatal = 5
    }
    
}
