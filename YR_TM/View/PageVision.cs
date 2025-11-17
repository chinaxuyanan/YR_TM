using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_TM.Utils;

namespace YR_TM.View
{
    public partial class PageVision : UserControl
    {
        private Button btnConnectCam, btnCapture, btnContiunous, btnSave;
        private PictureBox picDisplay;
        private Button btnLightToggle;
        private TrackBar trackLight;
        private Label lblLightValue, lblStatus;
        private bool lightOn = false;

        private Button btnUp, btnDown, btnLeft, btnRight;
        private NumericUpDown nudStep;
        private Label lblStep;

        public PageVision()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 45);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            //左侧功能区
            var panelLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(5),
                AutoScroll = true
            };

            // --- 相机控制 ---
            var groupCam = CreateGroupBox(LanguageManager.GetString("CameraControl_Text"));
            var flowCam = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                AutoSize = true
            };
            btnConnectCam = CreateButton(LanguageManager.GetString("Connect_Text"));
            btnCapture = CreateButton(LanguageManager.GetString("Photo"));
            btnContiunous = CreateButton(LanguageManager.GetString("Continuou_Text"));
            btnSave = CreateButton(LanguageManager.GetString("Btn_Save"));
            flowCam.Controls.AddRange(new Control[] { btnConnectCam, btnCapture, btnContiunous, btnSave });
            groupCam.Controls.Add(flowCam);

            //--- 光源控制 ---
            var groupLight = CreateGroupBox(LanguageManager.GetString("LightControl_Text"));
            var panelLight = new Panel { Dock = DockStyle.Fill, Height = 80, AutoSize = true };

            btnLightToggle = CreateButton(LanguageManager.GetString("On"), 60);
            btnLightToggle.Location = new Point(15, 16);
            btnLightToggle.Click += (s, e) =>
            {
                lightOn = !lightOn;
                btnLightToggle.Text = lightOn ? LanguageManager.GetString("Off") : LanguageManager.GetString("On");
                btnLightToggle.BackColor = lightOn ? Color.FromArgb(0, 120, 215) : Color.FromArgb(70, 70, 70);
            };

            trackLight = new TrackBar
            {
                Minimum = 0,
                Maximum = 255,
                TickStyle = TickStyle.None,
                Value = 128,
                Width = 170,
                Location = new Point(90, 20)
            };

            lblLightValue = new Label
            {
                Text = $"{LanguageManager.GetString("Brightness")} 128",
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(260, 20)
            };
            trackLight.Scroll += (s, e) => lblLightValue.Text = $"{LanguageManager.GetString("Brightness")} {trackLight.Value}";

            panelLight.Controls.AddRange(new Control[] {btnLightToggle, trackLight, lblLightValue});
            groupLight.Controls.Add(panelLight);

            // --- 点位调整 ---
            var groupJog = CreateGroupBox(LanguageManager.GetString("PointAdjustment"));
            var panelJog = new Panel { Dock = DockStyle.Fill, Height = 110, AutoSize = true };

            btnUp = CreateButton("↑", 45);
            btnDown = CreateButton("↓", 45);
            btnLeft = CreateButton("←", 45);
            btnRight = CreateButton("→", 45);

            btnUp.Location = new Point(250, 10);
            btnLeft.Location = new Point(205, 45);
            btnRight.Location = new Point(295, 45);
            btnDown.Location = new Point(250, 80);

            lblStep = new Label
            {
                Text = $"{LanguageManager.GetString("Step")}(mm):",
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                AutoSize = true,
                Location = new Point(15, 30)
            };

            nudStep = new NumericUpDown
            {
                Minimum = 0.01M,
                Maximum = 10,
                DecimalPlaces = 2,
                Increment = 0.1M,
                Value = 1.00M,
                Width = 60,
                Location = new Point(85, 45)
            };

            panelJog.Controls.AddRange(new Control[] { btnUp, btnLeft, btnRight, btnDown, lblStep, nudStep });
            groupJog.Controls.Add(panelJog);

            panelLeft.Controls.Add(groupCam);
            panelLeft.Controls.Add(groupLight);
            panelLeft.Controls.Add(groupJog);

            //右侧图像显示区
            var panelRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            picDisplay = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelRight.Controls.Add(picDisplay);

            lblStatus = new Label
            {
                Text = $"{LanguageManager.GetString("State_Text")}：未连接",
                ForeColor = Color.Orange,
                Font = new Font("微软雅黑", 9),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelRight.Controls.Add(lblStatus);

            mainLayout.Controls.Add(panelLeft, 0, 0);
            mainLayout.Controls.Add(panelRight, 1, 0);

            this.Controls.Add(mainLayout);

            //运动接口
            btnUp.Click += (s, e) => MoveAxis("Y", (double)nudStep.Value);
            btnDown.Click += (s, e) => MoveAxis("Y", -(double)nudStep.Value);
            btnLeft.Click += (s, e) => MoveAxis("X", -(double)nudStep.Value);
            btnRight.Click += (s, e) => MoveAxis("X", (double)nudStep.Value);
        }

        private void MoveAxis(string axis, double dist)
        {
            //调用运动接口
            lblStatus.Text = $"移动 {axis} 轴 {dist} mm";
            lblStatus.ForeColor = Color.LightGreen;
        }

        private GroupBox CreateGroupBox(string text)
        {
            return new GroupBox
            {
                Text = text,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(69, 139, 116),
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Width = 500,
                Height = 150,
                //AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(5)
            };
        }

        private Button CreateButton(string text, int width = 100)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 70),
                Margin = new Padding(5)
            };
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(100, 100, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 70);

            return btn;
        }
    }
}
