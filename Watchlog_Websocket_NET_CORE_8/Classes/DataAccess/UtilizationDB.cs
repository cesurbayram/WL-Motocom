using System;
using System.Collections.Generic;
using System.Linq;
using YMConnect.Implementation;
using Watchlog_Websocket_NET_CORE_8.Classes.Entityies;

namespace Watchlog_Websocket_NET_CORE_8.Classes
{
    public class UtilizationDB
    {
        public Utilization_Value UtilizationList { get; private set; }

        public UtilizationDB(string ipAddress)
        {
            UtilizationList = GetDefaultUtilization();
        }

        private Utilization_Value GetDefaultUtilization()
        {
            return new Utilization_Value
            {
                control_power_time = 0,
                servo_power_time = 0,
                playback_time = 0,
                moving_time = 0,
            };
        }
    }
}
