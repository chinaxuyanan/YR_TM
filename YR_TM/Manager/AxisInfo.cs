using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YR_TM.Manager
{
    public class AxisInfo
    {
        public enum MAGAxis
        {
            X = 1,
            Y,
            Z
        }

        public enum TAMAxis
        {
            吸嘴X轴 = 1,
            Y,
            吸嘴Z轴,
            点胶X轴,
            点胶Z轴,
            旋转R轴
        }
    }
}
