using Hatsushimo;

namespace Assets.NetChan
{
    public class ConnectionInfo
    {
        public static readonly ConnectionInfo Info = new ConnectionInfo();

        protected ConnectionInfo() { }

        public string ServerHost
        {
            get { return _serverHost; }
            set
            {
                _serverHost = value;
                UseDefaultServer = false;
            }
        }
        string _serverHost = "ws://127.0.0.1";


        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                _serverPort = value;
                UseDefaultServer = false;
            }
        }
        int _serverPort = Config.ServerPort;


        public bool UseDefaultServer { get; set; } = true;
        public string ServerURL
        {
            get { return $"{ServerHost}:{ServerPort}/game"; }
        }

        public int PlayerID { get; set; }
        public string WorldID { get; set; }
        public string Nickname { get; set; }
    }
}
