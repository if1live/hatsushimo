using UniRx;

namespace Assets.Game
{
    class Connection
    {
        public readonly static Connection Instance = new Connection();

        public string PlayerID { get; set; }
        public string RoomID { get; set; }
        public string Nickname { get; set; }

        public ReactiveProperty<bool> IsReady {
            get { return _isReady; }
        }
        ReactiveProperty<bool> _isReady = new ReactiveProperty<bool>(false);

        public Connection()
        {
            Reset();
        }

        public void Reset()
        {
            PlayerID = "";
            RoomID = "";
            Nickname = "";
            IsReady.Value = false;
        }
    }
}
