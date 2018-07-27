using System.Collections;
using Assets.NetChan;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    public class PingDisplay : MonoBehaviour
    {
        public Text pingText = null;

        int ping = 9999;

        private void Awake()
        {
            Debug.Assert(pingText != null);
        }

        void Start()
        {
            var checker = PingChecker.Instance;

            checker.LatencyObservable.Subscribe(latency =>
            {
                this.ping = latency;
            }).AddTo(gameObject);
        }

        void OnEnable()
        {
            coroutine = StartCoroutine(BeginNetworkStatus());
        }

        void OnDisable()
        {
            StopCoroutine(coroutine);
        }

        Coroutine coroutine;

        IEnumerator BeginNetworkStatus()
        {
            while (true)
            {
                var conn = ConnectionManager.Instance;

                var ping = $"ping: {this.ping}ms";
                var received = $"recv: {conn.ReceivedPerSecond*8}b/s";
                var sent = $"sent: {conn.SentPerSecond*8}b/s";
                var msg = string.Join(" ", new string[] { ping, received, sent });
                pingText.text = msg;

                yield return null;
            }
        }
    }
}
