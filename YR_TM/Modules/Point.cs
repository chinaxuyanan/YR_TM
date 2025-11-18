using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YR_TM.Utils;

namespace YR_TM.Modules
{
    public class MarkPoints
    {
        public string Device { get; set; }
        public string Name { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string R { get; set; }
        public string XValue {  get; set; }
        public string YValue { get; set; }
        public string ZValue { get; set; }
        public string RValue {  get; set; }
    }

    public static class GlobalDataPoint
    {
        public const string ConfigFile = "MotionPoints.json";

        public static List<MarkPoints> PointList = new List<MarkPoints>();

        //从文件读取数据并设置到全局数据
        public static void LoadPointListFromJson()
        {
            if (File.Exists(ConfigFile))
                PointList = FileHelper.ReadJson<List<MarkPoints>>(ConfigFile);
            else
                PointList = new List<MarkPoints>();
        }

        //保存全局数据到文件
        public static void SavePointListToJson(List<MarkPoints> points)
        {
            FileHelper.WriteJson(ConfigFile, points);
        }

        //设置点位数据
        public static void SetPointList(List<MarkPoints> points)
        {
            PointList = points;
        }

        //获取点位数据
        public static List<MarkPoints> GetPointList()
        {
            return PointList ?? new List<MarkPoints>();
        }
    }
}
