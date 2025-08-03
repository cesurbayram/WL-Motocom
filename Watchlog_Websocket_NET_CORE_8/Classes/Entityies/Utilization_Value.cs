namespace Watchlog_Websocket_NET_CORE_8.Classes.Entityies
{
    public class Utilization_Value
    {
        public string id { get; set; }
        public string controller_id { get; set; }
        public string ip_address { get; set; }
        public int control_power_time { get; set; }
        public int servo_power_time { get; set; }
        public int playback_time { get; set; }
        public int moving_time { get; set; }
        public DateTime timestamp { get; set; }
    }
}