using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace YR_TM.Utils
{
    public static class LanguageManager
    {
        private static ResourceManager _rm = new ResourceManager("YR_TM.Lang.Strings", typeof(LanguageManager).Assembly);
        public static event Action LanguageChanged;

        public static string CurrentLanguage { get; private set; } = "zh_CN";

        public static void ChangeLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode)) return;
            CurrentLanguage = langCode;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langCode);

            // 切换语言资源
            LanguageChanged?.Invoke();

            //立即刷新所有打开窗体的 UI
            //ForceRefreshUI();
        }

        private static void ForceRefreshUI()
        {
            foreach (Form form in Application.OpenForms)
            {
                ApplyResources(form);           // 刷新窗体及其子控件
                RefreshAllControls(form);       // 递归刷新所有控件
            }
        }

        private static void RefreshAllControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                ApplyResources(ctrl);

                if (ctrl.HasChildren)
                    RefreshAllControls(ctrl);
            }

            parent.Refresh();
        }

        public static string GetString(string key)
        {
            return _rm.GetString(key, Thread.CurrentThread.CurrentUICulture) ?? key;
        }

        //将资源应用到整个Control树，使用ResourceManager的ApplyResources方式
        public static void ApplyResources(Control root)
        {
            if(root == null) return;
            var cm = new ComponentResourceManager(root.GetType());
            ApplyResourcesRecursive(root, cm);
        }

        private static void ApplyResourcesRecursive(Control ctrl, ComponentResourceManager cm)
        {
            try { cm.ApplyResources(ctrl, ctrl.Name, Thread.CurrentThread.CurrentUICulture); } catch { }

            if(ctrl is MenuStrip ms)
            {
                foreach (ToolStripItem item in ms.Items)
                    TryApplyToolStripItemResources(item, cm);
            }

            if(ctrl is ToolStrip ts)
            {
                foreach (ToolStripItem item in ts.Items)
                    TryApplyToolStripItemResources(item, cm);
            }

            foreach (Control child in ctrl.Controls)
                ApplyResourcesRecursive(child, cm);
        }

        private static void TryApplyToolStripItemResources(ToolStripItem item, ComponentResourceManager cm)
        {
            try { cm.ApplyResources(item, item.Name, Thread.CurrentThread.CurrentUICulture); } catch { }
            if(item is ToolStripMenuItem menuItem)
            {
                foreach(ToolStripItem sub in menuItem.DropDownItems)
                    TryApplyToolStripItemResources(sub, cm);
            }
        }
    }
}
