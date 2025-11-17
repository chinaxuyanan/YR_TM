using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_Framework.Events;
using YR_Framework.Models;

namespace YR_Framework.UI
{
    /// <summary>
    /// 顶部栏：主界面、调试、视觉、报警、Log、用户、运行模式等按钮
    /// </summary>
    public partial class TopMenuControl : UserControl
    {
        //运行模式状态
        private Label lblRunState;
        private Label lblStationName;

        private Button btnMain;
        private Button btnDebug;
        private Button btnVision;
        private Button btnAlarm;
        private Button btnFolder;
        private Button btnLog;
        private Button btnUser;

        private Dictionary<string, Button> menuButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        List<Button> topButtons = new List<Button>();
        private Button currentSelectedButton = null;

        public TopMenuControl()
        {
            InitializeComponent();
            CreateTopMenu();

            SetButtonSelected(btnMain);
            UpdateMenuAccess(AppState.CurrentUser);

            EventCenter.Subscribe<TopMenuChangedEvent>((evt) =>
            {
                if (evt.CurrentTopMenu == "Reset")
                {
                    btnUser.Image = ResizeImage(Properties.Resources.Login_Off, 55);
                    UpdateMenuAccess(UserLevel.Operator);
                }
            });
        }

        private void CreateTopMenu()
        {
            this.Height = 80;
            this.Dock = DockStyle.Top;
            this.BackColor = Color.FromArgb(40, 45, 50);
            this.BorderStyle = BorderStyle.FixedSingle;

            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(40, 45, 50)
            };
            this.Controls.Add(mainPanel);

            btnMain = CreateButton("主界面", ResizeImage(Properties.Resources.Home_Off, 55), ResizeImage(Properties.Resources.Home_on, 55));
            btnDebug = CreateButton("调试界面", ResizeImage(Properties.Resources.Mannual_Off, 55), ResizeImage(Properties.Resources.Mannual_On, 55));
            btnVision = CreateButton("视觉界面", ResizeImage(Properties.Resources.Vision_Off, 55), ResizeImage(Properties.Resources.Vision_On, 55));
            btnAlarm = CreateButton("报警界面", ResizeImage(Properties.Resources.Alarm_Off, 55), ResizeImage(Properties.Resources.Alarm_On, 55));

            mainPanel.Controls.Add(btnMain);
            mainPanel.Controls.Add(btnDebug);
            mainPanel.Controls.Add(btnVision);
            mainPanel.Controls.Add(btnAlarm);

            //中间：工站名
            lblStationName = new Label
            {
                Text = FrameworkContext.StationName,
                Width = 120,
                Height = 60,
                ForeColor = Color.LightGreen,
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Margin = new Padding(10, 10, 10, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
                
            };
            mainPanel.Controls.Add(lblStationName);

            btnFolder = CreateButton("文件夹", ResizeImage(Properties.Resources.File_Off, 55), ResizeImage(Properties.Resources.File_On, 55));
            btnLog = CreateButton("Log", ResizeImage(Properties.Resources.Log_Off, 55), ResizeImage(Properties.Resources.Log_On, 55));

            btnUser = new Button
            {
                Width = 60,
                Height = 60,
                Margin = new Padding(10),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                ImageAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Image = ResizeImage(Properties.Resources.Login_Off, 55)
            };
            btnUser.Click += BtnUser_Click;

            mainPanel.Controls.Add(btnFolder);
            mainPanel.Controls.Add(btnLog);
            mainPanel.Controls.Add(btnUser);

            //右侧：用户 & 运行模式
            var rightPanel = new Panel
            {
                Width = 150,
                Height = 60,
                Margin = new Padding(10)
            };

            lblRunState = new Label
            {
                Text = FrameworkContext.RunState,
                Width = 140,
                Height = 60,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.LightGray,
                Font = new Font("微软雅黑", 18, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblRunState);

            mainPanel.Controls.Add(rightPanel);
        }

        private void BtnUser_Click(object sender, EventArgs e)
        {
            using (var loginForm = new UserLoginForm(AppState.CurrentUser, AppState.CurrentRunMode))
            {
                if(loginForm.ShowDialog() == DialogResult.OK)
                {
                    AppState.CurrentUser = loginForm.SelectedUser;
                    AppState.CurrentRunMode = loginForm.SelectedRunMode;

                    //更新图标
                    switch (AppState.CurrentUser)
                    {
                        case UserLevel.Operator: btnUser.Image = ResizeImage(Properties.Resources.Login_On, 55); break;
                        case UserLevel.Engineer: btnUser.Image = ResizeImage(Properties.Resources.Login_On, 55); break;
                        case UserLevel.Admin: btnUser.Image = ResizeImage(Properties.Resources.Login_On, 55); break;
                    }

                    //权限控制
                    UpdateMenuAccess(AppState.CurrentUser);

                    //发布事件
                    EventCenter.Publish(new UserChangedEvent { CurrentUser = AppState.CurrentUser });
                    EventCenter.Publish(new  RunModeChangedEvent { CurrentRunMode = AppState.CurrentRunMode });
                }
            }
        }

        private Image ResizeImage(Image img, int size = 24)
        {
            return new Bitmap(img, new Size(size, size));
        }

        private Button CreateButton(string toolTipText, Image imgNormal = null, Image imgSelected = null)
        {
            var btn = new Button
            {
                Width = 60,
                Height = 60,
                Margin = new Padding(10),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                ImageAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Image = imgNormal,
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Tag = new Image[] { imgNormal, imgSelected };

            //鼠标悬停时显示提示
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(btn, toolTipText);

            btn.Click += (s, e) =>
            {
                SetButtonSelected(btn);
                EventCenter.Publish(new TopMenuChangedEvent { CurrentTopMenu = toolTipText });
            };
            topButtons.Add(btn);

            if(!menuButtons.ContainsKey(toolTipText))
                menuButtons.Add(toolTipText, btn);
            else
                menuButtons[toolTipText] = btn;

            return btn;
        }

        private void SetButtonSelected(Button selectedBtn)
        {
            if (selectedBtn == currentSelectedButton) return;

            currentSelectedButton = selectedBtn;

            foreach (var btn in topButtons)
            {
                if (btn.Tag is Image[] imgs)
                    //img[0] = 灰色 imgs[1] = 蓝色
                    btn.Image = (btn == selectedBtn) ? imgs[1] : imgs[0];
            }
        }

        public void SetRunState(RunState runState)
        {
            if (lblRunState == null) return;

            lblRunState.Text = runState.ToString();

            switch (runState)
            {
                case RunState.Idle: lblRunState.ForeColor = Color.LightGray; break;
                case RunState.Ready: lblRunState.ForeColor = Color.Orange; break;
                case RunState.Running:
                    lblRunState.ForeColor = Color.Yellow;
                    btnUser.Enabled = false;
                    break;
                case RunState.PASS:
                    lblRunState.ForeColor = Color.LimeGreen;
                    btnUser.Enabled = false;
                    break;
                case RunState.Stop:
                    lblRunState.ForeColor = Color.Red;
                    btnUser.Enabled = false;
                    break;
                case RunState.EmerStop:
                    lblRunState.ForeColor = Color.Red;
                    btnUser.Enabled = false;
                    break;
                case RunState.FAIL:
                    lblRunState.ForeColor = Color.Red;
                    btnUser.Enabled = false;
                    break;
                default: lblRunState.ForeColor = Color.White; break;
            }
        }

        public void SetStationName(string stationName)
        {
            lblStationName.Text = stationName;
        }

        //根据用户权限更新菜单可用状态
        public void UpdateMenuAccess(UserLevel userLevel)
        {
            //先全部禁用（默认灰色）
            foreach (var btn in topButtons)
            {
                btn.Enabled = false;

                if(btn.Tag is Image[] imgs && imgs.Length >= 1)
                    btn.Image = imgs[0];
            }

            //按用户等级开启对应权限
            switch (userLevel)
            {
                case UserLevel.Operator: btnMain.Enabled = true; break;
                case UserLevel.Engineer:
                    btnMain.Enabled = true;
                    btnDebug.Enabled = true;
                    btnVision.Enabled = true;
                    btnAlarm.Enabled = true;
                    break;
                case UserLevel.Admin:
                    btnMain.Enabled = true;
                    btnDebug.Enabled = true;
                    btnVision.Enabled = true;
                    btnAlarm.Enabled = true;
                    btnFolder.Enabled = true;
                    btnLog.Enabled = true;
                    break;
            }

            currentSelectedButton = null;
            SetButtonSelected(btnMain);
        }
    }
}
