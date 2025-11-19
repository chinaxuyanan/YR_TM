using System;
using System.Collections.Generic;
using System.IO;
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

            string lang;
            try
            {
                lang = FileHelper.ReadJson<string>("SystemLanguage");
            }
            catch(FileNotFoundException)
            {
                lang = "zh-CN";
            }
            LanguageManager.ChangeLanguage(lang);

            FrameworkContext.StationName = "TAM";
            Application.Run(new MainForm());
        }
    }
}
