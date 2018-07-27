using System;
using System.IO;
using System.Reactive.Subjects;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using HatsushimoServer.NetChan;

namespace HatsushimoServer.NetChan
{
    public struct ReceivedPacket<TPacket>
        where TPacket : IPacket
    {
        public Session Session;
        public TPacket Packet;

        public static ReceivedPacket<TPacket> Create(Session s, TPacket p)
        {
            return new ReceivedPacket<TPacket>()
            {
                Session = s,
                Packet = p,
            };
        }
    }

    public class PacketObserver<TPacket> where TPacket : IPacket
    {
        Subject<ReceivedPacket<TPacket>> _received = new Subject<ReceivedPacket<TPacket>>();
        public IObservable<ReceivedPacket<TPacket>> Received { get { return _received; } }
        public void OnNext(ReceivedPacket<TPacket> p) { _received.OnNext(p); }
    }

    public class ServerPacketReceiver
    {
        // 패킷별로 핸들러 따로 만들기
        // 패킷은 struct 위주로 만들었고 GC가 발생하지 않게 하고싶다
        // 서버가 받을 패킷만 명시하기
        // 코딩이 귀찮아서
        // TODO 어떻게하면 타자수를 줄일수 있을까
        public readonly PacketObserver<ConnectPacket> Connect = new PacketObserver<ConnectPacket>();
        public readonly PacketObserver<DisconnectPacket> Disconnect = new PacketObserver<DisconnectPacket>();
        public readonly PacketObserver<PingPacket> Ping = new PacketObserver<PingPacket>();
        public readonly PacketObserver<HeartbeatPacket> Heartbeat = new PacketObserver<HeartbeatPacket>();
        public readonly PacketObserver<InputCommandPacket> InputCommand = new PacketObserver<InputCommandPacket>();
        public readonly PacketObserver<InputMovePacket> InputMove = new PacketObserver<InputMovePacket>();
        public readonly PacketObserver<WorldJoinRequestPacket> WorldJoin = new PacketObserver<WorldJoinRequestPacket>();
        public readonly PacketObserver<WorldLeaveRequestPacket> WorldLeave = new PacketObserver<WorldLeaveRequestPacket>();
        public readonly PacketObserver<PlayerReadyPacket> PlayerReady = new PacketObserver<PlayerReadyPacket>();

        readonly PacketCodec codec = new PacketCodec();

        protected void OnNext(Session session, byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            var type = (PacketType)codec.ReadPacketType(reader);

            if (HandlePacket<ConnectPacket>(type, reader, Connect, session)) { return; }
            if (HandlePacket<DisconnectPacket>(type, reader, Disconnect, session)) { return; }
            if (HandlePacket<PingPacket>(type, reader, Ping, session)) { return; }
            if (HandlePacket<HeartbeatPacket>(type, reader, Heartbeat, session)) { return; }
            if (HandlePacket<InputCommandPacket>(type, reader, InputCommand, session)) { return; }
            if (HandlePacket<InputMovePacket>(type, reader, InputMove, session)) { return; }
            if (HandlePacket<WorldJoinRequestPacket>(type, reader, WorldJoin, session)) { return; }
            if (HandlePacket<WorldLeaveRequestPacket>(type, reader, WorldLeave, session)) { return; }
            if (HandlePacket<PlayerReadyPacket>(type, reader, PlayerReady, session)) { return; }

            Console.Write($"handle not exist: packet={type}");
        }

        protected void OnNext(Session session, ConnectPacket packet)
        {
            var subject = Connect;
            subject.OnNext(ReceivedPacket<ConnectPacket>.Create(session, packet));
        }

        protected void OnNext(Session session, DisconnectPacket packet)
        {
            var subject = Disconnect;
            subject.OnNext(ReceivedPacket<DisconnectPacket>.Create(session, packet));
        }


        protected void OnNext(Session session, PingPacket packet)
        {
            var subject = Ping;
            subject.OnNext(ReceivedPacket<PingPacket>.Create(session, packet));
        }


        protected void OnNext(Session session, HeartbeatPacket packet)
        {
            var subject = Heartbeat;
            subject.OnNext(ReceivedPacket<HeartbeatPacket>.Create(session, packet));
        }


        protected void OnNext(Session session, InputCommandPacket packet)
        {
            var subject = InputCommand;
            subject.OnNext(ReceivedPacket<InputCommandPacket>.Create(session, packet));
        }


        protected void OnNext(Session session, InputMovePacket packet)
        {
            var subject = InputMove;
            subject.OnNext(ReceivedPacket<InputMovePacket>.Create(session, packet));
        }


        protected void OnNext(Session session, WorldJoinRequestPacket packet)
        {
            var subject = WorldJoin;
            subject.OnNext(ReceivedPacket<WorldJoinRequestPacket>.Create(session, packet));
        }


        protected void OnNext(Session session, WorldLeaveRequestPacket packet)
        {
            var subject = WorldLeave;
            subject.OnNext(ReceivedPacket<WorldLeaveRequestPacket>.Create(session, packet));
        }


        protected void OnNext(Session session, PlayerReadyPacket packet)
        {
            var subject = PlayerReady;
            subject.OnNext(ReceivedPacket<PlayerReadyPacket>.Create(session, packet));
        }



        bool HandlePacket<TPacket>(PacketType type, BinaryReader reader, PacketObserver<TPacket> subject, Session session)
        where TPacket : IPacket, new()
        {
            TPacket p = new TPacket();
            if (codec.TryDecode((short)type, reader, out p))
            {
                subject.OnNext(ReceivedPacket<TPacket>.Create(session, p));
                return true;
            }
            return false;
        }

        bool HandlePacket<TPacket>(PacketType type, TPacket p, PacketObserver<TPacket> subject, Session session)
        where TPacket : IPacket, new()
        {
            if (type != (PacketType)p.Type)
            {
                return false;
            }
            subject.OnNext(ReceivedPacket<TPacket>.Create(session, p));
            return true;
        }
    }
}
