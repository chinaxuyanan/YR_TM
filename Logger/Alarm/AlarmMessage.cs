using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Alarm
{
    public class AlarmMessage
    {
        public int ID {  get; set; }
        public string AlarmCode {  get; set; }
        public string Message {  get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }
}
