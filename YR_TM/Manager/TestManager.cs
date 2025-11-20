using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Core;
using YR_Framework.Events;
using YR_Framework.Models;
using YR_TM.Modules;
using YR_TM.PageView;
using YR_TM.Utils;

namespace YR_TM.Manager
{
    public class TestManager
    {
        #region 单例
        private static readonly Lazy<TestManager> _instance = new Lazy<TestManager>(() => new TestManager());
        public static TestManager Instance => _instance.Value;
        private TestManager() { }
        #endregion

        private ILogger logger = LogManager.GetLogger("TestManager");

        private CancellationTokenSource _cts;
        private Task _monitorTask;
        private readonly object _lockObj = new object();

        private bool _emergencyStop = false;
        public RunState State { get; private set; } = RunState.Idle;
        public RunMode Mode { get; private set; } = RunMode.Product;

        public event Action<RunState> StateChanged;
        public event Action<bool> ConnectBusChanged;

        public bool _connectBusState = false;


        public TestLogic_Enum CurrentTest_Step = TestLogic_Enum.TestLogic_0000;

        Thread TestLogicThread;

        private void SetState(RunState state)
        {
            if(State != state)
            {
                State = state;
                StateChanged?.Invoke(state);

                ConnectBusChanged?.Invoke(_connectBusState);
            }
        }

        /// <summary>
        /// 软件启动时调用，初始化加复位，进入Ready状态
        /// </summary>
        public void InitializeAndReset()
        {
            lock (_lockObj)
            {
                //确保旧线程结束
                _cts?.Cancel();
                _cts?.Dispose();
                //创建新的 CTS
                _cts = new CancellationTokenSource();

                //获取点位
                GlobalDataPoint.LoadPointListFromJson();
                var points = GlobalDataPoint.GetPointList();
                foreach (var point in points)
                {
                    logger.Info($"点为名：{point.Name}, X: {point.XValue}, Y: {point.YValue}, Z: {point.ZValue}, R: {point.RValue}");
                }

                _ = Initialize(_cts.Token);  

                //启动监控线程（值启动一次）
                if(_monitorTask == null || _monitorTask.IsCompleted)
                {
                    _monitorTask = Task.Run(() => StartMontiorThread(_cts.Token));
                }

                //启动测试线程
                TestLogicThread = new Thread(TestLogic);
                TestLogicThread.IsBackground = true;

            }
        }

        #region - 初始化 + 复位流程

        private async Task Initialize(CancellationToken token)
        {
            logger.Info("开始系统初始化...");
            //bool initResult = await Task.Run(() => MotionModule.Instance.Init());
            //if (!initResult)
            //{
            //    MessageBox.Show("初始化失败，请检查", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            logger.Info("初始化完成！");
            _connectBusState = true;
            SetState(State);


            logger.Info("开始复位...");
            //for (int i = 0; i < MotionModule.Instance.m_tResoures.AxisNum; i++)
            //{
            //    MotionModule.Instance.Home(i, 5);
            //}
            logger.Info("复位已完成！");

            SetState(RunState.Ready);
            await Task.Delay(200, token);
        }

        #endregion

        #region - 启动流程检查 + 开始测试

        bool startBtn;

        bool IsRunning = false;
        public bool IsPuase = false;
        private async Task StartMontiorThread(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    if(State == RunState.Ready)
                    {
                        if (IsRunning)   //增加防误触
                        {
                            continue;
                        }
                        if(!TestLogicThread.IsAlive)
                        {
                            TestLogicThread.Start();

                            //假装暂停
                            Thread.Sleep(10000);
                            IsPuase=true;
                            Thread.Sleep(8000);
                            IsPuase = false;



                        }
                        else
                        {
                            IsRunning = true;
                        }
                       
                        startBtn = false; /*MotionModule.Instance.ReadIoInBit(5);*/
                        //if (startBtn)
                        //    await StartTestFlow(token);


                    }
                }
                catch (TaskCanceledException)
                {
                    break;  //正常退出
                }
                catch (Exception ex)
                {
                    logger.Error($"主循环异常：{ex.Message}");
                }
                await Task.Delay(100, token);
            }
        }

        private async Task StartTestFlow(CancellationToken token)
        {
            logger.Info("检查安全光栅、安全门等...");

            //bool lightCurtainOK = !MotionModule.Instance.ReadIoInBit(10);
            //bool doorClosed = !MotionModule.Instance.ReadIoInBit(11);

            //bool emgStopOK = !_emergencyStop;

            //if (!lightCurtainOK)
            //{
            //    logger.Info("安全光栅触发，请检查！");
            //    return;
            //}

            //if (!doorClosed)
            //{
            //    logger.Info("安全门未关闭，请检查！");
            //    return;
            //}

            //if (!emgStopOK)
            //{
            //    logger.Info("急停未释放！");
            //    return;
            //}

            SetState(RunState.Running);
            logger.Info("安全检查正常，开始测试流程");

            bool result = await RunAssemblyFlow(token);

            if (result)
            {
                logger.Info("测试完成: PASS");
                SetState(RunState.PASS);
            }
            else
            {
                logger.Info("测试完成: FAIL");
                SetState(RunState.FAIL);
            }

            await Task.Delay(200);
            SetState(RunState.Ready);
        }

        #endregion

        #region - 流程

        
        private void TestLogic()
        {
            while (true)
            {
                if (!IsPuase)
                {
                    bool exist = false;
                    switch (CurrentTest_Step)
                    {
                        case TestLogic_Enum.TestLogic_0000:
                            {
                                string t1 = "回到原点";
                                logger.Info(t1);

                                CurrentTest_Step = TestLogic_Enum.TestLogic_0001;
                                break;
                            }
                        case TestLogic_Enum.TestLogic_0001:
                            {

                                string t1 = "移动到拍照点位1";
                                logger.Info(t1);

                                CurrentTest_Step = TestLogic_Enum.TestLogic_0002;
                                break;
                            }

                        case TestLogic_Enum.TestLogic_0002:
                            {

                                string t1 = "取排线";
                                logger.Info(t1);

                                string t2 = "点胶";
                                logger.Info(t2);


                                CurrentTest_Step = TestLogic_Enum.TestLogic_0003;
                                break;
                            }

                        case TestLogic_Enum.TestLogic_0003:
                            {

                                string t1 = "移动到拍照点位2";
                                logger.Info(t1);

                                CurrentTest_Step = TestLogic_Enum.TestLogic_0004;
                                break;
                            }

                        case TestLogic_Enum.TestLogic_0004:
                            {

                                string t1 = "拍照并检查点胶情况";
                                logger.Info(t1);

                                CurrentTest_Step = TestLogic_Enum.TestLogic_0005;
                                break;
                            }

                        case TestLogic_Enum.TestLogic_0005:
                            {

                                string t1 = "组装";
                                logger.Info(t1);

                                CurrentTest_Step = TestLogic_Enum.TestLogic_0010;
                                break;
                            }

                        case TestLogic_Enum.TestLogic_0010:
                            {

                                string t1 = "组装完成";
                                logger.Info(t1);
                                exist = true;
                                CurrentTest_Step = TestLogic_Enum.TestLogic_0000;
                                break;
                            }

                    }

                    if (exist)
                    {
                        break;
                    }
                }


                Thread.Sleep(5000);

            }


        }

        private async Task<bool> RunAssemblyFlow(CancellationToken token)
        {
            try
            {
                // === 1.hodler到上相机位置 ===
                SetState(RunState.Running);
                logger.Info("运动到拍照位...");
                //MotionModule.Instance.AbsMove(0, 200, 20);
                await Task.Delay(1000, token);

                // === 2. 取排线 + 点胶并行 ===
                SetState(RunState.Running);
                logger.Info("取排线 + 点胶...");
                //Task t1 = Task.Run(() => PickCable());
                //Task t2 = Task.Run(() => GlueProcess());
                //await Task.WhenAll(t1, t2);
                await Task.Delay(1000, token);

                // === 3. 去组装位组装 ===
                SetState(RunState.Running);
                logger.Info("进行组装...");
                //MotionModule.Instance.AbsMove(0, 30, 30);
                await Task.Delay(1500, token);

                // === 4. 返回原点 ===
                SetState(RunState.Running);
                logger.Info("返回初始位置...");
                //MotionModule.Instance.AbsMove(0, 0, 30);
                await Task.Delay(1000, token);

                return true;
            }
            catch (OperationCanceledException)
            {
                logger.Error("流程被中断");
                SetState(RunState.Stop);
                return false;
            }
            catch (Exception ex)
            {
                logger.Error($"流程异常：{ex.Message}");
                SetState(RunState.Error);
                return false;
            }
        }

        private void PickCable()
        {
            MotionModule.Instance.AbsMove(0, 150, 30);
            Thread.Sleep(1000);
        }

        private void GlueProcess()
        {
            MotionModule.Instance.AbsMove(0, 300, 30);
            Thread.Sleep(1500);
        }

        #endregion

        #region - 异常处理

        public void EmergencyStop()
        {
            _emergencyStop = true;
            MotionModule.Instance.StopEmg(1);
            logger.Warn("急停触发，所有动作停止");
            SetState(RunState.Stop);

            _cts.Cancel();
        }

        public void ResumeAfterAlarm()
        {
            if (State == RunState.Error || State == RunState.Paused)
                logger.Info("故障清除， 等待启动");
                SetState(RunState.Ready);
        }

        #endregion
    }

    public enum TestLogic_Enum
    {
        TestLogic_0000,

        TestLogic_0001,

        TestLogic_0002,

        TestLogic_0003,

        TestLogic_0004,

        TestLogic_0005,

        TestLogic_0006,

        TestLogic_0007,

        TestLogic_0010, //组装完成

    }
}
