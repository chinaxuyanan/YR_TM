using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YR_TM
{
    public class IOFilePath
    {
        public string MAGFilePath = "IOConfig\\MAG-IO.xlsx";
        public string TAMFilePath = "IOConfig\\TAM-IO.xlsx";
    }

    public class IOPoint
    {
        public string Address {  get; set; }
        public string Name { get; set; }
        public string Description {  get; set; }
        public bool State {  get; set; } = false;
    }

    public class IOConfig
    {
        public List<IOPoint> Inputs { get; set; } = new List<IOPoint>();
        public List<IOPoint> Outputs { get; set; } = new List<IOPoint>();
    }

    public static class IO_Address
    {
        public enum TAM_IO_Inputs
        {
            急停旋钮=0,

            检修灯旋钮,

            启动按钮,

            复位按钮,

            吸气按钮,

            停止按钮,

            Holder板光电开关=10,

            夹紧气缸到位磁性开关,

            夹紧气缸原位磁性开关,

            排线气缸到位磁性开关,

            排线气缸原位磁性开关,

            排线盖板磁性开关,

            气压数显表输出1,

            气压数显表输出2,

            点胶机运行状态=20,

            安全门接近开关,

            点胶针头原点传感器,

            点胶针头测高传感器,

            真空吸嘴吸真空传感器,

            力值显示表输出1,

            力值显示表输出2,

            安全光栅,

            温控仪报警_1=30,

            温控仪报警_2,

            擦胶原位磁性开关,

            擦胶到位磁性开关,


        }

        public enum TAM_IO_Outputs 
        {
            三色灯_红灯=0,

            三色灯_黄灯,

            三色灯_绿灯,

            三色灯_蜂鸣器,

            复位按钮指示灯,

            双手启动按钮指示灯,

            真空发生器_吸,

            真空发生器_破,

            点胶机启动=10,

            力值显示仪表清零,

            检修灯照明中间继电器,

            排线吸气电磁阀,

            排线气缸原位电磁阀,

            排线气缸到位电磁阀,

            Holder夹紧气缸原位电磁阀,

            Holder夹紧气缸到位电磁阀,

            停止按钮指示灯=20,

            擦胶原位电磁阀,

            擦胶到位电磁阀,


        }


        public enum MAG_IO_Inputs
        {
            急停按钮=0,

            检修灯旋钮,

            启动按钮,

            复位按钮,

            停止按钮,

            螺丝盖板光电开关=10,

            螺丝盖板磁性开关,

            侧相机气缸原位磁性开关,

            侧相机气缸到位磁性开关,

            电批头气缸原位磁性开关,

            电批头气缸到位磁性开关,

            电批吸真空压力表输出1,

            电批吸真空压力表输出2,

            点胶机运行状态=20,

            安全门接近开关,

            点胶针头原点传感器,

            点胶针头测高传感器,

            安全光栅1=26,

            安全光栅2,

            擦胶原位磁性开关=32,

            擦胶到位磁性开关,
        }

        public enum MAG_IO_Outputs
        {
            三色灯_红灯=0,

            三色灯_黄灯,

            三色灯_绿灯,

            三色灯_蜂鸣器,

            复位按钮指示灯,

            双手启动按钮指示灯,

            停止按钮指示灯,

            点胶机启动=10,

            检修灯照明中间继电器=12,

            真空吸气电磁阀=14,

            真空出气电磁阀,

            侧相机气缸原位电磁阀,

            侧相机气缸到位电磁阀,


            擦胶原位电磁阀=21,

            擦胶到位电磁阀,
        }
    }

    public  class IO_ConfigHelper
    {
        public static IOConfig GetIOEnumConfig()
        {
            var config = new IOConfig();

            foreach(IO_Address.TAM_IO_Outputs input in Enum.GetValues(typeof(IO_Address.TAM_IO_Outputs)))
            {
                config.Inputs.Add(new IOPoint
                {
                    Address = $"X{(int)input:D2}",

                    Name = input.ToString(),

                    Description = "",
                });
            }

            foreach(IO_Address.TAM_IO_Outputs output in Enum.GetValues(typeof(IO_Address.TAM_IO_Outputs)))
            {
                config.Outputs.Add(new IOPoint
                {
                    Address = $"X{(int)output:D2}",

                    Name= output.ToString(),

                    Description="",
                });       
             }

            return config;
        }
    }

}
