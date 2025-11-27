using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        #region - 单例
        private static readonly Lazy<TestManager> _instance = new Lazy<TestManager>(() => new TestManager());
        public static TestManager Instance => _instance.Value;
        private TestManager() { }
        #endregion

        private readonly ILogger logger = LogManager.GetLogger("TestManager");

        private readonly StateMachine _stateMachine = new StateMachine();
        private readonly FlowController _flow = new FlowController();
        private ButtonHandler _buttons;

        public static RunMode Mode { get; set; } = RunMode.Product;

        private CancellationTokenSource _cts;
        private Task _mainLoop;

        public event Action<RunState> StateChanged;
        public event Action<bool> ConnectBusChanged;

        public static bool _connectBusState = false;

        public RunState State => _stateMachine.State;

        public void Initialize()
        {
            AppState.IsBusConnected = MotionModule.Instance.Init();
            _flow.ResetStep();
            GetOriginPointAndReset();
            _stateMachine.SetState(RunState.Ready);
            StateChanged?.Invoke(State);
        }

        public void Start()
        {
            _buttons = new ButtonHandler(_stateMachine, _flow, logger);
            _cts = new CancellationTokenSource();
            _mainLoop = Task.Run(() => MainLoop(_cts.Token));
        }

        private async Task MainLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_stateMachine.IsRunning)
                {
                    bool result = await _flow.StartTestFlowAsync(token, _stateMachine);
                    _stateMachine.SetState(result ? RunState.PASS : RunState.FAIL);
                    StateChanged?.Invoke(State);
                }

                await Task.Delay(50, token);
            }
        }

        private void GetOriginPointAndReset()
        {
            //获取点位
            GlobalDataPoint.LoadPointListFromJson();
            var points = GlobalDataPoint.GetPointList();
            foreach (var point in points)
            {
                //获取原点
                logger.Info($"点为名：{point.Name}, X: {point.XValue}, Y: {point.YValue}, Z: {point.ZValue}, R: {point.RValue}");
            }

            for (int axis = 1; axis <= MotionModule.Instance.m_tResoures.AxisNum; axis++)
            {
                MotionModule.Instance.ClearError(axis);
                MotionModule.Instance.ServoOn(axis);
                MotionModule.Instance.Home(axis, (int)HomeMode.ORG_N);

                bool isHomeingComplete = (MotionModule.Instance.GetMotionState(axis) & 0x20000) == 0x20000; //检查bit17是否为1
                if (isHomeingComplete)
                {
                    logger.Info($"轴 {axis} 回零完成！");
                    if (MotionModule.Instance.m_tResoures.AxisNum > 3)
                        MotionModule.Instance.AbsMove((int)AxisInfo.TAMAxis.Y, 0, 10000);
                    else
                        MotionModule.Instance.AbsMove((int)AxisInfo.MAGAxis.Y, 0, 10000);
                }
            }
        }

        ///<summary>
        ///外部接收到按钮事件 --》 转给按钮处理器
        /// </summary>
        public void OnButtonPressed(ButtonType type)
        {
            _buttons?.Handle(type);
            StateChanged?.Invoke(State);
        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }

}
