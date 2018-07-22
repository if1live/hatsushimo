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
            get { return replication.AsObservable(); }
        }
        ReactiveProperty<ReplicationActionPacket> replication = new ReactiveProperty<ReplicationActionPacket>();

        public void ApplyReplication(ReplicationActionPacket p) { replication.Value = p; }


        private void Start()
        {
            ReplicationReceived.ObserveOnMainThread().Subscribe(packet =>
            {
                id = packet.ID;
                nickname = packet.Extra;

                DirX = packet.Dir.X;
                DirY = packet.Dir.Y;
                Speed = packet.Speed;
                
                var pos = new Vector3(packet.Pos.X, packet.Pos.Y, 0);
                transform.position = pos;
            }).AddTo(gameObject);
        }
    }
}
