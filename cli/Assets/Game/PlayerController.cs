using Assets.Game.Packets;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    [RequireComponent(typeof(Player))]
    class PlayerController : MonoBehaviour 
    {
        Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void Start()
        {
        }

        private void OnEnable()
        {
            Debug.Assert(coroutine_move == null);
            coroutine_move = StartCoroutine(BeginPredictedMove());
        }


        private void OnDisable()
        {
            Debug.Assert(coroutine_move != null);
            StopCoroutine(coroutine_move);
            coroutine_move = null;
        }


        Coroutine coroutine_move;

        private void Update()
        {
            if (player.IsOwner == false)
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
            if (Input.GetKey(KeyCode.UpArrow))
            {
                packet.dir_y = +1;
                packetExist = true;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                packet.dir_y = -1;
                packetExist = true;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                packet.dir_x = -1;
                packetExist = true;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                packet.dir_x = +1;
                packetExist = true;
            }


            if (packetExist)
            {
                var conn = ConnectionManager.Instance.Conn;
                conn.Emit(Events.MOVE, packet);
            }
            else
            {
                var conn = ConnectionManager.Instance.Conn;
                conn.Emit(Events.MOVE, packet);

            }
        }

        IEnumerator BeginPredictedMove()
        {
            while (true)
            {
                var dt = Time.deltaTime;
                var dir = new Vector3(player.DirX, player.DirY, 0);
                var speed = player.Speed;
                var delta = dir * speed * dt;

                var pos = transform.position;
                var nextPos = pos + delta;
                transform.position = nextPos;

                yield return null;
            }
        }
    }
}
