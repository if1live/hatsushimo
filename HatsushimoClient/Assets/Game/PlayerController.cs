using Assets.Game.InputSystem;
using System.Collections;
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

            // TODO 가상 스틱같은거로 대신할것이다
            HandleCommand();
            HandleMove();
        }

        void HandleCommand()
        {
            var inputmgr = InputManager.Instance;

            float dir_x = 0;
            float dir_y = 0;

            // TODO 키 입력에 따라서 움직이기
            // TODO 움직이는 명령은 얼마나 자주 보내나?
            // TODO 가상패드같은거로 바꾸는게 좋을거같은데
            if (Input.GetKey(KeyCode.UpArrow))
            {
                dir_y = +1;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                dir_y = -1;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                dir_x = -1;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                dir_x = +1;
            }

            var action = InputAction.CreateMove(dir_x, dir_y);
            inputmgr.PushMove(action);
        }

        void HandleMove()
        {
            var inputmgr = InputManager.Instance;
            if (Input.GetKeyDown(KeyCode.Z))
            {
                inputmgr.PushCommand(InputAction.CreateCommand(1));
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                inputmgr.PushCommand(InputAction.CreateCommand(2));
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
