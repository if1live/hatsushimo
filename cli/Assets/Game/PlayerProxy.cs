using UnityEngine;
using UniRx;
using System.Collections;

namespace Assets.Game
{
    [RequireComponent(typeof(Player))]
    class PlayerProxy : MonoBehaviour 
    {
        Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void Start()
        {
        }

        Coroutine coroutine_move;

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

        IEnumerator BeginPredictedMove()
        {
            while(true)
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
