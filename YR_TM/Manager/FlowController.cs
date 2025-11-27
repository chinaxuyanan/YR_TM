using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YR_Framework.Models;

namespace YR_TM.Manager
{
    public class FlowController
    {
        private TestStep _step = TestStep.None;
        private ILogger logger = LogManager.GetLogger("FlowController");

        public void ResetStep() => _step = TestStep.None;

        public async Task<bool> StartTestFlowAsync(CancellationToken token, StateMachine state)
        {
            try
            {
                while (true)
                {
                    if(token.IsCancellationRequested)
                        return false;

                    while (state.IsPaused)
                        await Task.Delay(50, token);

                    switch (_step)
                    {
                        case TestStep.None:
                            MAGTestAction.MoveUpCamera();
                            await DelayMove(2000, token, state);
                            _step = TestStep.Step_0001;
                            break;
                        case TestStep.Step_0001:
                            logger.Info("Step2: 点胶 + 取排线");
                            await DelayMove(2000, token, state);
                            _step = TestStep.Step_0002;
                            break;
                        case TestStep.Step_0002:
                            logger.Info("Step3: 组装");
                            await DelayMove(3000, token, state);
                            _step = TestStep.Step_0003;
                            break;
                        case TestStep.Step_0003:
                            logger.Info("Step4: 回原点");
                            await DelayMove(2000, token, state);
                            _step = TestStep.None;
                            return true;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.Warn("流程被取消");
                return false;
            }
            catch(Exception ex)
            {
                logger.Error($"流程出现异常：{ex.Message}");
                return false;
            }
        }

        private async Task DelayMove(int ms, CancellationToken token, StateMachine state)
        {
            int t = 0;
            while (t < ms)
            {
                if (token.IsCancellationRequested)
                    return;

                while(state.IsPaused)
                    await Task.Delay(50, token);

                await Task.Delay(20, token);
                t += 20;
            }
        }
    }
}
