using Assets.Game.NetChan;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class PingDisplay : MonoBehaviour 
    {
        public Text pingText = null;
        public long intervalMillis = 3000;

        private void Awake()
        {
            Debug.Assert(pingText != null);
        }

        private void Start()
        {
            var checker = LatencyChecker.Instance;
            Debug.Assert(checker != null);

            checker.LatencyObservable.ObserveOnMainThread().Subscribe(latency =>
            {
                var msg = $"ping: {latency}ms";
                pingText.text = msg;
            }).AddTo(gameObject);
        }
    }
}
