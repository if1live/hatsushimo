using Assets.NetChan;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    public class PingDisplay : MonoBehaviour
    {
        public Text pingText = null;

        private void Awake()
        {
            Debug.Assert(pingText != null);
        }

        void Start()
        {
            var checker = PingChecker.Instance;

            checker.LatencyObservable.Subscribe(latency =>
            {
                var msg = $"ping: {latency}ms";
                pingText.text = msg;
            }).AddTo(gameObject);
        }
    }
}
