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
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_TM.Utils;
using YR_Framework.Events;
using YR_Framework.Models;
using YR_TM.Manager;
using YR_TM.Modules;

namespace YR_TM.View
{
    public partial class PageMain : UserControl
    {
        private ILogger logger = LogManager.GetLogger("PageMain");

        private SplitContainer splitContainer;
        private Panel panelLog;
        private Panel panelAlarm;
        private RichTextBox txtLog;
        private List<PictureBox> cameraPics = new List<PictureBox>();

        private Label lblAxis;

        private DataGridView alarmDataGridView;

        //底部状态栏
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus, lblRunMode, lblCurrentTime;
        private Timer timer;
        private Timer axisTimer;

        public PageMain()
        {
            InitializeComponent();
            InitializeUI();

            this.HandleCreated += PageMain_HandleCreated;

            lblStatus.Text = AppState.IsBusConnected ? LanguageManager.GetString("Lbl_Status_Connect") : LanguageManager.GetString("Lbl_Status_Not_Connect");
            lblStatus.ForeColor = AppState.IsBusConnected ? Color.LimeGreen : Color.Red;

            //TestManager.Instance.ConnectBusChanged += OnConnectBusChanged;

            EventCenter.Subscribe<RunModeChangedEvent>((evt) =>
            {
                OnRunModeChangedFromTopMenu(evt.CurrentRunMode);
            }, this);

            LogManager.AddSink(new UILogSink(txtLog, new SimpleFormatter()));

            timer = new Timer { Interval = 1000 };
            timer.Tick += Timer_Tick;
            timer.Start();

            axisTimer = new Timer { Interval = 50 };
            axisTimer.Tick += AxisTimer_Tick;
            axisTimer.Start();
        }

        private void AxisTimer_Tick(object sender, EventArgs e)
        {
            double axisX = 1; /*MotionModule.Instance.GetAixsPos(0);*/
            double axisY = 2.4; /*MotionModule.Instance.GetAixsPos(1);*/
            double axisZ = 3.6; /*MotionModule.Instance.GetAixsPos(2);*/
            lblAxis.Text = $"AxisX: {axisX:F3}\nAxisY: {axisY:F3}\nAxisZ: {axisZ:F3}\nSN: 123456789";
        }

        private void OnConnectBusChanged(bool isConnected)
        {
            lblStatus.Text = isConnected ? LanguageManager.GetString("Lbl_Status_Connect") : LanguageManager.GetString("Lbl_Status_Not_Connect");
            lblStatus.ForeColor = isConnected ? Color.LimeGreen : Color.Red;
        }

        private void OnRunModeChangedFromTopMenu(object obj)
        {
            if(obj is RunMode runMode)
            {
                lblRunMode.Text = $"{LanguageManager.GetString("Run_Mode")} {runMode}";
                AppState.CurrentRunMode = runMode;
                TestManager.Mode = runMode;
            }
        }

        private void PageMain_HandleCreated(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                InitCameraPanels(1);
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lblCurrentTime.Text = $"{LanguageManager.GetString("Lbl_Time")}：{DateTime.Now:yyyy/yy/dd-HH:mm:ss}";
        }

        private void InitializeUI()
        {
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 600,
                IsSplitterFixed = false,
                BackColor = Color.FromArgb(40, 44, 52)
            };
            this.Controls.Add(splitContainer);
            this.Controls.SetChildIndex(splitContainer, 0);

            this.splitContainer.Panel1.SizeChanged += Panel1_SizeChanged;

            panelLog = new Panel
            {
                Dock = DockStyle.Top,
                Height = 600,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            splitContainer.Panel2.Controls.Add(panelLog);

            txtLog = new RichTextBox
            {
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(105, 105, 105),
                BorderStyle = BorderStyle.None
            };

            panelLog.Controls.Add(txtLog);

            var lblLog = new Label
            {
                Text = "Log Info",
                Height = 20,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 8)
            };
            panelLog.Controls.Add(lblLog);

            panelAlarm = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 300,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            splitContainer.Panel2.Controls.Add(panelAlarm);

            alarmDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                BackgroundColor = Color.FromArgb(105, 105, 105),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };
            alarmDataGridView.Columns.Add("Time", $"{LanguageManager.GetString("Lbl_Time")}");
            alarmDataGridView.Columns.Add("Message", $"{LanguageManager.GetString("Alarm_Info")}");
            panelAlarm.Controls.Add(alarmDataGridView);

            var lblAlarm = new Label
            {
                Text = "Alarm Info",
                Height = 20,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 8)
            };
            panelAlarm.Controls.Add(lblAlarm);

            //主界面底部状态栏
            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(105, 139, 105),
            };
            lblStatus = new ToolStripStatusLabel(LanguageManager.GetString("Lbl_Status_Not_Connect"));
            lblRunMode = new ToolStripStatusLabel($"{LanguageManager.GetString("Run_Mode")} {AppState.CurrentRunMode}");
            lblCurrentTime = new ToolStripStatusLabel($"{LanguageManager.GetString("Lbl_Time")}：{DateTime.Now:yyyy/yy/dd-HH:mm:ss}");
            lblRunMode.Spring = true;
            lblCurrentTime.Alignment = ToolStripItemAlignment.Right;
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblRunMode, lblCurrentTime });
            this.Controls.Add(statusStrip);
        }

        private void Panel1_SizeChanged(object sender, EventArgs e)
        {
            ResizeCameraPanels();
        }

        private void ResizeCameraPanels()
        {
            int camCount = splitContainer.Panel1.Controls.OfType<Panel>().Count();
            if (camCount == 0) return;

            int panelWidth = splitContainer.Panel1.ClientSize.Width;
            int panelHeight = splitContainer.Panel1.ClientSize.Height;

            int cols = (int)Math.Ceiling(Math.Sqrt(camCount));
            int rows = (int)Math.Ceiling((double)camCount / cols);

            int eachWidth = panelWidth / cols;
            int eachHeight = panelHeight / rows;

            var panels = splitContainer.Panel1.Controls.OfType<Panel>().OrderBy(p => p.Name).ToList();

            for (int i = 0; i < panels.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                var p = panels[i];
                p.SuspendLayout();
                p.Left = col * eachWidth;
                p.Top = row * eachHeight;
                p.Width = eachWidth;
                p.Height = eachHeight;
                p.ResumeLayout();
            }
        }

        private void InitCameraPanels(int cameraCount)
        {
            splitContainer.Panel1.Controls.Clear();
            cameraPics.Clear();

            int panelWidth = splitContainer.Panel1.ClientSize.Width;
            int panelHeight = splitContainer.Panel1.ClientSize.Height;

            int cols = (int)Math.Ceiling(Math.Sqrt(cameraCount));
            int rows = (int)Math.Ceiling((double)cameraCount / cols);

            int eachWidth = panelWidth / cols;
            int eachHeight = panelHeight / rows;

            for (int i = 0; i < cameraCount; i++)
            {
                int row = i / cols;
                int col = i % rows;

                Panel camPanel = new Panel
                {
                    Width = eachWidth,
                    Height = eachHeight,
                    Left = col * eachWidth,
                    Top = row * eachHeight,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(40, 40, 40),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left,
                    Padding = new Padding(10, 10, 10, 10)
                };

                PictureBox pic = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    SizeMode = PictureBoxSizeMode.Zoom,
                };

                lblAxis = new Label
                {
                    Dock = DockStyle.Bottom,
                    Height = 80,
                    Text = $"AxisX: 0.000\nAxisY: 0.000\nAxisZ: 0.000\nSN: 123456789",
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.White,
                    Font = new Font("微软雅黑", 10, FontStyle.Regular),
                    BackColor = Color.FromArgb(60, 60, 60)
                };

                camPanel.Controls.Add(lblAxis);
                camPanel.Controls.Add(pic);

                splitContainer.Panel1.Controls.Add(camPanel);
                cameraPics.Add(pic);
            }
        }

        ///<summary>
        ///外部调用这个方法更新相机图片
        /// </summary>
        public void UpdateCameraImage(int camIndex, Image img)
        {
            if(camIndex < 0 || camIndex >= cameraPics.Count) return;
            cameraPics[camIndex].Image = img;
        }
    }
}
