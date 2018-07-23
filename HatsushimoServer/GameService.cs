using System;
using System.Collections;
using System.Collections.Concurrent;
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

        public GameService()
        {
            var worlds = InstanceWorldManager.Instance;
            var defaultWorld = worlds.Get(InstanceWorldManager.DefaultID);

            var sessionLayer = SessionLayer.Layer;
            sessionLayer.Received.Subscribe(received => {
                EnqueueRecv(received.Session, received.Packet);
            });

            StartGameLoop();
        }

        readonly PacketQueue recvQueue = new PacketQueue();

        void HandlePing(Session session, PingPacket p)
        {
            // Console.WriteLine($"ping packet received : {p.millis}");
            session.Send(p);
        }

        void HandleConnect(Session session, ConnectPacket p)
        {
            var welcome = new WelcomePacket()
            {
                UserID = session.ID,
                Version = Config.Version,
            };
            Console.WriteLine($"connected: welcome id={session.ID}");
            session.Send(welcome);
        }

        void HandleDisconnect(Session session, DisconnectPacket p)
        {
            // 연결 종료는 소켓이 끊어질떄도 있고
            // 유저가 직접 종료시키는 경우도 있다
            // disconnect를 여러번 호출해도 꺠지지 않도록 하자
            // 연결 끊은것을 연결 종료 패킷처럼 다루면
            // 상위 레이어에서의 처리가 간단해진다
            if (session.WorldID != null)
            {
                var leave = new WorldLeaveRequestPacket();
                HandleWorldLeave(session, leave);
            }

            Console.WriteLine($"disconnected: id={session.ID}");
            SessionLayer.Layer.RemoveSession(session);
        }

        void HandleWorldJoinReq(Session session, WorldJoinRequestPacket p)
        {
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(p.WorldID);
            var ok = world.Join(session, p.Nickname);
            Console.WriteLine($"world join: id={session.ID} world={world.ID} ok={ok}");
            world.HandleJoinReq(session);

            var resp = new WorldJoinResponsePacket()
            {
                PlayerID = session.ID,
                WorldID = world.ID,
                Nickname = session.Nickname,
            };
            session.Send(resp);
        }

        void HandleWorldLeave(Session session, WorldLeaveRequestPacket p)
        {
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(session.WorldID);
            world.HandleLeaveReq(session);
            var ok = world.Leave(session);
            Console.WriteLine($"world leave: id={session.ID} world={world.ID} ok={ok}");

            var resp = new WorldLeaveResponsePacket()
            {
                PlayerID = session.ID,
            };
            // TODO world에 있는 사람들만 나갔다는걸 알면 된다
            // TOOD replication remove에서 처리하니까 필요 없을지도
        }

        readonly HashSet<PacketType> allowedPackets = new HashSet<PacketType>()
        {
            PacketType.Ping,
            PacketType.Connect,
            PacketType.Disconnect,
            PacketType.WorldJoinReq,
            PacketType.WorldLeaveReq,
        };

        void EnqueueRecv(Session session, IPacket packet)
        {
            if (allowedPackets.Contains((PacketType)packet.Type))
            {
                recvQueue.Enqueue(session, packet);
            }
            else
            {
                var worlds = InstanceWorldManager.Instance;
                var world = worlds.Get(session.WorldID);
                world.EnqueueRecv(session, packet);
            }
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

                case PacketType.WorldJoinReq:
                    HandleWorldJoinReq(session, (WorldJoinRequestPacket)packet);
                    break;

                case PacketType.WorldLeaveReq:
                    HandleWorldLeave(session, (WorldLeaveRequestPacket)packet);
                    break;

                default:
                    break;
            }
        }

        async void StartGameLoop()
        {
            while (true)
            {
                Session session = null;
                IPacket packet = null;
                while (recvQueue.TryDequeue(out session, out packet))
                {
                    HandlePacket(session, packet);
                }

                var interval = TimeSpan.FromMilliseconds(1000 / 120);
                await Task.Delay(interval);
            }
        }
    }
}
