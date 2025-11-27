using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using YR_Framework.Core;
using YR_TM.Modules;

namespace YR_TM.Manager
{
    public static class MAGTestAction
    {
        private static List<MarkPoints> points = GlobalDataPoint.PointList.FindAll(p => p.Device == FrameworkContext.StationName);

        static List<int> VersionX = new List<int>();
        static List<int> VersionY = new List<int>();
        static List<int> VersionZ = new List<int>();

        //上拍照动作
        public static void MoveUpCamera()
        {
            var p = points?.FirstOrDefault(x => x.Name == "上拍照位");
            if(p == null) return;

            double XValue = double.Parse(p.XValue);
            double YValue = double.Parse(p.YValue);

            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (int)XValue, 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (int)YValue, 10000);

            //视觉 -> 值
            VersionX = new List<int> { 12, 23, 34 };
            VersionY = new List<int> { 34, 43, 45 };
            VersionZ = new List<int> { 3, 5, 12 };
        }

        //点胶动作
        public static void DispenseGlue()
        {
            var p = points?.FirstOrDefault(x => x.Name == "点胶位");
            if(p == null) return;

            int xValue = int.Parse(p.XValue);
            int yValue = int.Parse(p.YValue);
            int zValue = int.Parse(p.ZValue);

            //根据视觉结果，分别移动到三个点胶位置进行点胶,第一个孔位
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (xValue + VersionX[0]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (yValue + VersionY[0]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, (zValue + VersionZ[0]), 10000);
            //TODO: 点胶指令

            //第二个孔位
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (xValue + VersionX[1]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (yValue + VersionY[1]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, (zValue + VersionZ[1]), 10000);
            //TODO: 点胶指令

            //第三个孔位
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (xValue + VersionX[2]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (yValue + VersionY[2]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, (zValue + VersionZ[2]), 10000);
            //TODO: 点胶指令
        }

        //取螺丝动作
        public static void PickScrew()
        {
            //Z轴回原，在移动X和Y轴
            MotionModule.Instance.Home((int)AxisInfo.MAGAxis.Z, (int)HomeMode.ORG_N);

            var p = points?.FirstOrDefault(x => x.Name == "取螺丝位");
            if (p == null) return;

            int xValue = int.Parse(p.XValue);
            int yValue = int.Parse(p.YValue);

            //视觉 -》 值
            //坐标或者偏移值

            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, xValue, 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, yValue, 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, yValue, 10000);

            //取螺丝开始
        }

        //侧相机和下相机检测同心度
        public static void CheckConcentricity()
        {
            var p = points?.FirstOrDefault(x => x.Name == "检测螺丝点位");
            if (p == null) return;

            int xValue = int.Parse(p.XValue);
            int yValue = int.Parse(p.YValue);
            int zValue = int.Parse(p.ZValue);

            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, xValue, 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, yValue, 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, zValue, 10000);

            //视觉检测 -》 PASS/NG
            bool result = false;
            if (result)
            {
                var point = points?.FirstOrDefault(n => n.Name == "抛料位");
                if (point == null) return;

                int x = int.Parse(point.XValue);
                int y = int.Parse(point.YValue);
                int z = int.Parse(point.ZValue);

                MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, x, 10000);
                MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, y, 10000);
                MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, z, 10000);
            }
        }

        //组装动作
        public static void Assemble()
        {
            var p = points?.FirstOrDefault(x => x.Name == "组装位");
            if (p == null) return;

            int xValue = int.Parse(p.XValue);
            int yValue = int.Parse(p.YValue);
            int zValue = int.Parse(p.ZValue);

            //第一个点
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (xValue + VersionX[0]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (yValue + VersionY[0]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, (zValue + VersionZ[0]), 10000);
            //锁附

            //第二个点
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (xValue + VersionX[1]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (yValue + VersionY[1]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, (zValue + VersionZ[1]), 10000);
            //锁附

            //第三个点
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.X, (xValue + VersionX[2]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, (yValue + VersionY[2]), 10000);
            MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Z, (zValue + VersionZ[2]), 10000);
            //锁附
        }

    }
}
