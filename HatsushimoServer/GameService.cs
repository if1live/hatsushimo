using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;
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

        readonly Dictionary<string, int> sessionIDPlayerIDTable = new Dictionary<string, int>();
        readonly Dictionary<int, Player> playerTable = new Dictionary<int, Player>();
        Player GetPlayer(GameSession session)
        {
            var playerID = sessionIDPlayerIDTable[session.ID];
            return playerTable[playerID];
        }


        readonly IEnumerator<int> playerIDEnumerator = IDGenerator.MakePlayerID().GetEnumerator();
        readonly RoomManager rooms = new RoomManager();

        public GameService()
        {
            var _ = rooms.GetRoom(RoomManager.DefaultRoomID);
        }

        public void HandlePing(GameSession session, PingPacket p)
        {
            Console.WriteLine($"ping packet received : {p.millis}");
            session.SendPacket(p);
        }

        public void HandleConnect(GameSession session, ConnectPacket p)
        {
            // 플레이어에게는 고유번호 붙인다
            // 어떤 방에 들어가든 id는 유지된다
            playerIDEnumerator.MoveNext();
            var id = playerIDEnumerator.Current;

            sessionIDPlayerIDTable[session.ID] = id;
            var player = new Player(id);
            playerTable[id] = player;

            var welcome = new WelcomePacket()
            {
                UserID = id,
                Version = Config.Version,
            };
            Console.WriteLine($"connected, welcome id={id}");
            session.SendPacket(welcome);
        }

        public void HandleDisconnect(GameSession session, DisconnectPacket p)
        {
            var player = GetPlayer(session);
            if (player.RoomID != null)
            {
                var room = rooms.GetRoom(player.RoomID);
                room.LeavePlayer(player);
            }

            var playerId = sessionIDPlayerIDTable[session.ID];
            sessionIDPlayerIDTable.Remove(session.ID);
            Console.WriteLine($"disconnected, id={playerId}");
            playerTable.Remove(playerId);
        }

        public void HandleRoomJoinReq(GameSession session, RoomJoinRequestPacket p)
        {
            var player = GetPlayer(session);
            player.Reset();
            player.Nickname = p.Nickname;

            var room = rooms.GetRoom(p.RoomID);
            room.JoinPlayer(player);

            var resp = new RoomJoinResponsePacket()
            {
                PlayerID = player.ID,
                RoomID = room.ID,
                Nickname = player.Nickname,
            };
            session.SendPacket(resp);
        }

        public void HandleRoomLeave(GameSession session, RoomLeavePacket p)
        {
            var player = GetPlayer(session);
            if (player.RoomID != null) { session.Disconnect(); }

            var room = rooms.GetRoom(player.RoomID);
            room.LeavePlayer(player);
            session.SendPacket(p);
        }

        public void HandleInputCommand(GameSession session, InputCommandPacket p)
        {
            // TODO exec action
            Console.WriteLine($"input - command : {p.Mode}");
        }

        public void HandleInputMove(GameSession session, InputMovePacket p)
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
        public void HandlePlayerReady(GameSession session, PlayerReadyPacket p)
        {
            var player = GetPlayer(session);
            if (player.RoomID == null) { session.Disconnect(); }

            var room = rooms.GetRoom(player.RoomID);
            room.SpawnPlayer(player);
        }
    }
}
