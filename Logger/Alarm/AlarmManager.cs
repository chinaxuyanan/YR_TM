using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Alarm
{
    public class AlarmManager
    {
        private static int _nextID = 0;

        private List<AlarmMessage> _alarmHistory = new List<AlarmMessage>();
        public IReadOnlyList<AlarmMessage> AlarmHistory => _alarmHistory.AsReadOnly();

        public int AddAlarm(ErrorType alarmCode, string messae)
        {
            var id = GenerateAlarmID();

            var alarm = new AlarmMessage
            {
                ID = id,
                AlarmCode = alarmCode,
                Message = messae,
                StartTime = DateTime.Now
            };

            _alarmHistory.Add(alarm);

            LogManager.GetLogger("AlarmManager").Log(LogLevel.Error, $"Alarm ID：{id}, Code: {alarmCode}, Message: {messae}");

            return id;
        }

        public int GenerateAlarmID()
        {
            return Interlocked.Increment(ref _nextID);
        }

        public void ResolveAlarm(int id)
        {
            var alarm = _alarmHistory.FirstOrDefault(a => a.ID == id);
            if (alarm != null)
                alarm.EndTime = DateTime.Now;
        }
    }
}
