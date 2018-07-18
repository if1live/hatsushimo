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
            StatusObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                Debug.Assert(ctx.id == id, "id mismatch");

                DirX = ctx.dir_x;
                DirY = ctx.dir_y;
                Speed = ctx.speed;

                var pos = new Vector3(ctx.pos_x, ctx.pos_y, 0);
                transform.position = pos;
            });

            InfoObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                id = ctx.id;
                nickname = ctx.nickname;
            });
        }

        private void Update()
        {
            if(IsOwner == false)
            {
                return;
            }


            bool packetExist = false;
            var packet = new MovePacket()
            {
                dir_x = 0,
                dir_y = 0,
            };

            // TODO 키 입력에 따라서 움직이기
            // TODO 움직이는 명령은 얼마나 자주 보내나?
            // TODO 가상패드같은거로 바꾸는게 좋을거같은데
            if(Input.GetKey(KeyCode.UpArrow))
            {
                packet.dir_y = +1;
                packetExist = true;
            }
            if(Input.GetKey(KeyCode.DownArrow))
            {
                packet.dir_y = -1;
                packetExist = true;
            }
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                packet.dir_x = -1;
                packetExist = true;
            }
            if(Input.GetKey(KeyCode.RightArrow))
            {
                packet.dir_x = +1;
                packetExist = true;
            }
            

            if(packetExist)
            {
                var conn = ConnectionManager.Instance.Conn;
                conn.Emit("move", packet);
            }
            else
            {
                var conn = ConnectionManager.Instance.Conn;
                conn.Emit("move", packet);
            }
        }
    }
}
