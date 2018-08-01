using System;
using System.IO;
using Hatsushimo.Packets;

namespace Hatsushimo.NetChan
{
    public class PacketReceiveEventArgs<TPacket, TArg> : EventArgs
    where TPacket : IPacket
    {
        public PacketReceiveEventArgs(TPacket packet, TArg arg)
        {
            _packet = packet;
            _arg = arg;
        }

        TPacket _packet;
        public TPacket Packet { get { return _packet; } }

        TArg _arg;
        public TArg Arg { get { return _arg; } }
    }

    public delegate void PacketReceiveEventHandler<TPacket, TArg>(
        PacketReceiveEventArgs<TPacket, TArg> a
    ) where TPacket : IPacket;

    // rx를 쓰고싶지만 rx를 위해
    // 유니티에서는 unirx, dotnet에서는 reactivex를 써야한다.
    // 복잡해지는걸 피하려고 레거시 이벤트를 이용해서 구현
    public class PacketDispatcher<TArg>
    {
        readonly PacketCodec codec = new PacketCodec();

        public event PacketReceiveEventHandler<ConnectPacket, TArg> Connect;
        public event PacketReceiveEventHandler<DisconnectPacket, TArg> Disconnect;

        public event PacketReceiveEventHandler<SignUpPacket, TArg> SignUp;
        public event PacketReceiveEventHandler<SignUpResultPacket, TArg> SignUpResult;

        public event PacketReceiveEventHandler<AuthenticationPacket, TArg> Authentication;
        public event PacketReceiveEventHandler<AuthenticationResultPacket, TArg> AuthenticationResult;

        public event PacketReceiveEventHandler<WelcomePacket, TArg> Welcome;

        public event PacketReceiveEventHandler<PingPacket, TArg> Ping;
        public event PacketReceiveEventHandler<HeartbeatPacket, TArg> Heartbeat;

        public event PacketReceiveEventHandler<WorldJoinPacket, TArg> WorldJoin;
        public event PacketReceiveEventHandler<WorldJoinResultPacket, TArg> WorldJoinResult;
        public event PacketReceiveEventHandler<WorldLeavePacket, TArg> WorldLeave;
        public event PacketReceiveEventHandler<WorldLeaveResultPacket, TArg> WorldLeaveResult;

        public event PacketReceiveEventHandler<PlayerReadyPacket, TArg> PlayerReady;

        public event PacketReceiveEventHandler<LeaderboardPacket, TArg> Leaderboard;

        public event PacketReceiveEventHandler<ReplicationAllPacket, TArg> ReplicationAll;

        public event PacketReceiveEventHandler<ReplicationRemovePacket, TArg> ReplicationRemove;
        public event PacketReceiveEventHandler<ReplicationBulkRemovePacket, TArg> ReplicationBulkRemove;

        public event PacketReceiveEventHandler<ReplicationCreateFoodPacket, TArg> CreateFood;
        public event PacketReceiveEventHandler<ReplicationCreatePlayerPacket, TArg> CreatePlayer;
        public event PacketReceiveEventHandler<ReplicationCreateProjectilePacket, TArg> CreateProjectile;

        public event PacketReceiveEventHandler<MovePacket, TArg> Move;
        public event PacketReceiveEventHandler<MoveNotifyPacket, TArg> MoveNotify;

        public event PacketReceiveEventHandler<AttackPacket, TArg> Attack;
        public event PacketReceiveEventHandler<AttackNotifyPacket, TArg> AttackNotify;

        public void Dispatch(byte[] data, TArg arg)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            var iter = new PacketEnumerator(reader);
            while (iter.MoveNext())
            {
                DispatchPacket(iter.CurrentType, reader, arg);
            }
        }

        bool DispatchPacket(short type, BinaryReader reader, TArg arg)
        {
            if (Handle<ConnectPacket>(type, reader, Connect, arg)) { return true; }
            if (Handle<DisconnectPacket>(type, reader, Disconnect, arg)) { return true; }
            if (Handle<SignUpPacket>(type, reader, SignUp, arg)) { return true; }
            if (Handle<SignUpResultPacket>(type, reader, SignUpResult, arg)) { return true; }
            if (Handle<AuthenticationPacket>(type, reader, Authentication, arg)) { return true; }
            if (Handle<AuthenticationResultPacket>(type, reader, AuthenticationResult, arg)) { return true; }
            if (Handle<WelcomePacket>(type, reader, Welcome, arg)) { return true; }
            if (Handle<PingPacket>(type, reader, Ping, arg)) { return true; }
            if (Handle<HeartbeatPacket>(type, reader, Heartbeat, arg)) { return true; }
            if (Handle<WorldJoinPacket>(type, reader, WorldJoin, arg)) { return true; }
            if (Handle<WorldJoinResultPacket>(type, reader, WorldJoinResult, arg)) { return true; }
            if (Handle<WorldLeavePacket>(type, reader, WorldLeave, arg)) { return true; }
            if (Handle<WorldLeaveResultPacket>(type, reader, WorldLeaveResult, arg)) { return true; }
            if (Handle<PlayerReadyPacket>(type, reader, PlayerReady, arg)) { return true; }
            if (Handle<LeaderboardPacket>(type, reader, Leaderboard, arg)) { return true; }
            if (Handle<ReplicationAllPacket>(type, reader, ReplicationAll, arg)) { return true; }
            if (Handle<ReplicationRemovePacket>(type, reader, ReplicationRemove, arg)) { return true; }
            if (Handle<ReplicationBulkRemovePacket>(type, reader, ReplicationBulkRemove, arg)) { return true; }
            if (Handle<ReplicationCreateFoodPacket>(type, reader, CreateFood, arg)) { return true; }
            if (Handle<ReplicationCreatePlayerPacket>(type, reader, CreatePlayer, arg)) { return true; }
            if (Handle<ReplicationCreateProjectilePacket>(type, reader, CreateProjectile, arg)) { return true; }
            if (Handle<MovePacket>(type, reader, Move, arg)) { return true; }
            if (Handle<MoveNotifyPacket>(type, reader, MoveNotify, arg)) { return true; }
            if (Handle<AttackPacket>(type, reader, Attack, arg)) { return true; }
            if (Handle<AttackNotifyPacket>(type, reader, AttackNotify, arg)) { return true; }
            return false;
        }

        bool Handle<T>(short type, BinaryReader reader, PacketReceiveEventHandler<T, TArg> handler, TArg arg) where T : IPacket, new()
        {
            var expected = (new T()).Type;
            if (type == expected)
            {
                var packet = default(T);
                codec.TryDecode(type, reader, out packet);
                handler?.Invoke(new PacketReceiveEventArgs<T, TArg>(packet, arg));
                return true;
            }
            return false;
        }
    }
}
