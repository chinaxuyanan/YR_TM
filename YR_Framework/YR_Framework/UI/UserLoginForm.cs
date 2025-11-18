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
    public partial class UserLoginForm : Form
    {
        private ComboBox cmbUser;
        private TextBox txtPwd;
        private ComboBox cmbRunMode;
        private Button btnLogin;
        private Button btnCancel;

        public UserLevel SelectedUser { get; private set; }
        public RunMode SelectedRunMode { get; private set; }

        public UserLoginForm(UserLevel currentUser, RunMode currentRunMode)
        {
            InitializeComponent();
            InitUI();

            cmbUser.SelectedItem = currentUser.ToString();
            cmbRunMode.SelectedItem = currentRunMode.ToString();
        }

        private void InitUI()
        {
            this.Size = new Size(360, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Label lblTitle = new Label()
            {
                Text = "用户登录",
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            //用户
            Label lblUser = new Label()
            {
                Text = "用户：",
                Left = 40,
                Top = 70,
                Width = 80,
                Font = new Font("微软雅黑", 10)
            };
            this.Controls.Add(lblUser);

            cmbUser = new ComboBox()
            {
                Left = 130,
                Top = 68,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 10)
            };
            cmbUser.Items.AddRange(Enum.GetNames(typeof(UserLevel)));
            cmbUser.SelectedIndexChanged += CmbUser_SelectedIndexChanged;
            this.Controls.Add(cmbUser);

            //密码
            Label lblPwd = new Label()
            {
                Text = "密码：",
                Left = 40,
                Top = 110,
                Width = 80,
                Font = new Font("微软雅黑", 10)
            };
            this.Controls.Add(lblPwd);

            txtPwd = new TextBox()
            {
                Left = 130,
                Top = 108,
                Width = 160,
                PasswordChar = '*',
                Font = new Font("微软雅黑", 10)
            };
            this.Controls.Add(txtPwd);

            //运行模式
            Label lblMode = new Label()
            {
                Text = "运行模式：",
                Left = 40,
                Top = 150,
                Width = 80,
                Font = new Font("微软雅黑", 10)
            };
            this.Controls.Add(lblMode);

            cmbRunMode = new ComboBox()
            {
                Left = 130,
                Top = 148,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 10)
            };
            this.Controls.Add(cmbRunMode);

            //登录按钮
            btnLogin = new Button()
            {
                Text = "登录",
                Left = 60,
                Top = 200,
                Width = 100,
                Height = 35,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(72, 133, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            //取消按钮
            btnCancel = new Button()
            {
                Text = "登出",
                Left = 190,
                Top = 200,
                Width = 100,
                Height = 35,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            AppState.CurrentUser = UserLevel.Operator;
            AppState.CurrentRunMode = RunMode.Product;

            EventCenter.Publish(new UserChangedEvent { CurrentUser = AppState.CurrentUser });
            EventCenter.Publish(new RunModeChangedEvent { CurrentRunMode = AppState.CurrentRunMode });
            EventCenter.Publish(new LogOutChangeEvent { User = AppState.CurrentUser });

            this.Close();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (cmbUser.SelectedItem == null)
            {
                MessageBox.Show("请选择用户", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedUser = (UserLevel)Enum.Parse(typeof(UserLevel), cmbUser.SelectedItem.ToString());
            if (!ValidatePassword(selectedUser, txtPwd.Text))
            {
                MessageBox.Show("密码错误，请重试", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPwd.Clear();
                txtPwd.Focus();
                return;
            }

            if (cmbRunMode.SelectedItem == null)
            {
                MessageBox.Show("请选择运行模式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectedUser = selectedUser;
            SelectedRunMode = (RunMode)Enum.Parse(typeof(RunMode), cmbRunMode.SelectedItem.ToString());
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CmbUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbUser.SelectedItem == null) return;

            var selectedUser = (UserLevel)Enum.Parse(typeof(UserLevel), cmbUser.SelectedItem.ToString());
            cmbRunMode.Items.Clear();

            switch(selectedUser)
            {
                case UserLevel.Operator: cmbRunMode.Items.Add(RunMode.Product.ToString()); break;
                case UserLevel.Engineer: cmbRunMode.Items.AddRange(new string[] { RunMode.Product.ToString(), RunMode.Debug.ToString() }); break;
                case UserLevel.Admin: cmbRunMode.Items.AddRange(Enum.GetNames(typeof(RunMode))); break;
            }

            cmbRunMode.SelectedIndex = 0;
        }

        private bool ValidatePassword(UserLevel user, string password)
        {
            switch(user)
            {
                case UserLevel.Operator: return password == "op";
                case UserLevel.Engineer: return password == "eng";
                case UserLevel.Admin: return password == "admin";
                default: return false;
            }
        }
    }
}
