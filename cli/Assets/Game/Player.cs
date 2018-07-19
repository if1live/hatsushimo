using Assets.Game.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class Player : MonoBehaviour
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

        IObservable<PlayerStatus> StatusObservable {
            get { return status.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerStatus> status = new ReactiveProperty<PlayerStatus>(null);

        IObservable<PlayerInfo> InfoObservable {
            get { return info.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerInfo> info = new ReactiveProperty<PlayerInfo>(null);

        public void ApplyStatus(PlayerStatus s) { status.Value = s; }
        public void ApplyInfo(PlayerInfo s) { info.Value = s; }


        private void Start()
        {
            StatusObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                Debug.Assert(packet.id == id, $"id mismatch: my={id} packet={packet.id}");

                DirX = packet.dir_x;
                DirY = packet.dir_y;
                Speed = packet.speed;

                var pos = new Vector3(packet.pos_x, packet.pos_y, 0);
                transform.position = pos;
            }).AddTo(gameObject);

            InfoObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                id = ctx.id;
                nickname = ctx.nickname;

                var pos = new Vector3(ctx.pos_x, ctx.pos_y, 0);
                transform.position = pos;
            }).AddTo(gameObject);
        }
    }
}
