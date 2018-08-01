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

using NLog;

namespace HatsushimoServer
{
    // 각각의 방을 월드로 정의한다
    // 월드안에서는 싱글 쓰레드로 작동해서 자료구조에 동시에 접근하는걸 피한다
    // 월드별로는 의존성을 없애서 독립적으로 작동한다
    internal class InstanceWorld
    {
        static readonly Logger log = LogManager.GetLogger("InstanceWorld");
        public string ID { get; private set; }
        public bool Running { get; set; } = false;

        readonly List<Session> sessions = new List<Session>();
        readonly PacketQueue recvQueue = new PacketQueue();

        readonly Room room;

        readonly Dictionary<int, Player> playerTable = new Dictionary<int, Player>();

        readonly ServerPacketReceiver receiver = new ServerPacketReceiver();

        public InstanceWorld(string id)
        {
            this.ID = id;
            this.room = new Room(id);

            Observable.Interval(TimeSpan.FromMilliseconds(Config.GameLoopIntervalMillis))
                .Subscribe(_ => Update());

            Observable.Interval(TimeSpan.FromMilliseconds(Config.MoveSyncIntervalMillis))
                .Subscribe(_ => NetworkUpdate());

            Observable.Interval(TimeSpan.FromMilliseconds(Config.LeaderboardSyncIntervalMillis))
                .Subscribe(_ => LeaderboardUpdate());

            // handle packet
            var r = receiver;
            r.WorldJoin.Received.Subscribe(pair => HandleJoinReq(pair.Session, pair.Packet));
            r.WorldLeave.Received.Subscribe(pair => HandleLeaveReq(pair.Session, pair.Packet));

            r.PlayerReady.Received.Subscribe(pair => HandlePlayerReady(pair.Session, pair.Packet));
            r.Attack.Received.Subscribe(pair => HandleAttack(pair.Session, pair.Packet));
            r.Move.Received.Subscribe(pair => HandleMove(pair.Session, pair.Packet));
        }

        void Update()
        {
            Session session = null;
            byte[] data = null;
            while (recvQueue.TryDequeue(out session, out data))
            {
                receiver.OnNext(session, data);
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
                    player.Session.SendLazy(packet);
                });
            }
        }

        void NetworkUpdate()
        {
            var players = new List<Player>();
            room.GetPlayers(ref players);

            // 위치 정보 갱신
            players.ForEach(player =>
            {
                var moves = players.Select(p => new MoveNotify()
                {
                    ID = p.ID,
                    TargetPos = p.TargetPosition,
                });
                var packet = new MoveNotifyPacket()
                {
                    list = moves.ToArray(),
                };
                player.Session.SendImmediate(packet);
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

        public void EnqueueRecv(Session s, byte[] d)
        {
            recvQueue.Enqueue(s, d);
        }

        void HandleJoinReq(Session session, WorldJoinPacket p)
        {
            var ok = Join(session, p.Nickname);
            log.Info($"world join: id={session.ID} world={ID} ok={ok} size={sessions.Count}");

            var player = GetPlayer(session);
            room.Join(player);

            var resp = new WorldJoinResultPacket()
            {
                PlayerID = session.ID,
                WorldID = ID,
                Nickname = session.Nickname,
            };
            session.SendLazy(resp);
        }

        void HandleLeaveReq(Session session, WorldLeavePacket p)
        {
            var player = GetPlayer(session);
            room.Leave(player);

            var ok = Leave(session);
            log.Info($"world leave: id={session.ID} world={ID} ok={ok} size={sessions.Count}");

            var resp = new WorldLeaveResultPacket()
            {
                PlayerID = session.ID,
            };
            player.Session.SendLazy(resp);
        }

        void HandleAttack(Session session, AttackPacket p)
        {
            var player = GetPlayer(session);
            player.UseSkill(p.Mode);

            var projectile = room.MakeProjectile(player);
            room.RegisterProjectile(projectile);
            room.SendProjectileCreatePacket(projectile);
        }

        void HandleMove(Session session, MovePacket p)
        {
            var player = GetPlayer(session);
            var speed = Config.PlayerSpeed;
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
            player.Session.SendImmediate(leaderboard.GenerateLeaderboardPacket());
        }
    }
}
