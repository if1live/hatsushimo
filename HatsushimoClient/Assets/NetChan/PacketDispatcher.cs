using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.NetChan
{
    public class PacketObserver<TPacket>
    {
        public IObservable<TPacket> Received
        {
            get { return received.Skip(1).AsObservable(); }
        }
        ReactiveProperty<TPacket> received = new ReactiveProperty<TPacket>();
        public void SetValueAndForceNotify(TPacket v)
        {
            received.SetValueAndForceNotify(v);
        }
    }


    public class PacketDispatcher : MonoBehaviour
    {
        public static PacketDispatcher Instance;

        public readonly PacketObserver<PingPacket> Ping = new PacketObserver<PingPacket>();
        readonly public PacketObserver<WelcomePacket> Welcome = new PacketObserver<WelcomePacket>();
        readonly public PacketObserver<ReplicationAllPacket> ReplicationAll = new PacketObserver<ReplicationAllPacket>();
        readonly public PacketObserver<ReplicationActionPacket> Replication = new PacketObserver<ReplicationActionPacket>();
        readonly public PacketObserver<ReplicationBulkActionPacket> ReplicationBulk = new PacketObserver<ReplicationBulkActionPacket>();
        readonly public PacketObserver<WorldJoinResponsePacket> WorldJoin = new PacketObserver<WorldJoinResponsePacket>();
        readonly public PacketObserver<WorldLeaveResponsePacket> WorldLeave = new PacketObserver<WorldLeaveResponsePacket>();
        readonly public PacketObserver<PlayerReadyPacket> PlayerReady = new PacketObserver<PlayerReadyPacket>();
        readonly public PacketObserver<LeaderboardPacket> Leaderboard = new PacketObserver<LeaderboardPacket>();

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;
        }
    }
}
