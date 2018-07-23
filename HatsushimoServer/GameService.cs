using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.NetChan;
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

        readonly Dictionary<int, Player> playerTable = new Dictionary<int, Player>();

        Player GetPlayer(Session session)
        {
            var playerID = session.ID;
            return playerTable[playerID];
        }

        readonly RoomManager rooms = new RoomManager();

        public GameService()
        {
            var _ = rooms.GetRoom(RoomManager.DefaultRoomID);
            StartRecvLoop();
        }

        public void HandlePing(Session session, PingPacket p)
        {
            // Console.WriteLine($"ping packet received : {p.millis}");
            session.Send(p);
        }

        public void HandleConnect(Session session, ConnectPacket p)
        {
            var id = session.ID;
            var player = new Player(id, session);
            playerTable[id] = player;

            var welcome = new WelcomePacket()
            {
                UserID = id,
                Version = Config.Version,
            };
            Console.WriteLine($"connected: welcome id={id}, session={session.ID} transport={session.TransportID}");
            session.Send(welcome);
        }

        public void HandleDisconnect(Session session, DisconnectPacket p)
        {
            // 연결 종료는 소켓이 끊어질떄도 있고
            // 유저가 직접 종료시키는 경우도 있다
            // disconnect를 여러번 호출해도 꺠지지 않도록 하자
            if (!playerTable.ContainsKey(session.ID))
            {
                Console.WriteLine($"session={session.ID} is already disconnected");
                return;
            }

            int playerID = session.ID;
            Player player = null;
            if (playerTable.TryGetValue(playerID, out player))
            {
                if (player.RoomID != null)
                {
                    var room = rooms.GetRoom(player.RoomID);
                    room.LeavePlayer(player);
                }
                playerTable.Remove(playerID);
            }

            Console.WriteLine($"disconnected: id={playerID}, session={session.ID}");
            SessionLayer.Layer.RemoveSession(player.Session);
        }

        public void HandleRoomJoinReq(Session session, RoomJoinRequestPacket p)
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
            player.Session.Send(resp);
        }

        public void HandleRoomLeave(Session session, RoomLeavePacket p)
        {
            var player = GetPlayer(session);
            if (player.RoomID != null)
            {
                SessionLayer.Layer.CloseSession(player.Session);
            }

            var room = rooms.GetRoom(player.RoomID);
            room.LeavePlayer(player);
        }

        public void HandleInputCommand(Session session, InputCommandPacket p)
        {
            // TODO exec action
            Console.WriteLine($"input - command : {p.Mode}");
        }

        public void HandleInputMove(Session session, InputMovePacket p)
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
        public void HandlePlayerReady(Session session, PlayerReadyPacket p)
        {
            var player = GetPlayer(session);
            if (player.RoomID == null)
            {
                SessionLayer.Layer.CloseSession(player.Session);
            }

            var room = rooms.GetRoom(player.RoomID);
            room.SpawnPlayer(player);
        }

        void HandlePacket(Session session, IPacket packet)
        {
            var service = GameService.Instance;
            switch ((PacketType)packet.Type)
            {
                case PacketType.Ping:
                    HandlePing(session, (PingPacket)packet);
                    break;

                case PacketType.Connect:
                    HandleConnect(session, (ConnectPacket)packet);
                    break;

                case PacketType.Disconnect:
                    HandleDisconnect(session, (DisconnectPacket)packet);
                    break;

                case PacketType.RoomJoinReq:
                    HandleRoomJoinReq(session, (RoomJoinRequestPacket)packet);
                    break;

                case PacketType.RoomLeave:
                    HandleRoomLeave(session, (RoomLeavePacket)packet);
                    break;

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
                    Console.WriteLine($"packet handler not exist: {packet.Type}");
                    break;
            }
        }

        async void StartRecvLoop()
        {
            var layer = SessionLayer.Layer;
            var codec = MyPacketCodec.Create();
            while (true)
            {
                var packets = layer.FlushReceivedPackets();
                foreach (var p in packets)
                {
                    HandlePacket(p.Session, p.Packet);
                }

                var interval = TimeSpan.FromMilliseconds(1000 / 100);
                await Task.Delay(interval);
            }
        }
    }
}
