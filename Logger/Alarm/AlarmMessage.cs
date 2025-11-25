using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Alarm
{
    public class AlarmMessage
    {
        public int ID {  get; set; }
        public ErrorType AlarmCode {  get; set; }
        public string Message {  get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;

        public string GetFormattedStartTime()
        {
            return StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public string GetFormattedEndTime()
        {
            return EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }

    public enum ErrorType
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info = 0,

        /// <summary>
        /// 急停错误
        /// </summary>
        Err_Emg = 1,

        /// <summary>
        /// 报警
        /// </summary>
        Warn,

        ///轴控制错误
        Err_Motion = 1000,
        /// <summary>
        /// 轴初始化错误
        /// </summary>
        Err_Motion_Init,
        /// 轴释放错误
        Err_Motion_DeInit,
        /// <summary>
        /// 轴使能错误
        /// </summary>
        Err_Motion_ServoOn,
        /// <summary>
        /// 轴去使能错误
        /// </summary>
        Err_Motion_ServoOff,
        /// <summary>
        /// 轴回原点错误
        /// </summary>
        Err_Motion_Home,
        /// <summary>
        /// 轴绝对运动错误
        /// </summary>
        Err_Motion_Abs,
        /// <summary>
        /// 轴相对运动错误
        /// </summary>
        Err_Motion_Rel,
        /// <summary>
        /// 轴JOG运行错误
        /// </summary>
        Err_Motion_Jog,
        /// <summary>
        /// 轴速度运动错误
        /// </summary>
        Err_Motion_Vel,
        /// <summary>
        /// 轴停止错误
        /// </summary>
        Err_Motion_Stop,
        /// <summary>
        /// 轴急停错误
        /// </summary>
        Err_Motion_EmgStop,
        /// <summary>
        /// 轴状态错误
        /// </summary>
        Err_Motion_State,
        /// <summary>
        /// 设置轴位置错误
        /// </summary>
        Err_Motion_SetPos,
        /// <summary>
        /// 设置轴参数错误
        /// </summary>
        Err_Motion_SetParam,
        /// <summary>
        /// 轴运动超时
        /// </summary>
        Err_Motion_TimeOut,
        /// <summary>
        /// 限位超时
        /// </summary>
        Err_Motion_El_TimeOut,
        /// <summary>
        /// 正限位超时
        /// </summary>
        Err_Motion_Pel_TimeOut,
        /// <summary>
        /// 负限位超时
        /// </summary>
        Err_Motion_Mel_TimeOut,
        /// <summary>
        /// 原点超时
        /// </summary>
        Err_Motion_Org_TimeOut,
        /// <summary>
        /// HOME超时
        /// </summary>
        Err_Motion_Home_TimeOut,

        ///IO控制错误
        Err_IO = 2000,
        ///IO初始化错误
        Err_IO_Init,
        ///IO读输入错误
        Err_IO_Read_In,
        ///IO读输出错误
        Err_IO_Read_Out,
        ///IO写错误
        Err_IO_Write,
        /// <summary>
        /// IO状态错误
        /// </summary>
        Err_IO_State,
        /// IO超时错误
        Err_IO_TimeOut,

        ///机器人错误
        Err_Robot = 3000,

        ///串口通信错误
        Err_Com = 4000,
        ///串口打开错误
        Err_Com_Open,
        ///串口读错误
        Err_Com_Read,
        ///串口写错误
        Err_Com_Write,
        ///串口超时错误
        Err_Com_TimeOut,

        ///网络通信错误
        Err_Tcp = 5000,
        ///网络打开错误
        Err_Tcp_Open,
        ///网络读错误
        Err_Tcp_Read,
        ///网络写错误
        Err_Tcp_Write,
        ///网络超时错误
        Err_Tcp_TimeOut,
        ///网络接收数据比对超时错误
        Err_TcpDataComp_TimeOut,



        ///OPC通信错误
        Err_Opc = 6000,
        ///OPC打开错误
        Err_Opc_Open,
        ///OPC读错误
        Err_Opc_Read,
        ///OPC写错误
        Err_Opc_Write,
        ///OPC超时错误
        Err_Opc_TimeOut,

        ///PLC通信错误
        Err_Plc = 7000,
        ///PLC打开错误
        Err_Plc_Open,
        ///PLC读错误
        Err_Plc_Read,
        ///PLC写错误
        Err_Plc_Write,
        ///PLC超时错误
        Err_Plc_TimeOut,

        ///流程错误
        Err_Work_Flow = 8000,
        /// <summary>
        /// 位寄存器超时
        /// </summary>
        Err_RegBit_TimeOut,

        /// <summary>
        /// 整型寄存器超时
        /// </summary>
        Err_RegInt_TimeOut,

        /// <summary>
        /// 浮点寄存器超时
        /// </summary>
        Err_RegDouble_TimeOut,

        /// <summary>
        /// 字符串寄存器超时
        /// </summary>
        Err_RegString_TimeOut,

        /// <summary>
        /// 视觉错误
        /// </summary>
        Err_Vision = 9000,
        ///视觉打开错误
        Err_Vision_Open,
        ///视觉拍照错误
        Err_Vision_Snap,
        ///视觉处理错误
        Err_Vision_Process,
        ///视觉参数错误
        Err_Vision_Param,


        ///系统错误
        Err_System = 10000,


        ///未定义错误
        Err_Und = 99999,
    }
}
