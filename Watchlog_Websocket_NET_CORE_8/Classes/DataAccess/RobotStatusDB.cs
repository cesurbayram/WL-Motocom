using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchlog_Websocket_NET_CORE_8.Classes.Entityies;

namespace Watchlog_Websocket_NET_CORE_8.Classes.DataAccess
{
    public class RobotStatusDB
    {
        private List<RobotStatusValue> RobotStatusList;

        public RobotStatusDB()
        {
            RobotStatusList = new List<RobotStatusValue>();
        }

        
        public List<RobotStatusValue> GetByIp(string ipAddress)
        {
            return RobotStatusList.Where(a => a.ip_address == ipAddress).ToList();
        }

       
        public List<RobotStatusValue> GetAllStatus()
        {
            return RobotStatusList;
        }
    }
}
