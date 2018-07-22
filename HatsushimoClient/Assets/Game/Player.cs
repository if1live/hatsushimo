using Assets.NetChan;
using Hatsushimo.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    public class Player : MonoBehaviour
    {
        public int id;
        public string nickname;

        public float DirX;
        public float DirY;
        public float Speed;

        public bool IsOwner {
            get
            {
                var conn = ConnectionManager.Instance.Conn;
                var myid = conn.PlayerID;
                return (myid == id);
            }
        }

        IObservable<ReplicationActionPacket> ReplicationReceived {
            get { return replication.Skip(1).AsObservable(); }
        }
        ReactiveProperty<ReplicationActionPacket> replication = new ReactiveProperty<ReplicationActionPacket>();

        IObservable<PlayerInitial> InitialReceived {
            get { return initial.Skip(1).AsObservable(); }
        }
        ReactiveProperty<PlayerInitial> initial = new ReactiveProperty<PlayerInitial>();

        public void ApplyReplication(ReplicationActionPacket p) { replication.Value = p; }
        public void ApplyInitial(PlayerInitial p) { initial.Value = p; }


        private void Start()
        {
            ReplicationReceived.Subscribe(packet =>
            {
                Debug.Assert(packet.ID == id, $"id mismatch: my={id} packet={packet.ID} action={packet.Action}");

                nickname = packet.Extra;
                DirX = packet.Dir.X;
                DirY = packet.Dir.Y;
                Speed = packet.Speed;
                
                var pos = new Vector3(packet.Pos.X, packet.Pos.Y, 0);
                transform.position = pos;
            }).AddTo(gameObject);

            InitialReceived.Subscribe(packet =>
            {
                id = packet.ID;
                nickname = packet.Nickname;

                var pos = new Vector3(packet.Pos[0], packet.Pos[1], 0);
                transform.position = pos;
            }).AddTo(gameObject);
        }
    }
}
