using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;
using HatsushimoServer.NetChan;
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

            var sessionLayer = NetworkStack.Session;

            sessionLayer.Connect.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.Disconnect.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.Ping.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.Heartbeat.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.InputCommand.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.InputMove.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.WorldJoin.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.WorldLeave.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));
            sessionLayer.PlayerReady.Received.Subscribe(d => EnqueueRecv(d.Session, d.Packet));

            Observable.Interval(TimeSpan.FromMilliseconds(1000 / 120))
                .Subscribe(_ => Update());
        }

        readonly PacketQueue recvQueue = new PacketQueue();

        void HandlePing(Session session, PingPacket p)
        {
            // Console.WriteLine($"ping packet received : {p.millis}");
            session.Send(p);
        }

        void HandleHeartbeat(Session session, HeartbeatPacket p)
        {
            session.RefreshHeartbeat();
            Console.WriteLine($"heartbeat: id={session.ID}");
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
            NetworkStack.Session.RemoveSessionWithLock(session);
        }

        void HandleWorldJoinReq(Session session, WorldJoinRequestPacket p)
        {
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(p.WorldID);
            world.EnqueueRecv(session, p);
        }

        void HandleWorldLeave(Session session, WorldLeaveRequestPacket p)
        {
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(session.WorldID);
            world.EnqueueRecv(session, p);
        }

        readonly HashSet<PacketType> allowedPackets = new HashSet<PacketType>()
        {
            PacketType.Ping,
            PacketType.Heartbeat,
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

        void EnqueueRecv<TPacket>(Session session, TPacket packet)
        where TPacket : IPacket
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

                case PacketType.Heartbeat:
                    HandleHeartbeat(session, (HeartbeatPacket)packet);
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

        void Update()
        {
            Session session = null;
            IPacket packet = null;
            while (recvQueue.TryDequeue(out session, out packet))
            {
                HandlePacket(session, packet);
            }
        }
    }
}
