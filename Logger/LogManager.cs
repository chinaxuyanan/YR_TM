using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// 日志管理器
    /// 负责：
    /// 1.统一管理Logger实例
    /// 2.管理全局Sink（输出目标）与Formatter（格式化器）
    /// 3.提供动态配置与释放功能
    /// </summary>
    public static class LogManager
    {
        //存储所有创建过的Logger（按名称缓存）
        private static readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();

        //全局Sink列表（控制台、文件、UI等）
        private static readonly List<ILogSink> _globalSinks = new List<ILogSink>();

        //默认格式化器
        private static ILogFormatter _defaultFormatter = new Formatters.SimpleFormatter();

        //默认日志级别
        private static LogLevel _defaultLevel = LogLevel.Debug;

        //全局锁，保证线程安全
        private static readonly object _lock = new object();

        ///<summary>
        ///添加一个全局sink（日志输出目标）
        /// </summary>
        /// <param name="sink">输出目标对象</param>
        public static void AddSink(ILogSink sink)
        {
            lock (_lock)
            {
                _globalSinks.Add(sink);
            }
        }

        ///<summary>
        ///设置默认日志格式化器
        /// </summary>
        /// <param name="formatter">格式化器实例</param>
        public static void SetFormatter(ILogFormatter formatter)
        {
            _defaultFormatter = formatter;
        }

        ///<summary>
        ///设置默认日志级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public static void SetDefaultLevel(LogLevel level)
        {
            _defaultLevel = level;
        }

        ///<summary>
        ///获取或创建一个指定名称的Logger实例
        /// </summary>
        /// <param name="name">Logger 名称（类名或模块名）</param>
        /// <returns>logger实例</returns>
        public static ILogger GetLogger(string name)
        {
            lock (_lock)
            {
                if(_loggers.TryGetValue(name, out var existing))
                    return existing;

                //创建新Logger
                var logger = new Logger(name, _defaultLevel, _globalSinks, _defaultFormatter);
                _loggers[name] = logger;
                return logger;
            }
        }

        ///<summary>
        ///关闭所有Logger（释放后台线程）
        ///应在应用退出时调用
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                foreach (var logger in _loggers.Values)
                {
                    logger.Dispose();
                }
                _loggers.Clear();
            }
        }
    }
}
