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

        private bool _connectBusState = false;

        public RunState State => _stateMachine.State;

        public void Initialize()
        {
            logger.Info("系统初始化...");
            //MotionModule.Instance.Init();

            _flow.ResetStep();
            _connectBusState = true;
            _stateMachine.SetState(RunState.Ready);
            logger.Info("初始化完成，进入 Ready 状态");

            StateChanged?.Invoke(RunState.Ready);
            ConnectBusChanged?.Invoke(_connectBusState);
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
