using System;
using System.IO;
using System.Reactive.Subjects;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using HatsushimoServer.NetChan;
using NLog;

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
        static readonly Logger log = LogManager.GetLogger("ServerPacketReceiver");
        // 패킷별로 핸들러 따로 만들기
        // 패킷은 struct 위주로 만들었고 GC가 발생하지 않게 하고싶다
        // 서버가 받을 패킷만 명시하기
        // 코딩이 귀찮아서
        // TODO 어떻게하면 타자수를 줄일수 있을까
        public readonly PacketObservable<ConnectPacket> Connect = new PacketObservable<ConnectPacket>();
        public readonly PacketObservable<DisconnectPacket> Disconnect = new PacketObservable<DisconnectPacket>();
        public readonly PacketObservable<PingPacket> Ping = new PacketObservable<PingPacket>();
        public readonly PacketObservable<HeartbeatPacket> Heartbeat = new PacketObservable<HeartbeatPacket>();
        public readonly PacketObservable<SignUpPacket> SignUp = new PacketObservable<SignUpPacket>();
        public readonly PacketObservable<AuthenticationPacket> Authentication = new PacketObservable<AuthenticationPacket>();
        public readonly PacketObservable<AttackPacket> Attack = new PacketObservable<AttackPacket>();
        public readonly PacketObservable<MovePacket> Move = new PacketObservable<MovePacket>();
        public readonly PacketObservable<WorldJoinPacket> WorldJoin = new PacketObservable<WorldJoinPacket>();
        public readonly PacketObservable<WorldLeavePacket> WorldLeave = new PacketObservable<WorldLeavePacket>();
        public readonly PacketObservable<PlayerReadyPacket> PlayerReady = new PacketObservable<PlayerReadyPacket>();

        readonly PacketCodec codec = new PacketCodec();

        public void OnNext(Session session, byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            var slicer = new PacketSlicer(reader);
            while(slicer.MoveNext())
            {
                HandlePacket((PacketType)slicer.CurrentType, reader, session);
            }
        }

        bool HandlePacket(PacketType type, BinaryReader reader, Session session)
        {
            // 패킷 추가되면 핸들러 추가
            if (HandlePacket<ConnectPacket>(type, reader, Connect, session)) { return true; }
            if (HandlePacket<DisconnectPacket>(type, reader, Disconnect, session)) { return true; }
            if (HandlePacket<PingPacket>(type, reader, Ping, session)) { return true; }
            if (HandlePacket<HeartbeatPacket>(type, reader, Heartbeat, session)) { return true; }
            if (HandlePacket<SignUpPacket>(type, reader, SignUp, session)) { return true; }
            if (HandlePacket<AuthenticationPacket>(type, reader, Authentication, session)) { return true; }
            if (HandlePacket<AttackPacket>(type, reader, Attack, session)) { return true; }
            if (HandlePacket<MovePacket>(type, reader, Move, session)) { return true; }
            if (HandlePacket<WorldJoinPacket>(type, reader, WorldJoin, session)) { return true; }
            if (HandlePacket<WorldLeavePacket>(type, reader, WorldLeave, session)) { return true; }
            if (HandlePacket<PlayerReadyPacket>(type, reader, PlayerReady, session)) { return true; }

            log.Error($"handle not exist: packet={type}");
            return false;
        }

        bool HandlePacket<TPacket>(PacketType type, BinaryReader reader, PacketObservable<TPacket> subject, Session session)
        where TPacket : IPacket, new()
        {
            TPacket p = new TPacket();
            if (type == (PacketType)p.Type)
            {
                // binary를 필요한 범위로 자르는 간단한 방법은 직렬화 쓰는거
                // 패킷 내용에 따라서 패킷의 길이가 바뀔수 있기때문에
                // decode-encode하는게 제일 무난하다
                codec.TryDecode((short)type, reader, out p);
                var data = codec.Encode(p);
                subject.OnNext(new ReceivedPacket<TPacket>(session, data));
                return true;
            }
            return false;
        }
    }
}
