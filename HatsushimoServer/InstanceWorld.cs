using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;
using HatsushimoServer.NetChan;

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
        }

        async void StartUpdateLoop()
        {
            var updateInterval = 1000 / 60;
            var networkInterval = 1000 / Config.SendRateCoord;
            var leaderboardInterval = 1000 / Config.SendRateLeaderboard;

            // 120fps로 루프를 돌린다
            // time slice가 전부 채워진 경우에 호출하기
            var refreshInterval = 1000 / 120;

            var updateRemain = 0;
            var networkRemain = 0;
            var leaderboardRemain = 0;

            int prevMillis = TimeUtils.NowMillis;

            while (true)
            {
                var startMillis = TimeUtils.NowMillis;
                var elapsedMillis = startMillis - prevMillis;
                updateRemain -= elapsedMillis;
                networkRemain -= elapsedMillis;
                leaderboardRemain -= elapsedMillis;

                if (updateRemain <= 0)
                {
                    Update();
                    updateRemain += updateInterval;
                }
                if (networkRemain <= 0)
                {
                    room.NetworkLoop();
                    networkRemain += networkInterval;
                }
                if (leaderboardRemain <= 0)
                {
                    room.LeaderboardLoop();
                    leaderboardRemain += leaderboardInterval;
                }

                var endMillis = TimeUtils.NowMillis;
                var diff = endMillis - startMillis;
                prevMillis = startMillis;

                var wait = refreshInterval - diff;
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
            var player = GetPlayer(session);
            player.UseSkill(p.Mode);
        }

        void HandleInputMove(Session session, InputMovePacket p)
        {
            var player = GetPlayer(session);
            var speed = 10;
            player.TargetPosition = p.TargetPos;
            player.Speed = speed;
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

        async void SendDelayedPacket(Session session, IPacket packet, TimeSpan dueTime)
        {
            await Task.Delay(dueTime);
            session.Send(packet);
        }
    }
}
