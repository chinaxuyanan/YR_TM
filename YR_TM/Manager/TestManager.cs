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

        private async Task StartMontiorThread(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    if(State == RunState.Ready)
                    {
                        bool startBtn = false; /*MotionModule.Instance.ReadIoInBit(5);*/
                        if (startBtn)
                            await StartTestFlow(token);
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
}
