using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.NetChan
{
    public class PacketObservable<TPacket>
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

        public readonly PacketObservable<PingPacket> Ping = new PacketObservable<PingPacket>();
        readonly public PacketObservable<WelcomePacket> Welcome = new PacketObservable<WelcomePacket>();
        readonly public PacketObservable<ReplicationAllPacket> ReplicationAll = new PacketObservable<ReplicationAllPacket>();
        readonly public PacketObservable<ReplicationActionPacket> Replication = new PacketObservable<ReplicationActionPacket>();
        readonly public PacketObservable<ReplicationBulkActionPacket> ReplicationBulk = new PacketObservable<ReplicationBulkActionPacket>();
        readonly public PacketObservable<WorldJoinResponsePacket> WorldJoin = new PacketObservable<WorldJoinResponsePacket>();
        readonly public PacketObservable<WorldLeaveResponsePacket> WorldLeave = new PacketObservable<WorldLeaveResponsePacket>();
        readonly public PacketObservable<PlayerReadyPacket> PlayerReady = new PacketObservable<PlayerReadyPacket>();
        readonly public PacketObservable<LeaderboardPacket> Leaderboard = new PacketObservable<LeaderboardPacket>();

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
