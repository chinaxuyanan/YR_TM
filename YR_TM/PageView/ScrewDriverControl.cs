using DeviceCommLib.Implementation;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
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

namespace YR_TM.PageView
{
    public partial class ScrewDriverControl : UserControl
    {
        private ComboBox cmbPort, cmbBaud;
        private Button btnConnect, btnStart, btnStop, btnReadTorque, btnReadAngle;
        private Label lblTorque, lblAngle, lblStatus;


        private ElectricScrewdrivers _electricScrewdrivers;

        private bool _ScrewdriverConnectStatus = false;

        public ScrewDriverControl()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.Dock = DockStyle.Fill;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

            // --- 串口设置 ---
            var panelCom = new Panel { Dock = DockStyle.Fill, Height = 90 };
            var lblCom = new Label { Text = LanguageManager.GetString("Serial_Text"), ForeColor = Color.White, AutoSize = true, Location = new Point(10, 20) };
            cmbPort = new ComboBox { Width = 100, Location = new Point(150, 15), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            var lblBaud = new Label { Text = LanguageManager.GetString("Baud_Text"), ForeColor = Color.White, AutoSize = true, Location = new Point(270, 20) };
            cmbBaud = new ComboBox { Width = 100, Location = new Point(370, 15) };
            cmbBaud.Items.AddRange(new[] { "9600", "115200" });
            cmbBaud.SelectedIndex = 0;
            btnConnect = CreateButton(LanguageManager.GetString("Connect_Text"), new Point(500, 10), 100);
            panelCom.Controls.AddRange(new Control[] { lblCom, cmbPort, lblBaud, cmbBaud, btnConnect });
            mainLayout.Controls.Add(panelCom, 0, 0);


            btnConnect.Click += BtnConnect_Click;



            // --- 电批操作区 ---
            var groupOperation = new GroupBox
            {
                Text = LanguageManager.GetString("ScrewdriverOperation_Text"),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };
            var flowOp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };
            btnStart = CreateButton(LanguageManager.GetString("Start_Text"), Point.Empty);
            btnStop = CreateButton(LanguageManager.GetString("Stop_Text"), Point.Empty);
            btnReadTorque = CreateButton(LanguageManager.GetString("Torque_Text"), Point.Empty);
            btnReadAngle = CreateButton(LanguageManager.GetString("Angle_Text"), Point.Empty);
            flowOp.Controls.AddRange (new Control[] { btnStart, btnStop, btnReadTorque, btnReadAngle });
            groupOperation.Controls.Add(flowOp);
            mainLayout.Controls.Add(groupOperation, 0, 1);

            // --- 状态显示区 ---
            var groupStatus = new GroupBox
            {
                Text = LanguageManager.GetString("State_Text"),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };

            lblTorque = new Label
            {
                Text = $"{LanguageManager.GetString("Torque")}：0.00 N·m",
                ForeColor = Color.LightGreen,
                AutoSize = true,
                Font = new Font("Consolas", 11, FontStyle.Bold),
                Location = new Point(30, 40)
            };

            lblAngle = new Label
            {
                Text = $"{LanguageManager.GetString("Angle")}：0°",
                ForeColor = Color.LightSkyBlue,
                AutoSize = true,
                Font = new Font("Consolas", 11, FontStyle.Bold),
                Location = new Point(200, 40)
            };

            lblStatus = new Label
            {
                Text = LanguageManager.GetString("ScrewStatu_Text"),
                ForeColor = Color.Orange,
                AutoSize = true,
                Font = new Font("微软雅黑", 10, FontStyle.Regular),
                Location = new Point(30, 80)
            };

            groupStatus.Controls.AddRange(new Control[] { lblTorque, lblAngle, lblStatus });
            mainLayout.Controls.Add(groupStatus, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (cmbPort.SelectedIndex == -1 || cmbBaud.SelectedIndex==-1)
            {
                MessageBox.Show(LanguageManager.GetString("PortAndBaudError_NoSelected"), LanguageManager.GetString("Prompt"));
                return;
            }
            if (_electricScrewdrivers == null)
            {
                _electricScrewdrivers = new ElectricScrewdrivers(cmbPort.SelectedItem.ToString(), int.Parse(cmbBaud.SelectedItem.ToString()));

            }

            if (_electricScrewdrivers.ConnectStatus != true)
            {
                if (_electricScrewdrivers.Connect())
                {
                    _ScrewdriverConnectStatus = true;
                }
                else
                {
                    _ScrewdriverConnectStatus = false;
                }
            }
            else
            {
                _ScrewdriverConnectStatus = false;
            }
            //更新连接按钮装
            if (_ScrewdriverConnectStatus == true)
                    btnConnect.Text = LanguageManager.GetString("DisConnect_Text");
            else
            {
                btnConnect.Text = LanguageManager.GetString("Connect_Text");
            }

        }

        private Button CreateButton(string text, Point location, int width = 120)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 70),
                Margin = new Padding(10),
                Location = location
            };
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(100, 100, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 70);

            return btn;
        }
    }
}
