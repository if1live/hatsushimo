using Assets.Game.InputSystem;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
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
            var inputmgr = InputManager.Instance;
            var joystick = inputmgr.moveJoystick;

            // 컨트롤러 놓은 경우
            // 0,0이 계속 발생해도 한번만 처리
            var stopStream = joystick.InputChanged
            .DistinctUntilChanged().Where(v => v == Vector3.zero);

            // 컨트롤러를 잡고 있는 동안에는 같은 값이 나올수 있다
            var moveStream = joystick.InputChanged
            .Where(v => v != Vector3.zero);

            stopStream.Subscribe(dir =>
            {
                var currPos = player.transform.position;
                // 이동방향이 없다 = 현위치를 도착으로 취급
                var action = InputAction.CreateMove(currPos);
                inputmgr.PushMove(action);
            });


            moveStream.Subscribe(dir =>
            {
                var currPos = player.transform.position;
                var delta = dir * 5;
                var nextPos = currPos + delta;
                var action = InputAction.CreateMove(nextPos);
                inputmgr.PushMove(action);
            });
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

                if (sqrLimit > sqrDistance)
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
