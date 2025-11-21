using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YR_Framework.Models;

namespace YR_TM.Manager
{
    public class StateMachine
    {
        private readonly object _lock = new object();

        public RunState State { get; private set; } = RunState.Idle;

        public bool IsRunning => State == RunState.Running;
        public bool IsPaused => State == RunState.Paused;
        public bool IsReady => State == RunState.Ready;

        public void SetState(RunState state)
        {
            lock(_lock)
            {
                if(State != state)
                    State = state;
            }
        }
    }
}
