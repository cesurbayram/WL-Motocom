using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchlog_Websocket_NET_CORE_8.Classes.Entityies;

namespace Watchlog_Websocket_NET_CORE_8.Classes
{
    public class AlarmDB
    {
        private List<Alarm_Value> alarmList;

        public AlarmDB()
        {
            alarmList = new List<Alarm_Value>();
        }

        public List<Alarm_Value> GetByIp(string ipAddress)
        {
            return alarmList.Where(a => a.ip_address == ipAddress).ToList();
        }

        public List<Alarm_Value> GetAllAlarms()
        {
            return alarmList;
        }
    }
}
