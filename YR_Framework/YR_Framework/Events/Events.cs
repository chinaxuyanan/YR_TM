using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YR_Framework.Models;

namespace YR_Framework.Events
{
    public class UserChangedEvent
    {
        public UserLevel CurrentUser {  get; set; }
    }

    public class RunModeChangedEvent
    {
        public RunMode CurrentRunMode { get; set; }
    }

    public class RunStateChangedEvent
    {
        public RunState CurrentRunState { get; set; }
    }

    public class LogOutChangeEvent
    {
        public UserLevel User {  get; set; }
    }

    public class TopMenuChangedEvent
    {
        public string CurrentTopMenu {  get; set; }
    }
}
