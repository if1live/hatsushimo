using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.NetChan
{
    public class PacketDispatcher : MonoBehaviour 
    {
        public static PacketDispatcher Instance;

        public IObservable<PingPacket> PingReceived {
            get { return ping.Skip(1).AsObservable(); }
        }
        ReactiveProperty<PingPacket> ping = new ReactiveProperty<PingPacket>();

        public IObservable<WelcomePacket> WelcomeReceived {
            get { return welcome.Skip(1).AsObservable(); }
        }
        ReactiveProperty<WelcomePacket> welcome = new ReactiveProperty<WelcomePacket>();

        public IObservable<ReplicationAllPacket> ReplicationAllReceived {
            get { return replicationAll.Skip(1).AsObservable(); }
        }
        ReactiveProperty<ReplicationAllPacket> replicationAll = new ReactiveProperty<ReplicationAllPacket>();

        public IObservable<ReplicationActionPacket> ReplicationReceived {
            get { return replication.Skip(1).AsObservable(); }
        }
        ReactiveProperty<ReplicationActionPacket> replication = new ReactiveProperty<ReplicationActionPacket>();

        public IObservable<ReplicationBulkActionPacket> ReplicationBulkReceived {
            get { return replicationBulk.Skip(1).AsObservable(); }
        }
        ReactiveProperty<ReplicationBulkActionPacket> replicationBulk = new ReactiveProperty<ReplicationBulkActionPacket>();

        public IObservable<RoomJoinResponsePacket> RoomJoinReceived {
            get { return roomJoin.Skip(1).AsObservable(); }
        }
        ReactiveProperty<RoomJoinResponsePacket> roomJoin = new ReactiveProperty<RoomJoinResponsePacket>();

        public IObservable<RoomLeavePacket> RoomLeaveReceived {
            get { return roomLeave.Skip(1).AsObservable(); }
        }
        ReactiveProperty<RoomLeavePacket> roomLeave = new ReactiveProperty<RoomLeavePacket>();

        public IObservable<PlayerReadyPacket> PlayerReadyReceived {
            get { return playerReady.Skip(1).AsObservable(); }
        }
        ReactiveProperty<PlayerReadyPacket> playerReady = new ReactiveProperty<PlayerReadyPacket>();

        public IObservable<LeaderboardPacket> LeaderboardReceived {
            get { return leaderboard.Skip(1).AsObservable(); }
        }
        ReactiveProperty<LeaderboardPacket> leaderboard = new ReactiveProperty<LeaderboardPacket>();


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

        internal void Dispatch(IPacket p)
        {
            var t = (PacketType)p.Type;
            switch (t)
            {
                case PacketType.Ping:
                    ping.SetValueAndForceNotify((PingPacket)p);
                    break;

                case PacketType.Welcome:
                    welcome.SetValueAndForceNotify((WelcomePacket)p);
                    break;

                case PacketType.ReplicationAll:
                    replicationAll.SetValueAndForceNotify((ReplicationAllPacket)p);
                    break;

                case PacketType.ReplicationAction:
                    replication.SetValueAndForceNotify((ReplicationActionPacket)p);
                    break;

                case PacketType.ReplicationBulkAction:
                    replicationBulk.SetValueAndForceNotify((ReplicationBulkActionPacket)p);
                    break;

                case PacketType.RoomJoinResp:
                    roomJoin.SetValueAndForceNotify((RoomJoinResponsePacket)p);
                    break;

                case PacketType.RoomLeave:
                    roomLeave.SetValueAndForceNotify((RoomLeavePacket)p);
                    break;

                case PacketType.PlayerReady:
                    playerReady.SetValueAndForceNotify((PlayerReadyPacket)p);
                    break;

                case PacketType.Leaderboard:
                    leaderboard.SetValueAndForceNotify((LeaderboardPacket)p);
                    break;
                    
                default:
                    Debug.LogError($"packet handle not exist : {t}");
                    break;
            }
        }
    }
}
