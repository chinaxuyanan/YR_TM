using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_Framework.Models;
using YR_TM.Modules;

namespace YR_TM.Manager
{
    public class ButtonHandler
    {
        private readonly StateMachine _state;
        private readonly FlowController _flow;
        private readonly ILogger _logger;

        public ButtonHandler(StateMachine state, FlowController flow, ILogger logger)
        {
            _state = state;
            _flow = flow;
            _logger = logger;
        }

        public void Handle(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Start:
                    Start(); break;
                case ButtonType.Pause:
                    Pause(); break;
                case ButtonType.Reset:
                    Reset(); break;
                case ButtonType.EmergencyStop:
                    EmergencyStop(); break;
                case ButtonType.Maintenance:
                    ToggleMaintence(); break;
            }
        }

        private void Start()
        {
            if(_state.State == RunState.Paused)
            {
                _logger.Info("从暂停继续");
                _state.SetState(RunState.Running);
                return;
            }

            if(_state.State == RunState.Ready || _state.State == RunState.PASS || _state.State == RunState.FAIL)
            {
                _flow.ResetStep();
                _logger.Info("开始测试流程");
                _state.SetState(RunState.Running);
            }
        }

        private void Pause()
        {
            if(_state.State == RunState.Running)
            {
                _logger.Warn("测试暂停");
                _state.SetState(RunState.Paused);
            }
        }

        private void Reset()
        {
            if (_state.IsPaused)
            {
                _logger.Info("复位 --》 继续");
                _state.SetState(RunState.Running);
            }
        }

        private void EmergencyStop()
        {
            _logger.Error("！！！急停，所有动作停止");
            //MotionModule.Instance.StopAxis(5);
            _state.SetState(RunState.EmerStop);
        }

        private void ToggleMaintence()
        {
            if(_state.State != RunState.Maintenance)
            {
                _logger.Warn("进入检修模式");
                _state.SetState(RunState.Maintenance);
            }
            else
            {
                _logger.Warn("退出检修模式");
                _state.SetState(RunState.Ready);
            }
        }
    }
}
