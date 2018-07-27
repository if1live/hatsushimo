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
        public readonly PacketObservable<WelcomePacket> Welcome = new PacketObservable<WelcomePacket>();

        public readonly PacketObservable<SignUpResultPacket> SignUp = new PacketObservable<SignUpResultPacket>();
        public readonly PacketObservable<AuthenticationResultPacket> Authentication = new PacketObservable<AuthenticationResultPacket>();

        public readonly PacketObservable<ReplicationAllPacket> ReplicationAll = new PacketObservable<ReplicationAllPacket>();
        public readonly PacketObservable<ReplicationActionPacket> Replication = new PacketObservable<ReplicationActionPacket>();
        public readonly PacketObservable<ReplicationBulkActionPacket> ReplicationBulk = new PacketObservable<ReplicationBulkActionPacket>();

        public readonly PacketObservable<WorldJoinResultPacket> WorldJoin = new PacketObservable<WorldJoinResultPacket>();
        public readonly PacketObservable<WorldLeaveResultPacket> WorldLeave = new PacketObservable<WorldLeaveResultPacket>();

        public readonly PacketObservable<PlayerReadyPacket> PlayerReady = new PacketObservable<PlayerReadyPacket>();
        public readonly PacketObservable<LeaderboardPacket> Leaderboard = new PacketObservable<LeaderboardPacket>();

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
