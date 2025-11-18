using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_TM.Utils;
using YR_TM.Modules;

namespace YR_TM.PageView
{
    public partial class MotionControl : UserControl
    {
        private DataGridView dgvPoints;
        private FlowLayoutPanel flowPanel;
        private Button btnRefresh;
        private ComboBox cmbDevice;

        public MotionControl()
        {
            InitializeComponent();
            InitUI();
            LoadConfig();
        }

        private void InitUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);

            //设备选择栏
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(50, 50, 50)
            };

            Label lblDevice = new Label
            {
                Text = LanguageManager.GetString("lblDevice_Text"),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 13)
            };

            cmbDevice = new ComboBox
            {
                Location = new Point(120, 10),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            cmbDevice.Items.AddRange(new object[] { "TAM", "MAG" });
            cmbDevice.SelectedIndex = 0;
            cmbDevice.SelectedIndexChanged += (s, e) => LoadConfig();

            panelTop.Controls.Add(lblDevice);
            panelTop.Controls.Add(cmbDevice);

            //datagridview配置区
            dgvPoints = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = true,
                BackgroundColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.Black,
                GridColor = Color.Gray,
                BorderStyle = BorderStyle.None
            };
            dgvPoints.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
            dgvPoints.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPoints.EnableHeadersVisualStyles = false;
            dgvPoints.RowHeadersVisible = false;

            dgvPoints.Columns.Add("Name", LanguageManager.GetString("Point_Text"));

            MotionModule.Instance.m_tResoures.AxisNum = 6;
            dgvPoints.Columns.Add(CreateAxisCombo("X", new[] { LanguageManager.GetString("DispensingX_Text"), LanguageManager.GetString("CameraX_Text") }));
            dgvPoints.Columns.Add(CreateAxisCombo("Y", new[] { LanguageManager.GetString("HodlerY_Text") }));
            dgvPoints.Columns.Add(CreateAxisCombo("Z", new[] { LanguageManager.GetString("DispensingZ_Text"), LanguageManager.GetString("CameraZ_Text") }));
            dgvPoints.Columns.Add(CreateAxisCombo("R", new[] { LanguageManager.GetString("RAxis_Text") }));

            dgvPoints.Columns.Add("XValue", LanguageManager.GetString("XAxis"));
            dgvPoints.Columns.Add("YValue", LanguageManager.GetString("YAxis"));
            dgvPoints.Columns.Add("ZValue", LanguageManager.GetString("ZAxis"));
            dgvPoints.Columns.Add("RValue", LanguageManager.GetString("RAxis"));

            //刷新控件按钮
            btnRefresh = new Button
            {
                Text = LanguageManager.GetString("Btn_Refresh"),
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(70, 120, 120),
                ForeColor = Color.White
            };
            btnRefresh.Click += BtnRefresh_Click;

            //动态生成控件区
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(35, 35, 35),
            };

            this.Controls.Add(flowPanel);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(dgvPoints);
            this.Controls.Add(panelTop);
        }

        private class ComboOption
        {
            public string Value { get; set; }
            public string Text {  get; set; }
        }

        private DataGridViewComboBoxColumn CreateAxisCombo(string name, string[] axes)
        {
            var options = axes.Select(a => new ComboOption { Value = a, Text = a }).ToList();

            return new DataGridViewComboBoxColumn
            {
                Name = name,
                HeaderText = $"{name} {LanguageManager.GetString("Selection_Text")}",
                DataSource = options,
                DisplayMember = "Text",
                ValueMember = "Value",
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            var points = GetPointList();
            GlobalDataPoint.SetPointList(points);
            SaveConfig();
            GeneratePointControls();
        }

        private void GeneratePointControls()
        {
            flowPanel.Controls.Clear();
            var list = GetPointList();

            foreach (var p in list)
            {
                var group = new GroupBox
                {
                    Text = p.Name,
                    ForeColor = Color.White,
                    Width = 350,
                    Height = 200,
                    BackColor = Color.FromArgb(55, 55, 55),
                    Padding = new Padding(8),
                    Margin = new Padding(10)
                };

                var lbl = new Label
                {
                    Text = $"X: {p.XValue}    Y: {p.YValue}   Z: {p.ZValue}    R: {p.RValue}",
                    ForeColor = Color.LightGray,
                    Dock = DockStyle.Top,
                    Height = 25
                };

                // --- 按钮区 ---
                var panelBtns = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    Height = 70
                };

                var btnMove = CreateButton(LanguageManager.GetString("Btn_MoveTo"), Color.FromArgb(80, 150, 220));
                var btnSave = CreateButton(LanguageManager.GetString("Btn_Save"), Color.FromArgb(200, 140, 80));

                btnMove.Click += (s, e) =>
                {
                    MessageBox.Show($"{LanguageManager.GetString("Btn_MoveTo")}：{p.Name}\nX: {p.X}  Y: {p.Y}  Z: {p.Z}  R: {p.R}", LanguageManager.GetString("Motion_Text"));
                    // TODO: 调用API MoveTo(p)
                };

                btnSave.Click += (s, e) =>
                {
                    //TODO: 从运动控制类获取实时位置
                    SaveConfig();
                };

                // --- JOG 区 ---
                var cmbAxis = new ComboBox
                {
                    Width = 70,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    Margin = new Padding(5)
                };
                cmbAxis.Items.AddRange(new[] { "X", "Y", "Z", "R" });
                cmbAxis.SelectedIndex = 0;

                var btnJogMinus = CreateButton("JOG -", Color.FromArgb(180, 80, 80));
                var btnJogPlus = CreateButton("JOG +", Color.FromArgb(80, 180, 80));

                btnJogMinus.Click += (s, e) => JogMove(p, cmbAxis.SelectedItem.ToString(), -0.5, lbl);
                btnJogPlus.Click += (s, e) => JogMove(p, cmbAxis.SelectedItem.ToString(), 0.5, lbl);

                panelBtns.Controls.Add(btnMove);
                panelBtns.Controls.Add(btnSave);
                panelBtns.SetFlowBreak(btnSave, true);  //第一行结束
                panelBtns.Controls.Add(cmbAxis);
                panelBtns.SetFlowBreak(cmbAxis, true);  //第二行结束
                panelBtns.Controls.Add(btnJogMinus);
                panelBtns.Controls.Add(btnJogPlus);

                group.Controls.Add(panelBtns);
                group.Controls.Add(lbl);

                flowPanel.Controls.Add(group);
            }
        }

        private void UpdatePointInGrid(MarkPoints p)
        {
            foreach (DataGridViewRow row in dgvPoints.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["Name"].Value?.ToString() == p.Name)
                {
                    row.Cells["XValue"].Value = p.XValue;
                    row.Cells["YValue"].Value = p.YValue;
                    row.Cells["ZValue"].Value = p.ZValue;
                    row.Cells["RValue"].Value = p.RValue;
                    break;
                }
            }
        }

        private void JogMove(MarkPoints p, string axis, double delta, Label lbl)
        {
            double.TryParse(p.XValue, out double x);
            double.TryParse(p.YValue, out double y);
            double.TryParse(p.ZValue, out double z);
            double.TryParse(p.RValue, out double r);

            switch (axis)
            {
                case "X": x += delta; p.XValue = x.ToString("F3"); break;
                case "Y": y += delta; p.YValue = y.ToString("F3"); break;
                case "Z": z += delta; p.ZValue = z.ToString("F3"); break;
                case "R": r += delta; p.RValue = r.ToString("F3"); break;
            }

            lbl.Text = $"X: {p.XValue}    Y: {p.YValue}   Z: {p.ZValue}    R: {p.RValue}";
            UpdatePointInGrid(p);
            SaveConfig();

            //TODO: 实际运动发送Jog命令
        }

        private Control CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 130,
                Height = 32,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8)
            };
        }

        public List<MarkPoints> GetPointList()
        {
            var list = new List<MarkPoints>();
            foreach (DataGridViewRow row in dgvPoints.Rows)
            {
                if (row.IsNewRow) continue;
                var p = new MarkPoints
                {
                    Device = cmbDevice.Text,
                    Name = row.Cells["Name"].Value?.ToString() ?? "",
                    X = row.Cells["X"].Value?.ToString() ?? "",
                    Y = row.Cells["Y"].Value?.ToString() ?? "",
                    Z = row.Cells["Z"].Value?.ToString() ?? "",
                    R = row.Cells["R"].Value?.ToString() ?? "",
                    XValue = row.Cells["XValue"].Value?.ToString() ?? "",
                    YValue = row.Cells["YValue"].Value?.ToString() ?? "",
                    ZValue = row.Cells["ZValue"].Value?.ToString() ?? "",
                    RValue = row.Cells["RValue"].Value?.ToString() ?? "",
                };
                if(!string.IsNullOrEmpty(p.Name))
                    list.Add(p);
            }

            GlobalDataPoint.SetPointList(list);
            return list;
        }

        private void SaveConfig()
        {
            List<MarkPoints> allPoints = new List<MarkPoints>();

            if (File.Exists(GlobalDataPoint.ConfigFile))
            {
                var old = JsonSerializer.Deserialize<List<MarkPoints>>(File.ReadAllText(GlobalDataPoint.ConfigFile));
                allPoints.AddRange(old ?? new List<MarkPoints>());
                allPoints.RemoveAll(p => p.Device == cmbDevice.Text);
            }

            allPoints.AddRange(GetPointList());
            GlobalDataPoint.SavePointListToJson(allPoints);
        }

        private void LoadConfig()
        {
            dgvPoints.Rows.Clear();
            //if(!File.Exists(GlobalDataPoint.ConfigFile)) return;

            //var list = FileHelper.ReadJson<List<MarkPoints>>(GlobalDataPoint.ConfigFile);
            GlobalDataPoint.LoadPointListFromJson();
            var list = GlobalDataPoint.GetPointList();

            var current = list?.FindAll(p => p.Device == cmbDevice.Text);

            if(current != null)
            {
                foreach (var p in current)
                {
                    object[] rowValues = new object[]
                    {
                        p.Name,
                        GetValidComboValue(dgvPoints.Columns["X"] as DataGridViewComboBoxColumn, p.X),
                        GetValidComboValue(dgvPoints.Columns["Y"] as DataGridViewComboBoxColumn, p.Y),
                        GetValidComboValue(dgvPoints.Columns["Z"] as DataGridViewComboBoxColumn, p.Z),
                        GetValidComboValue(dgvPoints.Columns["R"] as DataGridViewComboBoxColumn, p.R),
                        p.XValue,
                        p.YValue,
                        p.ZValue,
                        p.RValue
                    };

                    dgvPoints.Rows.Add(rowValues);
                }
            }
            GeneratePointControls();
        }

        private string GetValidComboValue(DataGridViewComboBoxColumn column, string value)
        {
            if (column == null || string.IsNullOrEmpty(value)) return null;

            var options = column.DataSource as List<ComboOption>;
            if(options != null && options.Any(o =>  o.Value == value))
                return value;

            return options?.FirstOrDefault()?.Value;
        }
    }
}
