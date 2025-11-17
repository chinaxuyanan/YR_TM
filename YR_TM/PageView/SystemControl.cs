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
using YR_TM.View;

namespace YR_TM.PageView
{
    public partial class SystemControl : UserControl
    {

        private ComboBox cmbBox;
        public SystemControl()
        {
            InitializeComponent();
            InitializeUI();

            LoadSavedLanguage();
        }

        private void LoadSavedLanguage()
        {
            string savedLang = LanguageManager.CurrentLanguage ?? "zh-CN";

            switch (savedLang)
            {
                case "en":
                    cmbBox.SelectedItem = "English";
                    break;
                case "vi":
                    cmbBox.SelectedItem = "Tiếng Việt";
                    break;
                default:
                    cmbBox.SelectedItem = "中文";
                    break;
            }
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);

            //通信
            var grpComm = CreateGroup(LanguageManager.GetString("Configuration_Text"), 20, 20, 400, 150);
            var cmbDevice = CreateCombo(LanguageManager.GetString("Device_Text"), new[] { LanguageManager.GetString("Camera"), LanguageManager.GetString("Light"), LanguageManager.GetString("Screwdriver"), "EtherCAT" }, 20, 40);
            var cmbPort = CreateCombo(LanguageManager.GetString("Serial_Text"), new[] { "COM1", "COM2", "COM3" }, 20, 80);
            grpComm.Controls.AddRange(new Control[] { cmbDevice.label, cmbDevice.combo, cmbPort.label, cmbPort.combo });

            //相机
            var grpCam = CreateGroup(LanguageManager.GetString("Camera_Text"), 440, 20, 400, 150);
            var txtCamIP = CreateText(LanguageManager.GetString("Address_Text"), "192.168.1.10", 20, 40);
            var txtExposure = CreateText($"{LanguageManager.GetString("Exposure_Text")}(ms): ", "10000", 20, 80);
            grpCam.Controls.AddRange(new Control[] { txtCamIP.label, txtCamIP.textBox, txtExposure.label, txtExposure.textBox });

            //光源
            var grpLight = CreateGroup(LanguageManager.GetString("Light_Text"), 860, 20, 300, 150);
            var cmbCh = CreateCombo(LanguageManager.GetString("Channel"), new[] { "CH1", "CH2", "CH3", "CH4" }, 20, 40);
            var trkLight = new TrackBar { Location = new Point(90, 80), Width = 180, Maximum = 255, Value = 100 };
            grpLight.Controls.AddRange(new Control[] { cmbCh.label, cmbCh.combo, trkLight });

            //EtherCAT
            var grpECAT = CreateGroup("EtherCAT", 20, 190, 400, 150);
            var cmbNet = CreateCombo(LanguageManager.GetString("Network"), new[] { "Inter I210", "Realtek 8125" }, 20, 40);
            var txtCycle = CreateText($"{LanguageManager.GetString("Refresh")}(ms): ", "10", 20, 80);
            grpECAT.Controls.AddRange(new Control[] { cmbNet.label, cmbNet.combo, txtCycle.label, txtCycle.textBox });

            //路径设置
            var grpPath = CreateGroup(LanguageManager.GetString("Path_Text"), 440, 190, 400, 150);
            var txtImgPath = CreateText(LanguageManager.GetString("Image"), "D:\\Images", 20, 40);
            var txtLogPath = CreateText(LanguageManager.GetString("Log"), "D:\\Logs", 20, 80);
            grpPath.Controls.AddRange(new Control[] { txtImgPath.label, txtImgPath.textBox, txtLogPath.label, txtLogPath.textBox });

            //系统参数
            var grpSys = CreateGroup(LanguageManager.GetString("System_Text"), 860, 190, 300, 150);
            var cmbLang = CreateCombo(LanguageManager.GetString("Label_Language"), new[] { "中文", "English", "Tiếng Việt" }, 20, 80);
            cmbBox = cmbLang.combo;
            cmbLang.combo.SelectedIndex = 0;
            grpSys.Controls.AddRange(new Control[] { cmbLang.label, cmbLang.combo });

            //底部按钮
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(55, 60, 65)
            };

            var btnSave = new Button
            {
                Text = LanguageManager.GetString("Btn_Save"),
                Width = 120,
                Height = 35,
                Location = new Point(700, 15),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            var btnLoad = new Button
            {
                Text = LanguageManager.GetString("Btn_Load"),
                Width = 120,
                Height = 35,
                Location = new Point(640, 15),
                BackColor = Color.FromArgb(100, 130, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLoad.FlatAppearance.BorderSize = 0;

            panelBottom.Controls.AddRange(new Control[] {btnSave});

            this.Controls.AddRange(new Control[] { grpComm, grpCam, grpLight, grpECAT, grpPath, grpSys, panelBottom });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string code = string.Empty;
            var sel = cmbBox.SelectedItem?.ToString() ?? "中文";
            switch (sel)
            {
                case "English": code = "en"; break;
                case "Tiếng Việt": code = "vi"; break;
                default: code = "zh-CN"; break;
            }

            FileHelper.WriteJson("SystemLanguage", code);

            LanguageManager.ChangeLanguage(code);

            MessageBox.Show(LanguageManager.GetString("Msg_SaveSuccess"));
        }

        private GroupBox CreateGroup(string title, int x, int y, int w, int h)
        {
            return new GroupBox
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, y),
                Size = new Size(w, h)
            };
        }

        private (Label label, ComboBox combo) CreateCombo(string name, string[] items, int x, int y)
        {
            var lbl = new Label { Text = name, ForeColor = Color.White, Location =  new Point(x, y + 3) };
            var cmb = new ComboBox { Location = new Point(x + 120, y - 3), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmb.Items.AddRange(items);
            return (lbl, cmb);
        }

        private (Label label, TextBox textBox) CreateText(string name, string defaultVal, int x, int y)
        {
            var lbl = new Label { Text = name, ForeColor = Color.White, Location = new Point(x, y + 3) };
            var txt = new TextBox { Text = defaultVal, Location = new Point(x + 120, y - 3), Width = 150 };
            return (lbl, txt);
        }
    }
}
