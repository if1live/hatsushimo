namespace Assets.NetChan
{
    public class ConnectionInfo
    {
        public static readonly ConnectionInfo Info = new ConnectionInfo();

        protected ConnectionInfo() { }

        public int PlayerID { get; set; }
        public string WorldID { get; set; }
        public string Nickname { get; set; }
    }
}
