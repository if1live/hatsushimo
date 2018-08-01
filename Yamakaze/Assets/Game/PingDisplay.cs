using System;
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

        struct NetworkStatus
        {
            public int ping;
            public int send;
            public int recv;
        }

        private void Awake()
        {
            Debug.Assert(pingText != null);
        }

        void Start()
        {
            var checker = PingChecker.Instance;
            var conn = ConnectionManager.Instance;

            var pingStream = checker.LatencyObservable;
            var sendStream = conn.SentBytes;
            var recvStream = conn.ReceivedBytes;
            pingStream.CombineLatest(sendStream, recvStream, (ping, send, recv) => new NetworkStatus()
            {
                ping = ping,
                send = send,
                recv = recv,
            }).Subscribe(status =>
            {
                RefreshNetworkStatus(status.ping, status.send, status.recv);
            }).AddTo(this);
        }

        void RefreshNetworkStatus(int ping, int send, int recv)
        {
            var pingStr = $"ping: {ping}ms";
            var receivedStr = $"recv: {recv * 8}b/s";
            var sentStr = $"sent: {send * 8}b/s";
            var msg = string.Join("\n", new string[] { pingStr, receivedStr, sentStr });
            pingText.text = msg;
        }
    }
}
