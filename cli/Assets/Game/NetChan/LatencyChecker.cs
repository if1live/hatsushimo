using Assets.Game.Packets;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Game.NetChan
{
    public class LatencyChecker : MonoBehaviour
    {
        public static LatencyChecker Instance = null;

        public int intervalMillis = 1000;

        public IObservable<int> LatencyObservable {
            get { return latency.AsObservable(); }
        }
        ReactiveProperty<int> latency = new ReactiveProperty<int>(99999);

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

        void OnReceivePong(StatusPongPacket packet)
        {
            int now = TimeUtils.NowMillis;
            int diff = now - packet.millis;
            latency.Value = diff;
        }

        void SendPing(Connection conn)
        {
            var packet = new StatusPingPacket()
            {
                millis = TimeUtils.NowMillis,
            };
            conn.EmitPacket(Events.STATUS_PING, packet);
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;

            var mgr = ConnectionManager.Instance;
            if(mgr == null) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.STATUS_PONG);
        }
    }
}
