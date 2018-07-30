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
        int sent = 9999;
        int received = 9999;

        private void Awake()
        {
            Debug.Assert(pingText != null);
        }

        void Start()
        {
            var checker = PingChecker.Instance;
            var conn = ConnectionManager.Instance;

            checker.LatencyObservable.Subscribe(v => ping = v).AddTo(gameObject);
            conn.SentBytes.Subscribe(len => sent = len).AddTo(gameObject);
            conn.ReceivedBytes.Subscribe(len => received = len).AddTo(gameObject);
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

                var pingStr = $"ping: {this.ping}ms";
                var receivedStr = $"recv: {received * 8}b/s";
                var sentStr = $"sent: {sent * 8}b/s";
                var msg = string.Join(" ", new string[] { pingStr, receivedStr, sentStr });
                pingText.text = msg;

                yield return null;
            }
        }
    }
}
