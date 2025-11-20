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
        private bool _isPaused = false;
        private bool _isStopping = false;

        public RunState State { get; private set; } = RunState.Idle;
        public static RunMode Mode = RunMode.Product;

        public event Action<RunState> StateChanged;
        public event Action<bool> ConnectBusChanged;

        public bool _connectBusState = false;
        private TestStep currentStep = TestStep.None;

        //IO
        private const int StartButtonIO = 0;
        private const int StopButtonIO = 1;
        private const int RsetButtonIO = 2;


        private void SetState(RunState state)
        {
            if (State != state)
            {
                State = state;
                try { StateChanged?.Invoke(state); } catch { }

                try { ConnectBusChanged?.Invoke(_connectBusState); } catch { }
            }
        }

        #region - 初始化 + 复位流程

        public void Initialize()
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

            //获取点位
            GlobalDataPoint.LoadPointListFromJson();
            var points = GlobalDataPoint.GetPointList();
            foreach (var point in points)
            {
                logger.Info($"点为名：{point.Name}, X: {point.XValue}, Y: {point.YValue}, Z: {point.ZValue}, R: {point.RValue}");
            }


            logger.Info("开始复位...");
            //for (int i = 0; i < MotionModule.Instance.m_tResoures.AxisNum; i++)
            //{
            //    MotionModule.Instance.Home(i, 5);
            //}
            logger.Info("复位已完成！");

            SetState(RunState.Ready);
        }

        #endregion

        /// <summary>
        /// 软件启动时调用，初始化加复位，进入Ready状态
        /// </summary>
        public void StartTest()
        {
            lock (_lockObj)
            {
                //确保旧线程结束
                _cts?.Cancel();
                _cts?.Dispose();
                //创建新的 CTS
                _cts = new CancellationTokenSource();

                _isPaused = false;
                _isStopping = false;
                _emergencyStop = false;

                //启动监控线程（启动一次）
                if (_monitorTask == null || _monitorTask.IsCompleted)
                {
                    _monitorTask = Task.Run(() => StartMontiorThread(_cts.Token));
                }
            }
        }

        #region - 启动流程检查 + 开始测试

        private async Task StartMontiorThread(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    //急停或停止，等待按钮信号
                    if(_isStopping || _emergencyStop)
                    {
                        await Task.Delay(200, token);
                        continue;
                    }

                    //生产模式
                    if ((State == RunState.Ready || State == RunState.PASS || State == RunState.FAIL) && Mode == RunMode.Product)
                    {
                        bool startPressed = true; /*MotionModule.Instance.ReadIoInBit(StartButtonIO);*/
                        if (startPressed)
                        {
                            if (!WaitForDoorClosed())
                            {
                                SetState(RunState.Error); await Task.Delay(500, token); continue;
                            }

                            SetState(RunState.Running);
                            bool result = await StartTestFlow(token);
                            SetState(result ? RunState.PASS : RunState.FAIL);

                            if (!_emergencyStop && !_isStopping)
                                SetState(RunState.Ready);
                        }
                    }
                    else if ((State == RunState.Ready || State == RunState.PASS) && Mode == RunMode.DryRun)
                    {
                        //空跑
                    }
                    else
                    {
                        //调试
                    }
                    await Task.Delay(5000, token);
                }
            }catch(OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Error($"异常：{ex.Message}");
            }
            finally
            {
                logger.Info("结束");
            }
            
        }

        private bool WaitForDoorClosed()
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
            return true;
        }

        private async Task<bool> StartTestFlow(CancellationToken token)
        {
            try
            {
                bool result = await RunAssemblyFlow(token);
                return result;
            }
            catch (OperationCanceledException)
            {
                logger.Info("StartTestFlow 取消");
                return false;
            }
            catch(Exception ex)
            {
                logger.Error($"异常: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region - 流程

        private async Task<bool> RunAssemblyFlow(CancellationToken token)
        {
            try
            {
                if (currentStep == TestStep.None) currentStep = TestStep.Step_0001;

                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    if (_emergencyStop)
                    {
                        logger.Warn("急停，退出");
                        SetState(RunState.Stop);
                        return false;
                    }
                    if (_isPaused)
                    {
                        logger.Info("暂停，等待...");
                        //MotionModule.Instance.StopAxis(3);
                        SetState(RunState.Paused);

                        while(_isPaused && !token.IsCancellationRequested && !_emergencyStop && !_isStopping)
                        {
                            await Task.Delay(2000, token);
                        }

                        if(token.IsCancellationRequested || _emergencyStop || _isStopping)
                        {
                            logger.Info("取消/停止/急停，退出");
                            return false;
                        }

                        logger.Info("恢复，继续步骤" + currentStep);
                        SetState(RunState.Running);
                    }

                    switch (currentStep)
                    {
                        case TestStep.Step_0001:
                            // === 1.hodler到上相机位置 ===
                            logger.Info("运动到拍照位...");
                            //MotionModule.Instance.AbsMove(0, 200, 20);
                            await Task.Delay(2000, token);
                            currentStep = TestStep.Step_0002;
                            break;
                        case TestStep.Step_0002:
                            // === 2. 取排线 + 点胶并行 ===
                            logger.Info("取排线 + 点胶...");
                            //Task t1 = Task.Run(() => PickCable());
                            //Task t2 = Task.Run(() => GlueProcess());
                            //await Task.WhenAll(t1, t2);
                            await Task.Delay(3000, token);
                            currentStep = TestStep.Step_0003;
                            break;
                        case TestStep.Step_0003:
                            // === 3. 去组装位组装 ===
                            logger.Info("进行组装...");
                            //MotionModule.Instance.AbsMove(0, 30, 30);
                            await Task.Delay(4500, token);
                            currentStep = TestStep.Step_0004;
                            break;
                        case TestStep.Step_0004:
                            // === 4. 返回原点 ===
                            logger.Info("返回初始位置...");
                            //MotionModule.Instance.AbsMove(0, 0, 30);
                            await Task.Delay(3000, token);
                            currentStep = TestStep.None;
                            logger.Info("流程执行完毕");
                            break;
                        default:
                            logger.Warn("结束流程");
                            return false;
                    }

                    await Task.Delay(10, token);
                }
            }
            catch (OperationCanceledException)
            {
                logger.Info("取消流程");
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
        
        public void StopTest()
        {
            _isStopping = true;
            _isPaused = false;
            _emergencyStop = false;
            _cts.Cancel();
            SetState(RunState.Stop);
        }

        public void PauseTest()
        {
            _isPaused = true;
            SetState(RunState.Paused);
            logger.Info("测试暂停");
        }

        public void ResetTest()
        {
            _isPaused = false;
            _isStopping = false;
            SetState(RunState.Running);
            logger.Info("收到复位信号继续执行");
        }

        public void EmergencyStop()
        {
            _emergencyStop = true;
            _isPaused = false;
            _isStopping = true;
            _cts.Cancel();
            SetState(RunState.EmerStop);
        }

        #endregion
    }
}
