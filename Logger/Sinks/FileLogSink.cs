using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Sinks
{
    /// <summary>
    /// 文件日志输出类
    /// 支持异步写入、自动轮转（按大小或者日期）、线程安全
    /// </summary>
    public class FileLogSink : ILogSink, IDisposable
    {
        private readonly ILogFormatter _formatter;
        private readonly string _logDirectory;
        private readonly string _fileNamePrefix;
        private readonly long _maxFileSizeBytes;
        private readonly bool _rotateByDate;

        private string _currentLogFilePath;
        private StreamWriter _writer;
        private readonly object _fileLock = new object();

        //日志队列，用于异步写入
        private readonly BlockingCollection<LogMessage> _queue = new BlockingCollection<LogMessage>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _workerTask;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="formatter">日志格式化器</param>
        /// <param name="logDirectory">日志保存目录</param>
        /// <param name="fileNamePrefix">日志文件名前缀（默认”log“）</param>
        /// <param name="maxFileSizeBytes">日志文件最大字节数（默认5M），超过后自动轮转</param>
        /// <param name="rotateByDate">是否每天创建一个新的日志文件</param>
        public FileLogSink(ILogFormatter formatter, string logDirectory = "logs", string fileNamePrefix = "log", long maxFileSizeBytes = 5 * 1024 * 1024, bool rotateByDate = true)
        {
            _formatter = formatter;
            _logDirectory = logDirectory;
            _fileNamePrefix = fileNamePrefix;
            _maxFileSizeBytes = maxFileSizeBytes;
            _rotateByDate = rotateByDate;

            Directory.CreateDirectory(_logDirectory);
            _currentLogFilePath = GetLogFilePath();

            _writer = new StreamWriter(new FileStream(_currentLogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8)
            {
                AutoFlush = true
            };
            _workerTask = Task.Run(ProcessQueueAsync);
        }

        public void Write(LogMessage message)
        {
            if(!_queue.IsAddingCompleted)
                _queue.Add(message);
        }

        public Task WriteAsync(LogMessage message)
        {
            Write(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 后台任务: 持续从队列中读取日志并写入文件
        /// </summary>
        private async Task ProcessQueueAsync()
        {
            try
            {
                foreach (var message in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        RotateIfNeeded();
                        var formatted = _formatter.Format(message);
                        lock (_fileLock)
                        {
                            _writer.WriteLine(formatted);
                        }
                    }catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[FileLogSink] 写入日志出错：{ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //任务结束
            }
        }

        /// <summary>
        /// 检查是否需要轮转日志文件
        /// </summary>
        private void RotateIfNeeded()
        {
            var needRotate = false;
            var newPath = _currentLogFilePath;

            if (_rotateByDate)
            {
                var todayFile = GetLogFilePath();
                if(!string.Equals(todayFile, _currentLogFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    needRotate = true;
                    newPath = todayFile;
                }
            }
            else
            {
                var fileInfo = new FileInfo(_currentLogFilePath);
                if(fileInfo.Exists && fileInfo.Length > _maxFileSizeBytes)
                {
                    needRotate= true;
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    newPath = Path.Combine(_logDirectory, $"{_fileNamePrefix}_{timestamp}.log");
                }
            }

            if (needRotate)
            {
                lock(_fileLock)
                {
                    _writer?.Close();
                    _currentLogFilePath = newPath;
                    _writer = new StreamWriter(new FileStream(_currentLogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                }
            }
        }

        /// <summary>
        /// 生成当前日志文件路径
        /// 如果启动按日期轮转，则文件名包含日期
        /// </summary>
        private string GetLogFilePath()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var fileName = _rotateByDate ? $"{_fileNamePrefix}_{date}.log" : $"{_fileNamePrefix}.log";
            return Path.Combine(_logDirectory, fileName);
        }

        /// <summary>
        /// 关闭文件写入器并释放资源
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            _queue.CompleteAdding();
            _cts.Cancel();
            try
            {
                _workerTask.Wait(1000);
            }
            catch { /*忽略等待异常*/ }

            lock (_fileLock)
            {
                _writer?.Close();
                _writer?.Dispose();
            }
            _cts.Dispose();
        }
    }
}
