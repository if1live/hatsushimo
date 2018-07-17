using Assets.Game.Types;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class Player : MonoBehaviour
    {
        public string playerID;

        public float DirX;
        public float DirY;
        public float Speed;

        public bool IsOwner {
            get
            {
                var myid = Connection.Instance.PlayerID;
                return (myid == playerID);
            }
        }

        ReactiveProperty<PlayerContext> status = new ReactiveProperty<PlayerContext>(null);

        private void Start()
        {
            // TODO remove null check
            status.Where(x => x != null).ObserveOnMainThread().Subscribe(ctx =>
            {
                playerID = ctx.playerID;
                DirX = ctx.dirX;
                DirY = ctx.dirY;
                Speed = ctx.speed;

                var pos = new Vector3(ctx.posX, ctx.posY, 0);
                transform.position = pos;
            });
        }

        public void ApplyStatus(PlayerContext ctx)
        {
            status.Value = ctx;
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
                dirX = 0,
                dirY = 0,
            };

            // TODO 키 입력에 따라서 움직이기
            // TODO 움직이는 명령은 얼마나 자주 보내나?
            // TODO 가상패드같은거로 바꾸는게 좋을거같은데
            if(Input.GetKey(KeyCode.UpArrow))
            {
                packet.dirY = +1;
                packetExist = true;
            }
            if(Input.GetKey(KeyCode.DownArrow))
            {
                packet.dirY = -1;
                packetExist = true;
            }
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                packet.dirX = -1;
                packetExist = true;
            }
            if(Input.GetKey(KeyCode.RightArrow))
            {
                packet.dirX = +1;
                packetExist = true;
            }
            

            if(packetExist)
            {
                var socket = SocketManager.Instance.MySocket;
                socket.Emit("move", packet);
            }
            else
            {
                var socket = SocketManager.Instance.MySocket;
                socket.Emit("move", packet);
            }
        }
    }
}
