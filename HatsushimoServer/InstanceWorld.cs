using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;

namespace HatsushimoServer
{
    // 각각의 방을 월드로 정의한다
    // 월드안에서는 싱글 쓰레드로 작동해서 자료구조에 동시에 접근하는걸 피한다
    // 월드별로는 의존성을 없애서 독립적으로 작동한다
    internal class InstanceWorld
    {
        public string ID { get; private set; }
        public bool Running { get; set; } = false;

        readonly List<Session> sessions = new List<Session>();
        readonly PacketQueue recvQueue = new PacketQueue();

        readonly Room room;

        readonly Dictionary<int, Player> playerTable = new Dictionary<int, Player>();

        public InstanceWorld(string id)
        {
            this.ID = id;
            this.room = new Room(id);

            StartUpdateLoop();
            StartNetworkLoop();
            StartLeaderboardLoop();
        }

        // 비동기 작업을 별도 쓰레드에서 처리하면
        // player 리스트에 동시에 접근하는 문제가 발생할수 있다
        // 이를 막고자 update 안에 모든 기능을 집어넣음
        // TODO update 안에 전부 집어넣는 좋은 방법 없나
        public bool RequireNetworkLoop { get; set; } = false;
        public bool RequireLeaderboardLoop { get; set; } = false;

        async void StartUpdateLoop()
        {
            var interval = 1000 / 60;
            while (true)
            {
                var a = TimeUtils.NowMillis;
                Update();
                var b = TimeUtils.NowMillis;

                var diff = b - a;
                var wait = interval - diff;
                if (wait < 0) { wait = 0; }
                var delay = TimeSpan.FromMilliseconds(wait);
                await Task.Delay(delay);
            }
        }

        void Update()
        {
            Session session = null;
            IPacket packet = null;
            while (recvQueue.TryDequeue(out session, out packet))
            {
                HandlePacket(session, packet);
            }

            room.GameLoop();

            if(RequireNetworkLoop) {
                room.NetworkLoop();
                RequireNetworkLoop = false;
            }

            if(RequireLeaderboardLoop) {
                room.LeaderboardLoop();
                RequireLeaderboardLoop = false;
            }
        }

        async void StartNetworkLoop()
        {
            var interval = 1000 / Config.SendRateCoord;
            while (true)
            {
                RequireNetworkLoop = true;
                var delay = TimeSpan.FromMilliseconds(interval);
                await Task.Delay(delay);
            }
        }

        async void StartLeaderboardLoop()
        {
            var interval = 1000 / Config.SendRateLeaderboard;
            while (true)
            {
                RequireLeaderboardLoop = true;
                var delay = TimeSpan.FromMilliseconds(interval);
                await Task.Delay(delay);
            }
        }

        Player GetPlayer(Session session)
        {
            var playerID = session.ID;
            return playerTable[playerID];
        }

        public bool Join(Session session, string nickname)
        {
            if (session.WorldID != null) { return false; }
            session.WorldID = ID;
            session.Nickname = nickname;
            sessions.Add(session);

            var sessionID = session.ID;
            var player = new Player(sessionID, session);
            playerTable[sessionID] = player;

            return true;
        }

        public bool Leave(Session session)
        {
            if (session.WorldID == null) { return false; }
            session.WorldID = null;
            session.Nickname = Session.DefaultNickname;
            sessions.Remove(session);
            playerTable.Remove(session.ID);
            return true;
        }

        public void EnqueueRecv(Session s, IPacket p)
        {
            recvQueue.Enqueue(s, p);
        }

        public void HandleJoinReq(Session session)
        {
            var player = GetPlayer(session);
            room.Join(player);
        }

        public void HandleLeaveReq(Session session)
        {
            var player = GetPlayer(session);
            room.Leave(player);
        }

        void HandleInputCommand(Session session, InputCommandPacket p)
        {
            // TODO exec action
            Console.WriteLine($"input - command : {p.Mode}");
        }

        void HandleInputMove(Session session, InputMovePacket p)
        {
            var player = GetPlayer(session);

            var speed = 10;
            var len = p.Dir.Magnitude;

            if (len == 0)
            {
                player.SetVelocity(Vec2.Zero, speed);
            }
            else
            {
                var dir = p.Dir.Normalize();
                player.SetVelocity(dir, speed);
            }
        }

        // 방에 접속하면 클라이언트에서 게임씬 로딩을 시작한다
        // 로딩이 끝난 다음부터 객체가 의미있도록 만들고싶다
        // 게임 로딩이 끝나기전에는 무적으로 만드는게 목적
        void HandlePlayerReady(Session session, PlayerReadyPacket p)
        {
            var player = GetPlayer(session);
            room.SpawnPlayer(player);
        }

        void HandlePacket(Session session, IPacket packet)
        {
            var service = GameService.Instance;
            switch ((PacketType)packet.Type)
            {
                case PacketType.PlayerReady:
                    HandlePlayerReady(session, (PlayerReadyPacket)packet);
                    break;

                case PacketType.InputCommand:
                    HandleInputCommand(session, (InputCommandPacket)packet);
                    break;

                case PacketType.InputMove:
                    HandleInputMove(session, (InputMovePacket)packet);
                    break;

                default:
                    break;
            }
        }
    }
}
