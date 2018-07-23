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
            coroutine_move = StartCoroutine(BeginPredictedMove(player, transform));
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

            // TODO 모바일에 맞는 입력방식도 구현하기
            HandleMove_UnityBasic();
            HandleCommand_UnityBasic();
        }

        void HandleMove_UnityBasic()
        {
            var inputmgr = InputManager.Instance;

            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");
            var dir = new Vector3(horizontal, vertical);

            var currPos = player.transform.position;
            if (dir == Vector3.zero)
            {
                // 이동방향이 없다 = 현위치를 도착으로 취급
                var action = InputAction.CreateMove(currPos);
                inputmgr.PushMove(action);
            }
            else
            {
                var delta = dir * 5;
                var nextPos = currPos + delta;
                var action = InputAction.CreateMove(nextPos);
                inputmgr.PushMove(action);
            }
        }

        void HandleCommand_UnityBasic()
        {
            var inputmgr = InputManager.Instance;
            if (Input.GetButtonDown("Fire1"))
            {
                inputmgr.PushCommand(InputAction.CreateCommand(1));
            }
            if (Input.GetButtonDown("Fire2"))
            {
                inputmgr.PushCommand(InputAction.CreateCommand(2));
            }
        }

        public static IEnumerator BeginPredictedMove(Player player, Transform transform)
        {
            while (true)
            {
                var dt = Time.deltaTime;
                var limit = dt * player.Speed;
                var sqrLimit = limit * limit;

                var diff = player.TargetPos - transform.position;
                var dir = diff.normalized;
                var sqrDistance = diff.sqrMagnitude;

                if(sqrLimit > sqrDistance)
                {
                    transform.position = player.TargetPos;
                }
                else
                {
                    var speed = player.Speed;
                    var delta = dir * speed * dt;

                    var pos = transform.position;
                    var nextPos = pos + delta;
                    transform.position = nextPos;
                }

                yield return null;
            }
        }
    }
}
