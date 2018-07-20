using Assets.Game.NetChan;
using Assets.Game.Packets;
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

        IObservable<ReplicationActionPacket> ReplicationObservable {
            get { return replication.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<ReplicationActionPacket> replication = new ReactiveProperty<ReplicationActionPacket>(null);

        IObservable<PlayerInitial> InitialObservable {
            get { return initial.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerInitial> initial = new ReactiveProperty<PlayerInitial>(null);

        public void ApplyInitial(PlayerInitial s) { initial.Value = s; }
        public void ApplyReplication(ReplicationActionPacket p) { replication.Value = p; }


        private void Start()
        {
            ReplicationObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                Debug.Assert(packet.id == id, $"id mismatch: my={id} packet={packet.id}");

                DirX = packet.dir_x;
                DirY = packet.dir_y;
                Speed = packet.speed;

                var pos = new Vector3(packet.pos_x, packet.pos_y, 0);
                transform.position = pos;
            }).AddTo(gameObject);

            InitialObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                id = ctx.id;
                nickname = ctx.nickname;

                var pos = new Vector3(ctx.pos_x, ctx.pos_y, 0);
                transform.position = pos;
            }).AddTo(gameObject);
        }
    }
}
