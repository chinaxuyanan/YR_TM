using lctdevice;
using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YR_TM.Modules
{
    public class MotionModule
    {
        private ILogger logger = LogManager.GetLogger("MotionModule");

        #region -单例模式
        private static readonly Lazy<MotionModule> _instance = new Lazy<MotionModule>(() => new MotionModule());
        public static MotionModule Instance => _instance.Value;
        private MotionModule() { }
        #endregion

        public ecat_motion.SL_RES m_tResoures = new ecat_motion.SL_RES();

        public int m_nCardNo;


        private bool _isInitialized = false;
        //private bool _isMonitoring = false;
        //private CancellationTokenSource _monitorToken;

        public bool IsInitialized => _isInitialized;
        public bool IsConnected { get; private set; }
        public int SlaveCount {  get; private set; }

        //public event Action<bool> BoardConnectionChanged;
        //public event Action<string> StatusUpdated;

        ///<summary>
        ///初始化轴卡（包含总线等）
        /// </summary>
        public bool Init()
        {
            try
            {
                logger.Info("开始初始化硬件...");

                //打开板卡
                int ret = ecat_motion.M_Open(0, 0);
                if (ret != 0)
                {
                    logger.Error("打开板卡失败！");
                    return false;
                }

                //加载ENI文件
                ret = ecat_motion.M_LoadEni(@"C:\Program Files (x86)\LCT\Pcie-M60\ENI\eni.xml", 0);
                if (ret != 0)
                {
                    logger.Error("加载ENI文件失败！");
                    ecat_motion.M_Close(0);
                    return false;
                }

                //重启FPGA
                ecat_motion.M_ResetFpga(0);
                Thread.Sleep(1000);

                //连接总线
                ret = ecat_motion.M_ConnectECAT(0, 0);
                if (ret != 0)
                {
                    logger.Error("连接EtherCAT总线失败!");
                    ecat_motion.M_Close(0);
                    return false;
                }

                //获取从站信息
                ret = ecat_motion.M_GetSlaveResource(out m_tResoures, 0);
                if (ret != 0)
                {
                    logger.Error("获取从站信息失败！");
                    ecat_motion.M_DisconnectECAT(0);
                    ecat_motion.M_Close(0);
                    return false;
                }

                _isInitialized = true;
                IsConnected = true;

                logger.Info("初始化完成");
                return true;
            }catch (Exception ex)
            {
                logger.Error("初始化异常：" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 关闭板卡并释放资源
        /// </summary>
        public void Close()
        {
            try
            {
                ecat_motion.M_DisconnectECAT(0);
                ecat_motion.M_Close(0);

                _isInitialized = false;
                IsConnected = false;
            }catch(Exception ex)
            {
                logger.Error("关闭板卡异常：" + ex.Message);
            }
        }

        #region - IO 方法

        /// <summary>
        /// 获取输入信号
        /// </summary>
        /// <param name="nData">输入信号信息</param>
        /// <returns></returns>
        public bool ReadIOIn(ref int nData)
        {
            // 总线卡可以扩展多个IO模块，当I/O数量超过32个的时候，此函数只能读取前面32个I/O数据
            uint nInputData = 0;

            //从1开始
            short nCardNo = (short)(m_nCardNo + 1);

            short ret = ecat_motion.M_Get_Digital_Port_Input(nCardNo, ref nInputData, 0);
            if (ret != 0)
            {
                //string str1 = "PCIeM60板卡M_Get_Digital_Port_Input失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Get_Digital_Port_Input error! Result = {0}";
                //}
                //WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_In, "PCIeM60",
                //    string.Format(str1, ret));
                logger.Error("PCIeM60板卡M_Get_Digital_Port_Input失败!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取输入信号
        /// </summary>
        /// <param name="nIndex">输入信号位</param>
        /// <returns></returns>
        public bool ReadIoInBit(int nIndex)
        {
            short diValue = 0;
            short diNo = (short)(m_nCardNo * 32 + nIndex);
            short ret = ecat_motion.M_Get_Digital_Chn_Input(diNo, out diValue, 0);
            if (ret != 0)
            {

                //string str1 = "PCIeM60板卡M_Get_Digital_Chn_Input失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Get_Digital_Chn_Input error! Result = {0}";
                //}
                //WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_In, "PCIeM60",
                //    string.Format(str1, ret));
                logger.Error("PCIeM60板卡M_Get_Digital_Chn_Input失败!");
                return false;
            }

            return (diValue != 0);
        }

        /// <summary>
        /// 获取输出信号
        /// </summary>
        /// <param name="nIndex">输出点位</param>
        /// <returns></returns>
        public bool ReadIoOutBit(int nIndex)
        {
            short doValue = 0;
            short doNo = (short)(m_nCardNo * 32 + nIndex);

            short ret = ecat_motion.M_Get_Digital_Chn_Output(doNo, out doValue, 0);
            if (ret != 0)
            {

                //string str1 = "PCIeM60板卡M_Get_Digital_Chn_Output失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Get_Digital_Chn_Output error! Result = {0}";
                //}
                //WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_In, "PCIeM60",
                //    string.Format(str1, ret));

                logger.Error("PCIeM60板卡M_Get_Digital_Chn_Output失败!");
                return false;
            }

            return (doValue != 0);
        }

        /// <summary>
        /// 获取输出信号
        /// </summary>
        /// <param name="nData"></param>
        /// <returns></returns>
        public bool ReadIOOut(ref int nData)
        {
            uint nOutputData = 0;

            //从1开始
            short nCardNo = (short)(m_nCardNo + 1);

            short ret = ecat_motion.M_Get_Digital_Port_Output(nCardNo, ref nOutputData, 0);
            if (ret != 0)
            {
                //string str1 = "PCIeM60板卡M_Get_Digital_Port_Output失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Get_Digital_Port_Output error! Result = {0}";
                //}
                //WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_Out, "PCIeM60",
                //    string.Format(str1, ret));
                logger.Error("PCIeM60板卡M_Get_Digital_Port_Output失败!");
                return false;
            }

            nData = (int)nOutputData;
            return true;
        }

        /// <summary>
        /// 输出信号
        /// </summary>
        /// <param name="nIndex">输出点</param>
        /// <param name="bBit">输出值</param>
        /// <returns></returns>
        public bool WriteIoBit(int nIndex, bool bBit)
        {
            short doNo = (short)(m_nCardNo * 32 + nIndex);

            short ret = ecat_motion.M_Set_Digital_Chn_Output(doNo, bBit ? (short)1 : (short)0, 0);

            if (ret != 0)
            {

                //string str1 = "PCIeM60板卡M_Set_Digital_Chn_Output失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Set_Digital_Chn_Output error! Result = {0}";
                //}
                //WarningMgr.GetInstance().Error(ErrorType.Err_IO_Write, "PCIeM60",
                //    string.Format(str1, ret));

                logger.Error("PCIeM60板卡M_Set_Digital_Chn_Output失败!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 输出行信号
        /// </summary>
        /// <param name="nData">输出信息</param>
        /// <returns></returns>
        public bool WriteIo(int nData)
        {
            //从1开始
            short nCardNo = (short)(m_nCardNo + 1);

            short ret = ecat_motion.M_Set_Digital_Port_Output(nCardNo, (uint)nData, 0xFFFFFFFF, 0);
            if (ret != 0)
            {
                //string str1 = "PCIeM60板卡M_Set_Digital_Port_Output失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Set_Digital_Port_Output error! Result = {0}";
                //}
                //WarningMgr.GetInstance().Error(ErrorType.Err_IO_Write, "PCIeM60",
                //    string.Format(str1, ret));
                logger.Error("PCIeM60板卡M_Set_Digital_Port_Output失败!");
                return false;
            }

            return true;
        }

        #endregion

        #region - 轴控

        /// <summary>
        /// 给予使能
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public bool ServoOn(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            int ret = ecat_motion.M_Servo_On((short)nAxisNo, 0);
            if (ret != 0)
            {
                //string str1 = "PCIeM60板卡M_Servo_On({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Servo_On({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_ServoOn, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret)); WarningMgr.GetInstance().Error(string.Format("30103,ERR-XYT,8254 Card Aixs {0} servo on Error,result = {1}", nAxisNo, ret));
                logger.Error("PCIeM60板卡M_Servo_On({0})失败!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断开使能
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public bool ServoOff(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            int ret = ecat_motion.M_Servo_Off((short)nAxisNo, 0);
            if (ret != 0)
            {
                //string str1 = "PCIeM60板卡M_Servo_Off({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Servo_Off({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_ServoOff, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error("PCIeM60板卡M_Servo_Off({0})失败!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读取伺服使能状态
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public bool GetServoState(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            //获取轴运行状态
            int[] axStatus = new int[1];
            int ret = ecat_motion.M_GetSts((short)nAxisNo, out axStatus[0], 1, 0);

            if (ret != 0)
            {
                //string str1 = "PCIeM60板卡M_GetSts({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetSts({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error("PCIeM60板卡M_GetSts({0})失败!");
                return false;
            }
            return ((axStatus[0] & 0x200) == 0x200);
        }

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <param name="nMode">回原点参数, 对于8254，此参数代表回原点的方向</param>
        /// <returns></returns>
        public bool Home(int nAxisNo, int nMode)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            switch ((HomeMode)nMode)
            {
                case HomeMode.ORG_P:
                    nMode = 23;
                    break;

                case HomeMode.ORG_N:
                    nMode = 27;
                    break;

                case HomeMode.PEL:
                    nMode = 18;
                    break;

                case HomeMode.MEL:
                    nMode = 17;
                    break;

                case HomeMode.ORG_P_EZ:
                    nMode = 7;
                    break;

                case HomeMode.ORG_N_EZ:
                    nMode = 11;
                    break;

                case HomeMode.PEL_EZ:
                    nMode = 2;
                    break;

                case HomeMode.MEL_EZ:
                    nMode = 1;
                    break;

                case HomeMode.EZ_PEL:
                    nMode = 34;
                    break;

                case HomeMode.EZ_MEL:
                    nMode = 33;
                    break;

                default:
                    if (nMode > (int)HomeMode.BUS_BASE && nMode <= (int)HomeMode.BUS_BASE + 35)
                    {
                        nMode -= (int)HomeMode.BUS_BASE;
                    }
                    else
                    {
                        //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                        //    string.Format("PCIeM60 Card Axis {0} Home Mode Error", nAxisNo));
                        logger.Error("PCIeM60 Card Axis {0} Home Mode Error");

                        return false;
                    }
                    break;
            }

            //将驱动器运行模式设置到6 HOMEMODE
            short ret = ecat_motion.M_SetHomingMode((short)nAxisNo, 6, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_SetHomingMode({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetHomingMode({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error(str1);
                return false;
            }

            //获取回原点参数
            short Method = 0;
            int Offset = 0;
            uint Vel1 = 5000;
            uint Vel2 = 1000;
            uint Acc = 100000;
            ushort Function = 0;

            ecat_motion.M_GetHomingPrm((short)nAxisNo, ref Method, ref Offset, ref Vel1, ref Vel2, ref Acc, ref Function, 0);

            Method = (short)nMode;

            ret = ecat_motion.M_SetHomingPrm((short)nAxisNo, Method, Offset, Vel1, Vel2, Acc, Function, 0);
            if (ret != 0)
            {
                    string str1 = "PCIeM60板卡M_SetHomingPrm({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetHomingPrm({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error(str1);
                return false;
            }

            //开始执行驱动器回零,参数由文件导入
            ret = ecat_motion.M_HomingStart((short)nAxisNo, 0);
            if (ret != 0)
            {
                string str1 = $"PCIeM60板卡M_HomingStart({nAxisNo})失败! result = {ret}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_HomingStart({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error(str1);

                return false;
            }

            Thread.Sleep(100);

            return true;
        }

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="nMode"></param>
        /// <param name="vm"></param>
        /// <param name="vo"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="offset"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool Home(int nAxisNo, int nMode, double vm, double vo, double acc, double dec, double offset = 0, double sFac = 0)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            switch ((HomeMode)nMode)
            {
                case HomeMode.ORG_P:
                    nMode = 23;
                    break;

                case HomeMode.ORG_N:
                    nMode = 27;
                    break;

                case HomeMode.PEL:
                    nMode = 18;
                    break;

                case HomeMode.MEL:
                    nMode = 17;
                    break;

                case HomeMode.ORG_P_EZ:
                    nMode = 7;
                    break;

                case HomeMode.ORG_N_EZ:
                    nMode = 11;
                    break;

                case HomeMode.PEL_EZ:
                    nMode = 2;
                    break;

                case HomeMode.MEL_EZ:
                    nMode = 1;
                    break;

                case HomeMode.EZ_PEL:
                    nMode = 34;
                    break;

                case HomeMode.EZ_MEL:
                    nMode = 33;
                    break;

                default:
                    if (nMode > (int)HomeMode.BUS_BASE && nMode <= (int)HomeMode.BUS_BASE + 35)
                    {
                        nMode -= (int)HomeMode.BUS_BASE;
                    }
                    else
                    {

                        //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                        //    string.Format("PCIeM60 Card Axis {0} Home Mode Error", nAxisNo));

                        logger.Error($"PCIeM60 Card Axis {nAxisNo} Home Mode Error");
                        return false;
                    }
                    break;
            }

            //将驱动器运行模式设置到6 HOMEMODE
            short ret = ecat_motion.M_SetHomingMode((short)nAxisNo, 6, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_SetHomingMode({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetHomingMode({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error(str1);

                return false;
            }

            //获取回原点参数
            short Method = 0;
            int Offset = 0;
            uint Vel1 = 5000;
            uint Vel2 = 1000;
            uint Acc = 100000;
            ushort Function = 0;

            ecat_motion.M_GetHomingPrm((short)nAxisNo, ref Method, ref Offset, ref Vel1, ref Vel2, ref Acc, ref Function, 0);

            Method = (short)nMode;
            Offset = (int)offset;
            Vel1 = (uint)vm;
            Vel2 = (uint)vo;
            Acc = (uint)(vm / acc);

            ret = ecat_motion.M_SetHomingPrm((short)nAxisNo, Method, Offset, Vel1, Vel2, Acc, Function, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_SetHomingPrm({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetHomingPrm({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));
                logger.Error(str1);

                return false;
            }

            //开始执行驱动器回零,参数由文件导入
            ret = ecat_motion.M_HomingStart((short)nAxisNo, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_HomingStart({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_HomingStart({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Home, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 以绝对位置移动
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <param name="nPos">位置</param>
        /// <param name="nSpeed">速度</param>
        /// <returns></returns>
        public bool AbsMove(int nAxisNo, int nPos, int nSpeed)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            //开始单轴绝对运动
            int ret = ecat_motion.M_AbsMove((short)nAxisNo, nPos, nSpeed, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_AbsMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_AbsMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Abs, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 以绝对位置移动
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="fPos"></param>
        /// <param name="vm"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="vs"></param>
        /// <param name="ve"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool AbsMove(int nAxisNo, double fPos, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            //获取参数文件配置
            ecat_motion.CmdPrm cmdPrm;

            int ret = ecat_motion.M_GetMove((short)nAxisNo, out cmdPrm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            cmdPrm.acc = vm / acc;
            cmdPrm.dec = vm / dec;

            ret = ecat_motion.M_SetMove((short)nAxisNo, ref cmdPrm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_SetMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            //开始单轴绝对运动
            ret = ecat_motion.M_AbsMove((short)nAxisNo, (int)fPos, vm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_AbsMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_AbsMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Abs, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }
        /// <summary>
        /// 以当前位置为起始点进行多轴直线插补
        /// </summary>
        /// <param name="nAixsArray"></param>
        /// <param name="nPosArray"></param>
        /// <param name="vm"></param>
        /// <param name="acc">加速时间</param>
        /// <param name="dec">减速时间</param>
        /// <param name="vs"></param>
        /// <param name="ve"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool AbsLinearMove(ref int[] nAixsArray, ref double[] nPosArray, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
        {
            //轴号从1开始
            short[] axisArray = new short[nAixsArray.Length];
            for (int i = 0; i < nAixsArray.Length; i++)
            {
                axisArray[i] = (short)(nAixsArray[i] + 1);
            }

            int[] posArray = new int[nPosArray.Length];
            for (int i = 0; i < nPosArray.Length; i++)
            {
                posArray[i] = (int)nPosArray[i];
            }

            acc = vm / acc;

            int ret = ecat_motion.M_Line_All((short)axisArray.Length, ref axisArray[0], ref posArray[0], acc, vm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_Line_All失败! result = {0}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Line_All error! Result = {0}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Abs, "AbsLinearMove",
                //    string.Format(str1, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 以当前位置为起始点进行多轴直线插补
        /// </summary>
        /// <param name="nAixsArray"></param>
        /// <param name="fPosOffsetArray"></param>
        /// <param name="vm"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="vs"></param>
        /// <param name="ve"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool RelativeLinearMove(ref int[] nAixsArray, ref double[] fPosOffsetArray, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
        {
            return false;
        }

        /// <summary>
        /// 以当前点为起点做圆弧插补运动
        /// </summary>
        /// <param name="nAixsArray"></param>
        /// <param name="fCenterArray"></param>
        /// <param name="fEndArray"></param>
        /// <param name="Dir"></param>
        /// <param name="vm"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="vs"></param>
        /// <param name="ve"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool AbsArcMove(ref int[] nAixsArray, ref double[] fCenterArray, ref double[] fEndArray, int Dir, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
        {
            return false;
        }

        /// <summary>
        /// 以当前点为起点做相对圆弧插补运动
        /// </summary>
        /// <param name="nAixsArray"></param>
        /// <param name="fCenterOffsetArray"></param>
        /// <param name="fEndArray"></param>
        /// <param name="Dir"></param>
        /// <param name="vm"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="vs"></param>
        /// <param name="ve"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool RelativeArcMove(ref int[] nAixsArray, ref double[] fCenterOffsetArray, ref double[] fEndArray, int Dir, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
        {
            return false;
        }

        /// <summary>
        /// 相对位置移动
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <param name="nPos">位置</param>
        /// <param name="nSpeed">速度</param>
        /// <returns></returns>
        public bool RelativeMove(int nAxisNo, int nPos, int nSpeed)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            //开始单轴相对运动
            int ret = ecat_motion.M_RelMove((short)nAxisNo, nPos, nSpeed, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_RelMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_RelMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Rel, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 相对位置移动
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="fOffset"></param>
        /// <param name="vm"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="vs"></param>
        /// <param name="ve"></param>
        /// <param name="sFac"></param>
        /// <returns></returns>
        public bool RelativeMove(int nAxisNo, double fOffset, double vm, double acc, double dec, double vs = 0, double ve = 0, double sFac = 0)
        {

            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            //获取参数文件配置
            ecat_motion.CmdPrm cmdPrm;

            int ret = ecat_motion.M_GetMove((short)nAxisNo, out cmdPrm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            cmdPrm.acc = vm / acc;
            cmdPrm.dec = vm / dec;

            ret = ecat_motion.M_SetMove((short)nAxisNo, ref cmdPrm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_SetMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            //开始单轴相对对运动
            ret = ecat_motion.M_RelMove((short)nAxisNo, (int)fOffset, vm, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_RelMove({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_RelMove({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Rel, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// JOG运动
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <param name="bPositive">方向</param>
        /// <param name="bStart">开始标志</param>
        /// <param name="nSpeed">速度</param>
        /// <returns></returns>
        public bool JogMove(int nAxisNo, bool bPositive, int bStart, int nSpeed)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            //开始单轴JOG运动
            double vel = nSpeed;
            if (!bPositive)
            {
                vel = nSpeed * -1;
            }

            short ret = ecat_motion.M_Jog((short)nAxisNo, vel, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_Jog({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Jog({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Jog, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 轴正常停止
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public bool StopAxis(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            int ret = ecat_motion.M_Stop(Convert.ToUInt32(1 << (short)(nAxisNo - 1)), 0, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_Stop({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Stop({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 急停
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public bool StopEmg(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            int ret = ecat_motion.M_Stop(Convert.ToUInt32(1 << (short)(nAxisNo - 1)), 1, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_Stop({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Stop({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取轴卡运动状态
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public long GetMotionState(int nAxisNo)
        {
            //轴状态
            //Bit0:预留
            //Bit1:伺服报警
            //Bit2:预留
            //Bit3:预留
            //Bit4:预留
            //Bit5:正限位报警 
            //Bit6:负限位报警
            //Bit7:预留
            //Bit8:预留
            //Bit9:使能
            //Bit10:运动
            //Bit11:到位
            //Bit24:掉线        
            //从1开始
            nAxisNo += 1;

            //获取轴运行状态
            int[] axStatus = new int[1];
            int ret = ecat_motion.M_GetSts((short)nAxisNo, out axStatus[0], 1, 0);

            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetSts({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetSts({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return -1;
            }

            return axStatus[0];
        }

        /// <summary>
        /// 获取轴卡运动IO信号
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public long GetMotionIoState(int nAxisNo)
        {
            //轴状态
            //Bit0:预留
            //Bit1:伺服报警
            //Bit2:预留
            //Bit3:预留
            //Bit4:预留
            //Bit5:正限位报警 
            //Bit6:负限位报警
            //Bit7:预留
            //Bit8:预留
            //Bit9:使能
            //Bit10:运动
            //Bit11:到位
            //Bit24:掉线        
            //从1开始
            nAxisNo += 1;

            //获取轴运行状态
            int[] axStatus = new int[1];
            int ret = ecat_motion.M_GetSts((short)nAxisNo, out axStatus[0], 1, 0);

            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetSts({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetSts({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return -1;
            }

            uint digitalInput = 0;
            ret = ecat_motion.M_GetEcatDigitalInput((short)nAxisNo, out digitalInput, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetEcatDigitalInput({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetEcatDigitalInput({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return -1;
            }

            // 8254 motion io table
            // |-bit0-|--1--|--2--|--3--|--4--|--5--|--6--|--7--|--8--|...|--11--|--12--|
            // |-ALM--|-PEL-|-MEL-|-ORG-|-EMG-|-EZ--|-INP-|-SVO-|-RDY-|...|-SPEL-|-SMEL-|
            long nStdIo = 0;
            if ((axStatus[0] & (0x01 << 1)) != 0)
                nStdIo |= (0x01 << 0);

            if ((axStatus[0] & (0x01 << 5)) != 0)
                nStdIo |= (0x01 << 1);

            if ((axStatus[0] & (0x01 << 6)) != 0)
                nStdIo |= (0x01 << 2);

            if ((digitalInput & (0x01 << 2)) != 0)
                nStdIo |= (0x01 << 3);

            if ((axStatus[0] & (0x01 << 11)) != 0)
                nStdIo |= (0x01 << 6);

            if ((axStatus[0] & (0x01 << 9)) != 0)
                nStdIo |= (0x01 << 7);

            if ((axStatus[0] & (0x01 << 24)) == 0)
                nStdIo |= (0x01 << 8);

            //报警以及限位状态需要用 M_ClrSts清除
            if ((axStatus[0] & (0x01 << 1)) != 0 || (axStatus[0] & (0x01 << 5)) != 0 || (axStatus[0] & (0x01 << 6)) != 0)
            {
                ClearError(nAxisNo - 1);
            }

            return nStdIo;
        }

        /// <summary>
        /// 清除错误报警
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <returns></returns>
        public bool ClearError(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            int ret = ecat_motion.M_ClrSts((short)nAxisNo, 1, 0);
            if (ret != 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取轴的当前位置
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public double GetAixsPos(int nAxisNo)
        {
            //从1开始
            nAxisNo += 1;

            double[] pPrfPos = new double[1];
            int ret = ecat_motion.M_GetEncPos((short)nAxisNo, out pPrfPos[0], 1, 0);
            if (ret != 0)
            {
                return -1;
            }

            return (long)pPrfPos[0];
        }

        /// <summary>
        /// 轴是否正常停止
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns>0:正常停止, -1:未到位 其它:急停,报警等</returns>
        public int IsAxisNormalStop(int nAxisNo)
        {
            //轴状态
            //Bit0:预留
            //Bit1:伺服报警
            //Bit2:预留
            //Bit3:预留
            //Bit4:预留
            //Bit5:正限位报警 
            //Bit6:负限位报警
            //Bit7:预留
            //Bit8:预留
            //Bit9:使能
            //Bit10:运动
            //Bit11:到位
            //Bit24:掉线        
            //从1开始

            //正负限位报警不能自动清掉，需要手动清掉
            ClearError(nAxisNo);

            nAxisNo += 1;

            //获取轴运行状态
            int[] axStatus = new int[1];
            int ret = ecat_motion.M_GetSts((short)nAxisNo, out axStatus[0], 1, 0);

            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetSts({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetSts({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_State, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return -1;
            }

            if ((axStatus[0] & (0x01 << 1)) != 0)//急停
            {
                Debug.WriteLine("Axis {0} have Emg signal \r\n", nAxisNo);
                return 1;
            }
            else if ((axStatus[0] & (0x01 << 24)) != 0)//报警
            {
                Debug.WriteLine("Axis {0} have sevo alarm signal \r\n", nAxisNo);
                return 2;
            }
            else if ((axStatus[0] & (0x01 << 9)) == 0)//Servo off
            {
                Debug.WriteLine("Axis {0} have servo signal \r\n", nAxisNo);
                return 3;
            }
            else if ((axStatus[0] & (0x01 << 5)) != 0)//正向硬限位 
            {
                Debug.WriteLine("Axis {0} have PEL signal \r\n", nAxisNo);
                return 4;
            }
            else if ((axStatus[0] & (0x01 << 6)) != 0)//负向硬限位 
            {
                Debug.WriteLine("Axis {0} have MEL signal \r\n", nAxisNo);
                return 5;
            }
            else if ((axStatus[0] & (0x01 << 10)) != 0
                || (axStatus[0] & (0x01 << 11)) == 0)//未到位
            {
                return -1;//未完成
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 判断轴是否到位
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="nInPosError"></param>
        /// <returns></returns>
        public int IsAxisInPos(int nAxisNo, int nInPosError = 1000)
        {
            int nRet = IsAxisNormalStop(nAxisNo);
            if (nRet == 0)
            {
                //从1开始
                nAxisNo += 1;

                double[] pPrfPos = new double[1];
                double[] pEncPos = new double[1];
                int ret = ecat_motion.M_GetCmd((short)nAxisNo, out pPrfPos[0], 1, 0);
                if (ret != 0)
                    return -1;
                ret = ecat_motion.M_GetEncPos((short)nAxisNo, out pEncPos[0], 1, 0);
                if (ret != 0)
                    return -1;

                if (Math.Abs(pPrfPos[0] - pEncPos[0]) > nInPosError)
                    return 6;  //轴停止后位置超限
            }
            return nRet;
        }

        /// <summary>
        /// 位置清零
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <returns></returns>
        public bool SetPosZero(int nAxisNo)
        {
            nAxisNo += 1;

            //获取回原点参数
            short Method = 0;
            int Offset = 0;
            uint Vel1 = 5000;
            uint Vel2 = 1000;
            uint Acc = 100000;
            ushort Function = 0;

            ecat_motion.M_GetHomingPrm((short)nAxisNo, ref Method, ref Offset, ref Vel1, ref Vel2, ref Acc, ref Function, 0);

            Method = 35;

            int ret = ecat_motion.M_SetHomingPrm((short)nAxisNo, Method, Offset, Vel1, Vel2, Acc, Function, 0);

            //将驱动器运行模式设置到6 HOMEMODE
            ret = ecat_motion.M_SetHomingMode((short)nAxisNo, 6, 0);

            //开始执行驱动器回零,参数由文件导入
            ret = ecat_motion.M_HomingStart((short)nAxisNo, 0);

            return true;
        }

        /// <summary>
        /// 是否回零模式
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <returns></returns>
        private bool IsHomeMode(int nAxisNo)
        {
            nAxisNo += 1;
            //切换到8 csp模式，并且获取当前的模式状态，确认切换成功（主要针对部分相应慢的伺服）
            short sts = 0;
            ecat_motion.M_EcatGetOperationMode((short)nAxisNo, ref sts, 0);

            return sts == 6;
        }

        /// <summary>
        /// 设置单轴的某一运动参数
        /// </summary>
        /// <param name="nAxisNo">轴号</param>
        /// <param name="nParam">参数:1:加速度 2:减速度 3:起跳速度 4:结束速度(凌华卡) 5:平滑时间(固高卡S曲线) 其它：自定义扩展</param>
        /// <param name="nData">参数值</param>
        /// <returns></returns>
        public bool SetAxisParam(int nAxisNo, int nParam, int nData)
        {

            return true;
        }

        /// <summary>
        /// 速度模式旋转轴
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="nSpeed"></param>
        /// <returns></returns>
        public bool VelocityMove(int nAxisNo, int nSpeed)
        {
            if (IsHomeMode(nAxisNo))
            {
                ecat_motion.M_SetHomingMode((short)(nAxisNo + 1), 8, 0);
            }

            //从1开始
            nAxisNo += 1;

            //开始单轴JOG运动
            double vel = nSpeed;
            short ret = ecat_motion.M_Jog((short)nAxisNo, vel, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_Jog({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Jog({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion_Vel, GetSysAxisNo(nAxisNo).ToString(),
                //    string.Format(str1, nAxisNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        ///此函数8254板卡不提供不使用,回原点内部已经封装好过程处理 
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <returns></returns>
        public int IsHomeNormalStop(int nAxisNo)
        {
            int nRet = IsAxisNormalStop(nAxisNo);
            if (nRet == 4 || nRet == 5)
            {
                //以正负限位作为原点时不能判断
                nRet = -1;
            }

            if (nRet == 0)
            {
                //从1开始
                nAxisNo += 1;

                short sts = 0;
                do
                {
                    Thread.Sleep(10);
                    //切换到8 csp模式，并且获取当前的模式状态，确认切换成功（主要针对部分相应慢的伺服）
                    ecat_motion.M_SetHomingMode((short)nAxisNo, 8, 0);
                    ecat_motion.M_EcatGetOperationMode((short)nAxisNo, ref sts, 0);
                } while (sts != 8);

                ecat_motion.M_ClrSts((short)nAxisNo, 1, 0);
            }

            return nRet;
        }

        /// <summary>
        /// 配置连续插补运动，最多只支持三轴
        /// </summary>
        /// <param name="crdNo">坐标系</param>
        /// <param name="nAixsArray">参与插补运动的轴</param>
        /// <param name="bAbsolute">true:绝对位置模式，　false:相对位置模式</param>
        /// <returns></returns>
        public bool ConfigPointTable(int crdNo, ref int[] nAixsArray, bool bAbsolute)
        {
            ecat_motion.CrdCfg crdCfg;
            short ret = ecat_motion.M_GetCrd((short)crdNo, out crdCfg, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetCrd({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetCrd({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }

            crdCfg.axis = new short[8];
            crdCfg.orignPos = new int[8];
            crdCfg.dimension = (short)nAixsArray.Length;
            for (int i = 0; i < nAixsArray.Length; i++)
            {
                crdCfg.axis[i] = (short)(nAixsArray[i] + 1);

                if (bAbsolute)
                {
                    crdCfg.orignPos[i] = 0;
                }
                else
                {
                    crdCfg.orignPos[i] = (int)GetAixsPos(nAixsArray[i]);
                }
            }

            ret = ecat_motion.M_SetCrd((short)crdNo, ref crdCfg, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_SetCrd({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_SetCrd({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }

            //清除FIFO缓存
            ret = ecat_motion.M_CrdClear((short)crdNo, 1, 0, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_CrdClear({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_CrdClear({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 连续直线插补运动
        /// </summary>
        /// <param name="crdNo">坐标系</param>
        /// <param name="positionArray">运动点位</param>
        /// <param name="acc">加速时间，ms</param>
        /// <param name="dec">减速时间, ms</param>
        /// <param name="vs">开始速度</param>
        /// <param name="vm">运行速度</param>
        /// <param name="ve">结束速度</param>
        /// <param name="sf">平滑系数</param>
        /// <returns></returns>
        public bool PointTable_Line_Move(int crdNo, ref double[] positionArray, double acc, double dec, double vs, double vm, double ve, double sf)
        {
            ecat_motion.CrdCfg crdCfg;
            short ret = ecat_motion.M_GetCrd((short)crdNo, out crdCfg, 0);
            if (ret != 0)
            {
                string str1 = "PCIeM60板卡M_GetCrd({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetCrd({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }

            int[] posArray = new int[positionArray.Length];
            for (int i = 0; i < posArray.Length; i++)
            {
                posArray[i] = (int)positionArray[i];
            }

            acc = vm / acc;
            dec = vm / dec;

            ret = ecat_motion.M_Line((short)crdNo, crdCfg.dimension, ref crdCfg.axis[0], ref posArray[0], vm, acc, ve);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_Line({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_Line({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crdNo"></param>
        /// <param name="centerArray"></param>
        /// <param name="endArray"></param>
        /// <param name="dir"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="vs"></param>
        /// <param name="vm"></param>
        /// <param name="ve"></param>
        /// <param name="sf"></param>
        /// <returns></returns>
        public bool PointTable_ArcE_Move(int crdNo, ref double[] centerArray, ref int[] midPosArray, ref int[] endArray, short dir, double acc, double dec, double vs, double vm, double ve, double sf)
        {
            ecat_motion.CrdCfg crdCfg;
            short ret = ecat_motion.M_GetCrd((short)crdNo, out crdCfg, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_GetCrd({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_GetCrd({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }

            int[] posArray = new int[endArray.Length];
            for (int i = 0; i < posArray.Length; i++)
            {
                posArray[i] = (int)endArray[i];
            }

            acc = vm / acc;
            dec = vm / dec;

            if (crdCfg.dimension == 2)
            {
                ret = ecat_motion.M_Arc2C((short)crdNo, ref crdCfg.axis[0], ref posArray[0], ref centerArray[0], dir, vm, acc);

                if (ret != 0)
                {

                    string str1 = "PCIeM60板卡M_Arc2C({0})失败! result = {1}";
                    //if (LanguageMgr.GetInstance().LanguageID != 0)
                    //{
                    //    str1 = "PCIeM60 board card M_Arc2C({0}) error! Result = {1}";
                    //}

                    //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                    //    string.Format(str1, crdNo, ret));

                    logger.Error(str1);
                    return false;
                }
            }
            else if (crdCfg.dimension == 3)
            {
                ret = ecat_motion.M_Arc3D((short)crdNo, ref endArray[0], ref midPosArray[0], vm, acc, ve);

                if (ret != 0)
                {
                    string str1 = "PCIeM60板卡M_Arc3D({0})失败! result = {1}";
                    //if (LanguageMgr.GetInstance().LanguageID != 0)
                    //{
                    //    str1 = "PCIeM60 board card M_Arc3D({0}) error! Result = {1}";
                    //}

                    //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                    //    string.Format(str1, crdNo, ret));

                    logger.Error(str1);
                    return false;
                }
            }
            else
            {
                string str1 = "PCIeM60板卡PointTable_ArcE_Move({0})失败! 仅支持2D/3D！";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card PointTable_ArcE_Move({0}) error! Only support 2D/3D!";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo));

                logger.Error(str1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 插补缓冲区输出DO,在插入位置之后插入
        /// </summary>
        /// <param name="crdNo">坐标系号</param>
        /// <param name="nChannel">输出DO的端口号</param>
        /// <param name="bOn">输出DO的电平，0低电平，1 高电平</param>
        /// <returns></returns>
        public bool PointTable_IO(int crdNo, int nChannel, int bOn)
        {
            short ret = ecat_motion.M_BufIO((short)crdNo, (ushort)nChannel, (ushort)bOn, 0, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_BufIO({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_BufIO({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crdNo"></param>
        /// <param name="nMillsecond"></param>
        /// <returns></returns>
        public bool PointTable_Delay(int crdNo, int nMillsecond)
        {
            short ret = ecat_motion.M_BufDelay((short)crdNo, (uint)nMillsecond, 0, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_BufDelay({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_BufDelay({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));

                logger.Error(str1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 确定连续运动列表的BUFF是否已满
        /// </summary>
        /// <param name="crdNo"></param>
        /// <returns></returns>
        public bool PointTable_IsIdle(int crdNo)
        {
            short sts, cmdNum;
            int space;
            short ret = ecat_motion.M_CrdStatus((short)crdNo, out sts, out cmdNum, out space, 0, 0);
            if (ret != 0)
            {

                string str1 = "PCIeM60板卡M_BufDelay({0})失败! result = {1}";
                //if (LanguageMgr.GetInstance().LanguageID != 0)
                //{
                //    str1 = "PCIeM60 board card M_BufDelay({0}) error! Result = {1}";
                //}

                //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                //    string.Format(str1, crdNo, ret));
 
                logger.Error(str1);
                return false;
            }

            return space > 0;
        }

        /// <summary>
        /// 启动或停止一个连续运动
        /// </summary>
        /// <param name="crdNo"></param>
        /// <param name="bStart"></param>
        /// <returns></returns>
        public bool PointTable_Start(int crdNo, bool bStart)
        {
            short ret = 0;
            if (bStart)
            {
                ret = ecat_motion.M_CrdStart((short)(0x01 << crdNo), 1, 0);

                if (ret != 0)
                {

                    string str1 = "PCIeM60板卡M_CrdStart({0})失败! result = {1}";
                    //if (LanguageMgr.GetInstance().LanguageID != 0)
                    //{
                    //    str1 = "PCIeM60 board card M_CrdStart({0}) error! Result = {1}";
                    //}

                    //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                    //    string.Format(str1, crdNo, ret));

                    logger.Error(str1);
                    return false;
                }
            }
            else
            {
                //0 - 平滑停止 1 - 急停
                ret = ecat_motion.M_CrdStop((short)(0x01 << crdNo), 0, 0);

                if (ret != 0)
                {

                    string str1 = "PCIeM60板卡M_CrdStop({0})失败! result = {1}";
                    //if (LanguageMgr.GetInstance().LanguageID != 0)
                    //{
                    //    str1 = "PCIeM60 board card M_CrdStop({0}) error! Result = {1}";
                    //}

                    //WarningMgr.GetInstance().Error(ErrorType.Err_Motion, crdNo.ToString(),
                    //    string.Format(str1, crdNo, ret));

                    logger.Error(str1);
                    return false;
                }
            }
            return true;
        }


        /// 启用软件正限位
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="bEnable"></param>
        /// <returns></returns>
        public bool SetSPELEnable(int nAxisNo, bool bEnable)
        {
            return true;
        }

        /// <summary>
        /// 启用软件负限位
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="bEnable"></param>
        /// <returns></returns>
        public bool SetSMELEnable(int nAxisNo, bool bEnable)
        {
            return true;
        }

        /// <summary>
        /// 设置软件正限位位置
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool SetSPELPos(int nAxisNo, double pos)
        {
            //从1开始
            nAxisNo += 1;

            int nSoftPosLimit = 0, nSoftNegLimit = 0;
            int ret = ecat_motion.M_GetSoftLimit((short)nAxisNo, out nSoftPosLimit, out nSoftNegLimit, 0);

            ret = ecat_motion.M_SetSoftLimit((short)nAxisNo, (int)pos, nSoftNegLimit, 0);

            return ret == 0;
        }

        /// <summary>
        /// 设置软件负限位位置
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool SetSMELPos(int nAxisNo, double pos)
        {
            //从1开始
            nAxisNo += 1;

            int nSoftPosLimit = 0, nSoftNegLimit = 0;
            int ret = ecat_motion.M_GetSoftLimit((short)nAxisNo, out nSoftPosLimit, out nSoftNegLimit, 0);

            ret = ecat_motion.M_SetSoftLimit((short)nAxisNo, nSoftPosLimit, (int)pos, 0);

            return ret == 0;
        }

        #endregion
    }
}
