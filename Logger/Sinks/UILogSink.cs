using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logger.Sinks
{
    /// <summary>
    /// UI 日志输出器
    /// 可将日志输出到指定的TextBox、RichTextBox或ListBox上
    /// 自动处理跨线程 UI 调用
    /// </summary>
    public class UILogSink : ILogSink
    {
        private readonly Control _uiControl;
        private readonly ILogFormatter _formatter;
        private readonly SynchronizationContext _uiContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="control">要显示的UI控件（如 TextBox、RichTextBox、ListBox）</param>
        /// <param name="formatter">日志格式化器</param>
        public UILogSink(Control control, ILogFormatter formatter)
        {
            _uiControl = control ?? throw new ArgumentNullException(nameof(control));
            _formatter = formatter;
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        /// <summary>
        /// 同步写入日志到UI
        /// 实际上内部仍通过 UI 线程安全调用
        /// </summary>
        /// <param name="message">日志消息对象</param>
        public void Write(LogMessage message)
        {
            var formatted = _formatter.Format(message);
            AppendToUI(formatted);
        }

        /// <summary>
        /// 异步写入日志到UI
        /// </summary>
        /// <param name="message">日志消息对象</param>
        public Task WriteAsync(LogMessage message)
        {
            Write(message);
            return Task.CompletedTask;
        }

        ///<summary>
        ///向UI控件追加日志内容
        ///根据控件类型自动选择更新方式
        /// </summary>
        private void AppendToUI(string text)
        {
            if (_uiControl == null || _uiControl.IsDisposed) return;

            //使用UI同步上下文调用
            _uiContext.Post(_ =>
            {
                try
                {
                    switch (_uiControl)
                    {
                        case TextBox tb: tb.AppendText(text + Environment.NewLine); break;
                        case RichTextBox rtb: rtb.AppendText(text + Environment.NewLine); break;
                        case ListBox lb: lb.Items.Add(text); lb.TopIndex = lb.Items.Count - 1; break;
                        default: _uiControl.Text += text + Environment.NewLine; break;
                    }
                }catch (Exception ex)
                {
                    Console.Error.WriteLine($"[UILogSink] UI 更新失败：{ex.Message}");
                }
            }, null);
        }
    }
}
