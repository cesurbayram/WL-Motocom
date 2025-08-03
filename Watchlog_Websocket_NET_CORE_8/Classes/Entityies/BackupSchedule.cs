namespace Watchlog_Websocket_NET_CORE_8.Classes.Entityies
{
    public class BackupSchedules
    {
        public string id { get; set; }
        public string controller_id { get; set; }
        public int[] days { get; set; }
        public TimeSpan time { get; set; }
        public string[] file_types { get; set; }
        public bool is_active { get; set; }
    }
}