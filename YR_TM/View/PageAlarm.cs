using Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_TM.Utils;

namespace YR_TM.View
{
    public partial class PageAlarm : UserControl
    {
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Button btnQuery, btnExport, btnClear;
        private DataGridView dgvAlarm;

        public PageAlarm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 45);

            dgvAlarm = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 43),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false
            };

            dgvAlarm.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);
            dgvAlarm.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAlarm.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvAlarm.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvAlarm.DefaultCellStyle.ForeColor = Color.White;
            dgvAlarm.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 130, 180);

            dgvAlarm.Columns.Add("ID", "ID");
            dgvAlarm.Columns.Add("Code", LanguageManager.GetString("AlarmCode"));
            dgvAlarm.Columns.Add("Message", LanguageManager.GetString("Alarm_Info"));
            dgvAlarm.Columns.Add("StartTime", LanguageManager.GetString("StartTime_Text"));
            dgvAlarm.Columns.Add("EndTime", LanguageManager.GetString("EndTime_Text"));
            dgvAlarm.Columns.Add("Duration", LanguageManager.GetString("Duration_Text"));

            dgvAlarm.Columns["ID"].Width = 60;
            dgvAlarm.Columns["Code"].Width = 100;
            dgvAlarm.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvAlarm.Columns["StartTime"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvAlarm.Columns["EndTime"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvAlarm.Columns["Duration"].Width = 120;

            this.Controls.Add(dgvAlarm);

            //顶部筛选栏
            Panel topPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(55, 55, 58),
                Padding = new Padding(10, 10, 10, 10)
            };

            Label lblStart = new Label
            {
                Text = $"{LanguageManager.GetString("StartTime_Text")}: ",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 20)
            };
            topPanel.Controls.Add(lblStart);

            dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                Width = 180,
                Location = new Point(120, 15)
            };
            topPanel.Controls.Add(dtpStart);

            Label lblEnd = new Label
            {
                Text = $"{LanguageManager.GetString("EndTime_Text")}: ",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(320, 20)
            };
            topPanel.Controls.Add(lblEnd);

            dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                Width = 180,
                Location = new Point(450, 15)
            };
            topPanel.Controls.Add(dtpEnd);

            btnQuery = new Button
            {
                Text = LanguageManager.GetString("Btn_Query"),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 80,
                Height = 30,
                Location = new Point(650, 15)
            };
            btnQuery.FlatAppearance.BorderSize = 0;
            btnQuery.Click += BtnQuery_Click;
            topPanel.Controls.Add(btnQuery);

            btnExport = new Button
            {
                Text = LanguageManager.GetString("Btn_Export"),
                BackColor = Color.FromArgb(100, 149, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 100,
                Height = 30,
                Location = new Point(750, 15)
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            topPanel.Controls.Add(btnExport);

            this.Controls.Add(topPanel);

            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(55, 60, 65)
            };

            btnClear = new Button
            {
                Text = LanguageManager.GetString("Btn_Clear"),
                Width = 140,
                Height = 35,
                BackColor = Color.FromArgb(200, 70, 70),
                ForeColor = Color.White
            };
            btnClear.Click += BtnClear_Click;
            panelBottom.Controls.Add(btnClear);
            CenterButtonInPanel(panelBottom, btnClear);

            this.Controls.Add(panelBottom);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("确认要清除所有报警信息吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                dgvAlarm.Rows.Clear();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvAlarm.Rows.Count == 0)
            {
                MessageBox.Show("没有可导出的报警记录...");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV文件 （*.csv）| *.csv",
                FileName = $"报警记录_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportToCsv(sfd.FileName);
                    MessageBox.Show("导出成功！");
                }
            }
        }

        private void BtnQuery_Click(object sender, EventArgs e)
        {
            DateTime startTime = dtpStart.Value;
            DateTime endTime = dtpEnd.Value;

            var alarmManager = LogManager.GetLogger(FrameworkContext.AlarmTestManager).GetAlarmManager();
            var filteredAlarms = alarmManager.AlarmHistory.Where(a => a.StartTime >= startTime && a.EndTime <= endTime).ToList();

            dgvAlarm.Rows.Clear();

            foreach (var alarm in filteredAlarms)
            {
                dgvAlarm.Rows.Add(alarm.ID, alarm.AlarmCode, alarm.Message, alarm.GetFormattedStartTime(), alarm.GetFormattedEndTime(), alarm.Duration);
            }
        }

        private void ExportToCsv(string filePath)
        {
            StringBuilder sb = new StringBuilder();

            //写入表头
            for (int i = 0; i < dgvAlarm.Columns.Count; i++)
            {
                sb.Append(dgvAlarm.Columns[i].HeaderText);
                if(i < dgvAlarm.Columns.Count - 1)
                    sb.Append(",");
            }
            sb.AppendLine();

            //写入数据
            foreach (DataGridViewRow row in dgvAlarm.Rows)
            {
                for (int i = 0; i < dgvAlarm.Columns.Count; i++)
                {
                    sb.Append(row.Cells[i].Value?.ToString()?.Replace(",", "，"));
                    if(i < dgvAlarm.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void CenterButtonInPanel(Panel panel, Button button)
        {
            button.Left = (panel.ClientSize.Width - button.Width) / 2;
            button.Top = (panel.ClientSize.Height - button.Height) / 2;

            panel.Resize += (s, e) =>
            {
                button.Left = (panel.ClientSize.Width - button.Width) / 2;
                button.Top = (panel.ClientSize.Height - button.Height) / 2;
            };
        }
    }
}
