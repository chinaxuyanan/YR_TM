using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_TM.Utils;

namespace YR_TM
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string lang = FileHelper.ReadJson<string>("SystemLanguage") ?? "zh-CN";
            LanguageManager.ChangeLanguage(lang);

            FrameworkContext.StationName = "TAM";
            Application.Run(new MainForm());
        }
    }
}
