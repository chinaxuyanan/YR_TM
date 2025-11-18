using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_Framework.Events;
using YR_Framework.Models;

namespace YR_Framework.UI
{
    public abstract partial class BaseMainForm : Form
    {
        protected TopMenuControl topMenu;
        protected Panel mainPanel;

        public BaseMainForm()
        {
            InitializeBaseUI();
            RegisterEvents();
        }

        private void InitializeBaseUI()
        {
            //窗体基础属性
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 50, 55);
            this.MinimumSize = new Size(1000, 750);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Text = $"YR - Platform  Ver.{FrameworkContext.FrameworkVersion}";

            //主内容区
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 65, 70),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(mainPanel);

            //顶部菜单栏控件
            topMenu = new TopMenuControl
            {
                Dock = DockStyle.Top,
                Height = 85
            };
            this.Controls.Add(topMenu);
        }

        private void RegisterEvents()
        {
            //订阅菜单点击事件（事件中心发布)
            EventCenter.Subscribe<TopMenuChangedEvent>((evt) =>
            {
                if (evt == null) return;

                if (this.InvokeRequired)
                    this.Invoke(new Action(() => OnMenuClicked(evt.CurrentTopMenu)));
                else
                    OnMenuClicked(evt.CurrentTopMenu);
            }, this);

            //用户登录事件
            EventCenter.Subscribe<UserChangedEvent>((evt) =>
            {
                if (evt == null) return;

                if (this.InvokeRequired)
                    this.Invoke(new Action(() => OnUserChanged(evt.CurrentUser.ToString())));
                else
                    OnUserChanged(evt.CurrentUser.ToString());
            }, this);

            EventCenter.Subscribe<RunStateChangedEvent>((evt) =>
            {
                if (evt == null) return;

                if (this.InvokeRequired)
                    this.Invoke(new Action(() => UpdateRunStateLabel(evt.CurrentRunState)));
                else
                    UpdateRunStateLabel(evt.CurrentRunState);
            }, this);
        }

        private void UpdateRunStateLabel(RunState currentRunState)
        {
            topMenu?.SetRunState(currentRunState);
        }

        ///<summary>
        ///菜单点击回调，由子类实现
        /// </summary>
        /// <param name="menuName">菜单名称</param>
        protected abstract void OnMenuClicked(string menuName);

        ///<summary>
        ///用户切换时回调，由子类决定是否限制界面
        /// </summary>
        protected virtual void OnUserChanged(string userName){ }

        ///<summary>
        ///提供一个切换内容显示的方法
        /// </summary>
        protected void ShowControl(UserControl control)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<UserControl>(ShowControl), control);
                return;
            }
            
            //隐藏旧控件
            if(mainPanel.Controls.Count > 0)
            {
                mainPanel.Controls[0].Visible = false;
            }

            //mainPanel.Controls.Clear();
            if(control != null)
            {
                control.Dock = DockStyle.Fill;
                mainPanel.Controls.Clear();
                mainPanel.Controls.Add(control);
                control.Visible = true;
            }
        }
    }
}
