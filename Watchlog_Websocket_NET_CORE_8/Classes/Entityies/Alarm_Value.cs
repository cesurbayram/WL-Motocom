namespace Watchlog_Websocket_NET_CORE_8.Classes.Entityies
{
    public class Alarm_Value
    {
        public string id { get; set; }
        public string ip_address { get; set; }
        public string code { get; set; }
        public string alarm { get; set; }
        public string text { get; set; }
        public string origin_date { get; set; }
        public bool is_active { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Alarm_Value other)
            {
                return this.ip_address == other.ip_address && this.code == other.code && this.text == other.text;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ip_address, code, text);
        }
    }
}
