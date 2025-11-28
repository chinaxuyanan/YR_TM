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
using static System.Windows.Forms.AxHost;

namespace YR_TM.PageView
{
    public partial class IOControl : UserControl
    {
        private DataGridView dgvInput;
        private FlowLayoutPanel panelOutput;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblRefresh, lblAuto, lblConn;

        private IOConfig _ioConfig;

        public IOControl()
        {
            InitializeComponent();
            LoadIOConfig();
            InitializeUI();
        }

        private void LoadIOConfig()
        {
            //_ioConfig = FileHelper.ReadIOExcel(new IOFilePath().TAMFilePath);
            _ioConfig = IO_ConfigHelper.GetIOEnumConfig(); 

        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);

            // === splitcontainer: 左输入/右输出 ===
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = this.Width / 2,
                Orientation = Orientation.Vertical,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            this.Controls.Add(split);

            // --- 左侧输入区 ---
            dgvInput = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.Black,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(70, 70, 70)
            };
            dgvInput.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
            dgvInput.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvInput.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
            dgvInput.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvInput.Columns.Add("Address", LanguageManager.GetString("SignalAddressInput_Text"));
            dgvInput.Columns.Add("Signal", LanguageManager.GetString("Signal_Text"));
            dgvInput.Columns.Add("State", LanguageManager.GetString("State_Text"));
            
            dgvInput.Columns[0].Width = 100;
            dgvInput.Columns[1].Width = 300;
            dgvInput.Columns[2].Width = 200;
            split.Panel1.Controls.Add(dgvInput);

            foreach (var input in _ioConfig.Inputs)
            {
                if (!string.IsNullOrEmpty(input.Name))
                    //dgvInput.Rows.Add(input.Name, (input.State ? "ON" : "OFF"));
                    dgvInput.Rows.Add(input.Address,input.Name, (input.State ? "ON" : "OFF"));
            }

            // --- 右侧输出区 ---
            panelOutput = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(40, 40, 40)
            };
            split.Panel2.Controls.Add(panelOutput);

            foreach (var output in _ioConfig.Outputs)
            {
                if (!string.IsNullOrEmpty(output.Name))
                    panelOutput.Controls.Add(CreateIOButton($"{output.Address}:{output.Name}"));
            }

            // --- 底部状态栏 ---
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(105, 139, 105),
                SizingGrip = false
            };

            lblRefresh = new ToolStripStatusLabel($"IO{LanguageManager.GetString("State_Text")}") { IsLink = true, LinkColor = Color.DeepSkyBlue };
            lblRefresh.Click += (s, e) => RefreshIOStatus();

            lblAuto = new ToolStripStatusLabel(LanguageManager.GetString("AutoRefresh_Text")) { ForeColor = Color.LightGray };

            lblConn = new ToolStripStatusLabel(LanguageManager.GetString("IOConnect_Text")) { Alignment = ToolStripItemAlignment.Right, ForeColor = Color.LightGreen };

            statusStrip.Items.Add(lblRefresh);
            statusStrip.Items.Add(lblAuto);
            statusStrip.Items.Add(lblConn);

            this.Controls.Add(statusStrip);
            statusStrip.BringToFront();
        }

        private void RefreshIOStatus()
        {
            //TODO: 实际从EtherCat总线读取输入状态
            foreach (DataGridViewRow row in dgvInput.Rows)
            {
                string state = (row.Cells[2].Value?.ToString() == "ON") ? "OFF" : "ON";
                row.Cells[2].Value = state;
            }
        }

        private Control CreateIOButton(string name)
        {
            var btn = new Button
            {
                Text = $"{name} OFF",
                Width = 240,
                Height = 40,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8)
            };
            btn.FlatAppearance.BorderColor = Color.Gray;

            btn.Click += (s, e) =>
            {
                bool isOn = btn.BackColor == Color.FromArgb(80, 80, 80);
                btn.BackColor = isOn ? Color.FromArgb(0, 160, 0) : Color.FromArgb(80, 80, 80);
                btn.Text = $"{name} {(isOn ? "ON" : "OFF")}";

                //TODO: 写IO输出信号
            };
            return btn;
        }


    }
}
