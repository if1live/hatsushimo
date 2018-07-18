using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class PingDisplay : MonoBehaviour 
    {
        public Text pingText = null;
        public long intervalMillis = 3000;

        PingSender sender = null;

        private void Awake()
        {
            Debug.Assert(pingText != null);
        }

        private void Start()
        {
            sender = new PingSender(intervalMillis);
            sender.Setup();

            sender.LatencyObservable.ObserveOnMainThread().Subscribe(latency =>
            {
                var msg = $"ping: {latency}ms";
                pingText.text = msg;
            });
        }

        private void OnDestroy()
        {
            sender.Cleanup();
        }
    }
}
