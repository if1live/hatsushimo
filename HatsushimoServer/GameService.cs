using System;
using System.Threading.Tasks;
using HatsushimoShared;
using WebSocketSharp.Server;

namespace HatsushimoServer
{
    class GameService
    {
        public static readonly GameService Instance;
        static GameService()
        {
            Instance = new GameService();
        }

        public WebSocketSessionManager Sessions { get; set; }

        // TODO id generator
        int nextId = 1;

        public GameService()
        {
        }

        void Update()
        {

        }

        public async void StartUpdateLoop()
        {
            while (true)
            {
                Update();

                var delay = TimeSpan.FromMilliseconds(1000 / 60);
                await Task.Delay(delay);
            }
        }

        public void HandlePing(GameSession session, PingPacket p)
        {
            Console.WriteLine($"ping packet received : {p.millis}");
            session.SendPacket(p);
        }

        public void HandleConnect(GameSession session, ConnectPacket p)
        {
            var id = nextId;
            nextId += 1;

            var welcome = new WelcomePacket()
            {
                UserID = id,
                Version = Config.Version,
            };
            Console.WriteLine($"welcome id={id}");
            session.SendPacket(welcome);
        }

        public void HandleDisconnect(GameSession session, DisconnectPacket p)
        {

        }
    }
}
