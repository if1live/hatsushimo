using System;
using System.IO;
using System.Reactive.Subjects;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using HatsushimoServer.NetChan;

namespace HatsushimoServer.NetChan
{
    // 패킷을 해석해서 들고있으면 레이어를 건너다닐떄 IPacket가 박싱될수있다
    // 패밋을 해석하는데 필요한 데이터를 갖고 다니자
    public struct ReceivedPacket<TPacket>
        where TPacket : IPacket, new()
    {
        public readonly Session Session;
        public readonly byte[] Data;

        public PacketType Type
        {
            get
            {
                var stream = new MemoryStream(Data);
                var reader = new BinaryReader(stream);
                var codec = new PacketCodec();
                return (PacketType)codec.ReadPacketType(reader);
            }
        }
        public TPacket Packet
        {
            get
            {
                var stream = new MemoryStream(Data);
                var reader = new BinaryReader(stream);
                var codec = new PacketCodec();
                var type = codec.ReadPacketType(reader);
                TPacket packet;
                codec.TryDecode<TPacket>(type, reader, out packet);
                return packet;
            }
        }

        public ReceivedPacket(Session s, byte[] data)
        {
            this.Session = s;
            this.Data = data;
        }
    }

    public class PacketObservable<TPacket> where TPacket : IPacket, new()
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
        public readonly PacketObservable<ConnectPacket> Connect = new PacketObservable<ConnectPacket>();
        public readonly PacketObservable<DisconnectPacket> Disconnect = new PacketObservable<DisconnectPacket>();
        public readonly PacketObservable<PingPacket> Ping = new PacketObservable<PingPacket>();
        public readonly PacketObservable<HeartbeatPacket> Heartbeat = new PacketObservable<HeartbeatPacket>();
        public readonly PacketObservable<InputCommandPacket> InputCommand = new PacketObservable<InputCommandPacket>();
        public readonly PacketObservable<InputMovePacket> InputMove = new PacketObservable<InputMovePacket>();
        public readonly PacketObservable<WorldJoinRequestPacket> WorldJoin = new PacketObservable<WorldJoinRequestPacket>();
        public readonly PacketObservable<WorldLeaveRequestPacket> WorldLeave = new PacketObservable<WorldLeaveRequestPacket>();
        public readonly PacketObservable<PlayerReadyPacket> PlayerReady = new PacketObservable<PlayerReadyPacket>();

        readonly PacketCodec codec = new PacketCodec();

        public void OnNext(Session session, byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            var type = (PacketType)codec.ReadPacketType(reader);

            // 패킷 추가되면 핸들러 추가
            if (HandlePacket<ConnectPacket>(type, data, Connect, session)) { return; }
            if (HandlePacket<DisconnectPacket>(type, data, Disconnect, session)) { return; }
            if (HandlePacket<PingPacket>(type, data, Ping, session)) { return; }
            if (HandlePacket<HeartbeatPacket>(type, data, Heartbeat, session)) { return; }
            if (HandlePacket<InputCommandPacket>(type, data, InputCommand, session)) { return; }
            if (HandlePacket<InputMovePacket>(type, data, InputMove, session)) { return; }
            if (HandlePacket<WorldJoinRequestPacket>(type, data, WorldJoin, session)) { return; }
            if (HandlePacket<WorldLeaveRequestPacket>(type, data, WorldLeave, session)) { return; }
            if (HandlePacket<PlayerReadyPacket>(type, data, PlayerReady, session)) { return; }

            Console.Write($"handle not exist: packet={type}");
        }

        /*
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
        */


        bool HandlePacket<TPacket>(PacketType type, byte[] data, PacketObservable<TPacket> subject, Session session)
        where TPacket : IPacket, new()
        {
            TPacket p = new TPacket();
            if(type == (PacketType)p.Type)
            {
                subject.OnNext(new ReceivedPacket<TPacket>(session, data));
                return true;
            }
            return false;
        }

        /*
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
        */
    }
}
