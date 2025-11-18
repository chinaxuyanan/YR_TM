using Logger;
using Logger.Formatters;
using Logger.Sinks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_Framework.UI;
using YR_Framework.Models;
using YR_TM.View;
using YR_Framework.Events;
using YR_TM.Modules;
using YR_TM.Utils;
using YR_TM.Manager;
using System.Net.Configuration;

namespace YR_TM
{
    public partial class MainForm : BaseMainForm
    {
        private ILogger logger = LogManager.GetLogger("MainForm");

        public TopMenuControl TopMenuControl => topMenu;

        public MainForm()
        {
            InitializeComponent();
            ShowControl(new PageMain());

            LanguageManager.ApplyResources(this); //应用语言资源

            //订阅语言改变事件，动态刷新UI
            LanguageManager.LanguageChanged += OnLanguageChanged;
            TestManager.Instance.StateChanged += OnStateChanged;

            LogManager.AddSink(new FileLogSink(new SimpleFormatter(), "Logs", "YR_Test_Log"));

            TestManager.Instance.InitializeAndReset();
        }

        protected override void OnMenuClicked(string menuName)
        {
            switch (menuName)
            {
                case "主界面":
                    ShowControl(new PageMain());
                    break;
                case "调试界面":
                    ShowControl(new PageDebug());
                    break;
                case "视觉界面":
                    ShowControl(new PageVision());
                    break;
                case "报警界面":
                    ShowControl(new PageAlarm());
                    break;
                case "文件夹":
                    ShowControl(new PageFile());
                    break;
                case "Log":
                    ShowControl(new PageLog());
                    break;
            }
        }

        protected override void OnUserChanged(string userName)
        {
            ShowControl(new PageMain());
        }

        private void OnStateChanged(RunState state)
        {
            SafeInvoke(() =>
            {
                EventCenter.Publish(new RunStateChangedEvent { CurrentRunState = state });
            });
        }

        private void OnLanguageChanged()
        {
            SafeInvoke(() =>
            {
                LanguageManager.ApplyResources(this);
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            //防止误关闭，在运行状态下不允许关闭
            if (AppState.CurrentRunState == RunState.Running)
            {
                var result = MessageBox.Show("系统运行中，确定要退出吗？", "退出确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            MotionModule.Instance.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnsubscribeEvents();

            base.OnFormClosed(e);
        }

        private void UnsubscribeEvents()
        {
            try
            {
                LanguageManager.LanguageChanged -= OnLanguageChanged;

                if (TestManager.Instance != null)
                {
                    TestManager.Instance.StateChanged -= OnStateChanged;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"取消事件订阅时发生错误：{ex.Message}");
            }
        }

        private void SafeInvoke(Action action)
        {
            if (this.IsDisposed || this.Disposing) return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    if(!this.IsDisposed && !this.Disposing)
                        action();
                }));
            }
            else
            {
                action();
            }
        }

    }
}
