using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_TM.PageView;
using YR_TM.Utils;

namespace YR_TM.View
{
    public partial class PageDebug : UserControl
    {
        private TabControl tabControl;
        private TabPage tabMotionPage;
        private TabPage tabIOPage;
        private TabPage tabScrewdriverPage;
        private TabPage SystemPage;

        public PageDebug()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            tabControl = new TabControl();
            this.SuspendLayout();

            tabMotionPage = CreateTabPage("Motion_Text", new MotionControl());
            tabIOPage = CreateTabPage("IO_Text", new IOControl());
            tabScrewdriverPage = CreateTabPage("Screwdriver_Text", new ScrewDriverControl());
            SystemPage = CreateTabPage("System_Text", new SystemControl());

            //TabControl配置
            tabControl.Dock = DockStyle.Fill;
            tabControl.ItemSize = new Size(120, 40);
            tabControl.Font = new Font("微软雅黑", 10, FontStyle.Regular);
            tabControl.SizeMode = TabSizeMode.Fixed;

            tabControl.TabPages.Add(tabMotionPage);
            tabControl.TabPages.Add(tabIOPage);
            tabControl.TabPages.Add(tabScrewdriverPage);
            tabControl.TabPages.Add(SystemPage);

            //设置标签页内容
            tabMotionPage.Controls.Add(new MotionControl());
            tabIOPage.Controls.Add(new IOControl());
            tabScrewdriverPage.Controls.Add(new ScrewDriverControl());
            SystemPage.Controls.Add(new SystemControl());

            this.Controls.Add(tabControl);
            this.ResumeLayout(false);
        }

        ///<summary>
        ///创建TabPage并添加控件
        /// </summary>
        private TabPage CreateTabPage(string resourceKey, Control control)
        {
            TabPage tabPage = new TabPage(LanguageManager.GetString(resourceKey));
            tabPage.Controls.Add(control);
            return tabPage;
        }
    }
}
