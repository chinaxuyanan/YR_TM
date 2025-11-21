using Logger.Alarm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 定义日志记录器接口
    /// 所有Logger类都应实现该接口，以确保一致的日志输出方式
    /// </summary>
    public interface ILogger
    {
        ///<summary>
        ///通用日志写入方法，可指定日志级别和异常信息
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志内容</param>
        /// <param name="ex">异常信息（可选）</param>
        void Log(LogLevel level, string message, Exception ex = null);

        ///<summary>
        ///写入调制日志（用于开发阶段调试信息）
        /// </summary>
        void Debug(string message);

        ///<summary>
        ///写入普通信息日志
        /// </summary>
        void Info(string message);

        ///<summary>
        ///写入警告日志（潜在风险但不影响运行）
        /// </summary>
        void Warn(string message);

        ///<summary>
        ///写入错误日志（出现问题但仍可继续运行）
        /// </summary>
        void Error(string message, Exception ex = null);

        ///<summary>
        ///写入致命错误日志（严重问题可能导致程序崩溃）
        /// </summary>
        void Fatal(string message, Exception ex = null);

        //添加GetAlarmManager方法
        AlarmManager GetAlarmManager();

        //添加报警信息
        void LogAlarm(int id, string alarmCode, string message);

        //结束报警
        void EndAlarm(int id);
    }
}
