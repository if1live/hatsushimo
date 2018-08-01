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
            coroutine_move = StartCoroutine(PlayerController.BeginPredictedMove(player, transform));
        }

        private void OnDisable()
        {
            Debug.Assert(coroutine_move != null);
            StopCoroutine(coroutine_move);
            coroutine_move = null;
        }
    }
}
