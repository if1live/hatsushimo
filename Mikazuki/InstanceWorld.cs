using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using Mikazuki.NetChan;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

using NLog;

namespace Mikazuki
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


        struct PlayerHolder
        {
            public Player Player { get; private set; }
            public Observer Observer { get; private set; }
            public PlayerMode Mode { get; private set; }

            public static PlayerHolder FromPlayer(Player p)
            {
                return new PlayerHolder() { Player = p, Mode = PlayerMode.Player };
            }

            public static PlayerHolder FromObserver(Observer o)
            {
                return new PlayerHolder() { Observer = o, Mode = PlayerMode.Observer };
            }
        }

        readonly Dictionary<int, PlayerHolder> playerTable = new Dictionary<int, PlayerHolder>();

        readonly RxPacketDispatcher dispatcher = new RxPacketDispatcher();

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
            var r = dispatcher;
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
                dispatcher.OnNext(session, data);
            }
            room.GameLoop();
        }

        Leaderboard<Player> leaderboard = new Leaderboard<Player>(new Player[] { }, Config.LeaderboardSize);
        void LeaderboardUpdate()
        {
            // 리더보드 변경 사항이 있는 경우에만 전송
            // 밑바닥 사람들의 점수는 몇점이든 별로 중요하지 않다
            // 상위 랭킹이 바뀐것만 리더보드로 취급하자
            var players = new List<Player>();
            room.GetPlayers(ref players);

            var observers = new List<Observer>();
            room.GetObservers(ref observers);

            var newLeaderboard = new Leaderboard<Player>(players, Config.LeaderboardSize);
            if (!leaderboard.IsLeaderboardEqual(newLeaderboard))
            {
                leaderboard = newLeaderboard;
                var packet = newLeaderboard.GenerateLeaderboardPacket();
                // TODO broadcaster 만들어둔거 쓰면 좋을거같은데
                foreach (var player in players)
                {
                    player.Session.SendLazy(packet);
                }
                foreach (var o in observers)
                {
                    o.Session.SendLazy(packet);
                }
            }
        }

        void NetworkUpdate()
        {
            var players = new List<Player>();
            room.GetPlayers(ref players);

            var observers = new List<Observer>();
            room.GetObservers(ref observers);

            var moves = players.Select(p => new MoveNotify()
            {
                ID = p.ID,
                TargetPos = p.TargetPosition,
            });
            var packet = new MoveNotifyPacket(moves.ToArray());

            // 위치 정보 갱신
            // TOOD broadcast?
            foreach (var player in players)
            {
                player.Session.SendImmediate(packet);
            }
            foreach (var o in observers)
            {
                o.Session.SendImmediate(packet);
            }
        }

        PlayerHolder GetPlayerHolder(Session session)
        {
            var playerID = session.ID;
            return playerTable[playerID];
        }

        public bool Join(Session session, string nickname, PlayerMode mode)
        {
            if (session.WorldID != null) { return false; }
            session.WorldID = ID;
            session.Nickname = nickname;
            sessions.Add(session);

            var sessionID = session.ID;
            if (mode == PlayerMode.Player)
            {
                var p = new Player(sessionID, session);
                playerTable[sessionID] = PlayerHolder.FromPlayer(p);
                return true;
            }
            else if (mode == PlayerMode.Observer)
            {
                var o = new Observer(sessionID, session);
                playerTable[sessionID] = PlayerHolder.FromObserver(o);
                return true;
            }
            // else...
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
            var ok = Join(session, p.Nickname, p.Mode);
            log.Info($"world join: id={session.ID} world={ID} ok={ok} size={sessions.Count}");

            var holder = GetPlayerHolder(session);
            if (holder.Mode == PlayerMode.Player)
            {
                room.Join(holder.Player);
            }
            else if (holder.Mode == PlayerMode.Observer)
            {
                room.Join(holder.Observer);
            }
            else { return; }

            var resp = new WorldJoinResultPacket(0, session.ID);
            session.SendLazy(resp);
        }

        void HandleLeaveReq(Session session, WorldLeavePacket p)
        {
            var holder = GetPlayerHolder(session);
            if (holder.Mode == PlayerMode.Player)
            {
                room.Leave(holder.Player);
            }
            else if (holder.Mode == PlayerMode.Observer)
            {
                room.Leave(holder.Observer);
            }
            else { return; }

            var ok = Leave(session);
            log.Info($"world leave: id={session.ID} world={ID} ok={ok} size={sessions.Count}");

            var resp = new WorldLeaveResultPacket(session.ID);
            session.SendLazy(resp);
        }

        void HandleAttack(Session session, AttackPacket p)
        {
            var holder = GetPlayerHolder(session);
            if (holder.Mode == PlayerMode.Player)
            {
                var player = holder.Player;
                player.UseSkill(p.Mode);
                room.LaunchProjectile(player);
            }
        }

        void HandleMove(Session session, MovePacket p)
        {
            var holder = GetPlayerHolder(session);
            if (holder.Mode == PlayerMode.Player)
            {
                var player = holder.Player;
                var speed = Config.PlayerSpeed;
                player.TargetPosition = p.TargetPos;
                player.Speed = speed;
            }
        }

        // 방에 접속하면 클라이언트에서 게임씬 로딩을 시작한다
        // 로딩이 끝난 다음부터 객체가 의미있도록 만들고싶다
        // 게임 로딩이 끝나기전에는 무적으로 만드는게 목적
        void HandlePlayerReady(Session session, PlayerReadyPacket p)
        {
            var holder = GetPlayerHolder(session);
            if (holder.Mode == PlayerMode.Player)
            {
                var player = holder.Player;
                room.SpawnPlayer(player);
                player.Session.SendImmediate(leaderboard.GenerateLeaderboardPacket());
            }
        }
    }
}
