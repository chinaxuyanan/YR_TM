using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YR_Framework.Core;
using YR_Framework.Events;
using YR_Framework.Models;
using YR_TM.Modules;

namespace YR_TM.Manager
{
    public class ButtonMonitorManager
    {
        private static readonly Lazy<ButtonMonitorManager> _inst = new Lazy<ButtonMonitorManager>(() => new ButtonMonitorManager());
        public static ButtonMonitorManager Instance => _inst.Value;

        private CancellationTokenSource _cts;
        private Task _task;

        //上次状态，防止重复触发
        private bool lastStart, lastPause, lastReset, lastEstop, lastMaintenance;

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Run(() => Loop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private void Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool start = true; /*MotionModule.Instance.ReadIoInBit(1);*/
                bool pause = false; /*MotionModule.Instance.ReadIoInBit(2);*/  //停止就对应暂停按钮
                bool reset = false; /*MotionModule.Instance.ReadIoInBit(3);*/
                bool estop = false; /*MotionModule.Instance.ReadIoInBit(4);*/
                bool maint = false; /*MotionModule.Instance.ReadIoInBit(5);*/

                if (estop && !lastEstop)
                    EventCenter.Publish(new ButtonPressedEvent(ButtonType.EmergencyStop));

                if(start && !lastStart)
                    EventCenter.Publish(new ButtonPressedEvent(ButtonType.Start));

                if(pause && !lastPause)
                    EventCenter.Publish(new ButtonPressedEvent(ButtonType.Pause));

                if(reset && !lastReset)
                    EventCenter.Publish(new ButtonPressedEvent(ButtonType.Reset));

                if(maint && !lastMaintenance)
                    EventCenter.Publish(new ButtonPressedEvent(ButtonType.Maintenance));

                lastStart = start;
                lastPause = pause;
                lastReset = reset;
                lastEstop = estop;
                lastMaintenance = maint;

                Thread.Sleep(20);
            }
        }
    }
}
