using Assets.Game.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Game.NetChan
{
    public class LatencyChecker : MonoBehaviour
    {
        public static LatencyChecker Instance = null;

        public int intervalMillis = 1000;

        public const int INF_LATENCY = 99999;

        public IObservable<int> LatencyObservable {
            get { return latency.AsObservable(); }
        }
        ReactiveProperty<int> latency = new ReactiveProperty<int>(INF_LATENCY);

        // TODO latency 변화 추적이 목적
        // 버퍼를 다루는 stream으로 바꿀수 있을거같은데
        const int LATENCY_QUEUE_MAX_SIZE = 30;
        Queue<int> latencyQueue = new Queue<int>(LATENCY_QUEUE_MAX_SIZE);
        

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            ConnectionManager.Instance.ReadyObservable.Subscribe(async conn =>
            {
                conn.OnPacket<StatusPongPacket>(Events.STATUS_PONG, OnReceivePong);

                var delay = TimeSpan.FromMilliseconds(intervalMillis);
                while (true)
                {
                    SendPing(conn);
                    await Task.Delay(delay);
                }
            }).AddTo(gameObject);
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;

            var mgr = ConnectionManager.Instance;
            if (mgr == null) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.STATUS_PONG);
        }

        void OnReceivePong(StatusPongPacket packet)
        {
            int now = TimeUtils.NowMillis;
            int diff = now - packet.millis;
            PushLatency(diff);
        }

        void SendPing(Connection conn)
        {
            var packet = new StatusPingPacket()
            {
                millis = TimeUtils.NowMillis,
            };
            conn.EmitPacket(Events.STATUS_PING, packet);
        }

        void PushLatency(int millis)
        {
            latencyQueue.Enqueue(millis);
            if (latencyQueue.Count > LATENCY_QUEUE_MAX_SIZE)
            {
                latencyQueue.Dequeue();
            }

            latency.Value = millis;
        }
    }
}
