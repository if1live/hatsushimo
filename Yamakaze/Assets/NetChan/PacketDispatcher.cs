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
        public readonly PacketObservable<DisconnectPacket> Disconnect = new PacketObservable<DisconnectPacket>();

        public readonly PacketObservable<SignUpResultPacket> SignUp = new PacketObservable<SignUpResultPacket>();
        public readonly PacketObservable<AuthenticationResultPacket> Authentication = new PacketObservable<AuthenticationResultPacket>();

        public readonly PacketObservable<ReplicationAllPacket> ReplicationAll = new PacketObservable<ReplicationAllPacket>();

        public readonly PacketObservable<ReplicationCreatePlayerPacket> CreatePlayer = new PacketObservable<ReplicationCreatePlayerPacket>();
        public readonly PacketObservable<ReplicationCreateFoodPacket> CreateFood = new PacketObservable<ReplicationCreateFoodPacket>();
        public readonly PacketObservable<ReplicationCreateProjectilePacket> CreateProjectile = new PacketObservable<ReplicationCreateProjectilePacket>();

        public readonly PacketObservable<ReplicationRemovePacket> ReplicationRemove = new PacketObservable<ReplicationRemovePacket>();
        public readonly PacketObservable<ReplicationBulkRemovePacket> ReplicationBulkRemove = new PacketObservable<ReplicationBulkRemovePacket>();

        public readonly PacketObservable<WorldJoinResultPacket> WorldJoin = new PacketObservable<WorldJoinResultPacket>();
        public readonly PacketObservable<WorldLeaveResultPacket> WorldLeave = new PacketObservable<WorldLeaveResultPacket>();

        public readonly PacketObservable<PlayerReadyPacket> PlayerReady = new PacketObservable<PlayerReadyPacket>();
        public readonly PacketObservable<LeaderboardPacket> Leaderboard = new PacketObservable<LeaderboardPacket>();

        public readonly PacketObservable<MoveNotifyPacket> MoveNotify = new PacketObservable<MoveNotifyPacket>();
        public readonly PacketObservable<AttackNotifyPacket> AttackNotify = new PacketObservable<AttackNotifyPacket>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;
        }
    }
}
