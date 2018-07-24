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
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

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

            Observable.Interval(TimeSpan.FromMilliseconds(1000 / 60))
                .Subscribe(_ => Update());

            Observable.Interval(TimeSpan.FromMilliseconds(1000 / Config.SendRateCoord))
                .Subscribe(_ => NetworkUpdate());

            Observable.Interval(TimeSpan.FromMilliseconds(1000 / Config.SendRateLeaderboard))
                .Subscribe(_ => LeaderboardUpdate());
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

        Leaderboard leaderboard = new Leaderboard(new Player[] { }, Config.LeaderboardSize);
        void LeaderboardUpdate()
        {
            // 리더보드 변경 사항이 있는 경우에만 전송
            // 밑바닥 사람들의 점수는 몇점이든 별로 중요하지 않다
            // 상위 랭킹이 바뀐것만 리더보드로 취급하자
            var players = new List<Player>();
            room.GetPlayers(ref players);

            var newLeaderboard = new Leaderboard(players, Config.LeaderboardSize);
            if (!leaderboard.IsLeaderboardEqual(newLeaderboard))
            {
                leaderboard = newLeaderboard;
                var packet = newLeaderboard.GenerateLeaderboardPacket();
                players.ForEach(player =>
                {
                    player.Session.Send(packet);
                });
            }
        }

        void NetworkUpdate()
        {
            var players = new List<Player>();
            room.GetPlayers(ref players);

            players.ForEach(player =>
            {
                var actions = players.Select(p => new ReplicationActionPacket()
                {
                    Action = ReplicationAction.Update,
                    ID = p.ID,
                    ActorType = p.Type,
                    Pos = p.Position,
                    TargetPos = p.TargetPosition,
                    Speed = p.Speed,
                    Extra = "",
                });

                var packet = new ReplicationBulkActionPacket()
                {
                    Actions = actions.ToArray(),
                };
                player.Session.Send(packet);
            });
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

        void HandleJoinReq(Session session, WorldJoinRequestPacket p)
        {
            var ok = Join(session, p.Nickname);
            Console.WriteLine($"world join: id={session.ID} world={ID} ok={ok}");

            var player = GetPlayer(session);
            room.Join(player);

            var resp = new WorldJoinResponsePacket()
            {
                PlayerID = session.ID,
                WorldID = ID,
                Nickname = session.Nickname,
            };
            session.Send(resp);
        }

        void HandleLeaveReq(Session session, WorldLeaveRequestPacket p)
        {
            var player = GetPlayer(session);
            room.Leave(player);

            var ok = Leave(session);
            Console.WriteLine($"world leave: id={session.ID} world={ID} ok={ok}");

            var resp = new WorldLeaveResponsePacket()
            {
                PlayerID = session.ID,
            };
            // TODO world에 있는 사람들만 나갔다는걸 알면 된다
            // TOOD replication remove에서 처리하니까 필요 없을지도
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
            player.Session.Send(leaderboard.GenerateLeaderboardPacket());
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

                case PacketType.WorldJoinReq:
                    HandleJoinReq(session, (WorldJoinRequestPacket)packet);
                    break;

                case PacketType.WorldLeaveReq:
                    HandleLeaveReq(session, (WorldLeaveRequestPacket)packet);
                    break;

                default:
                    break;
            }
        }
    }
}
