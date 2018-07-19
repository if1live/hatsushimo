using Assets.Game.Packets;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class PingSender
    {
        public IObservable<int> LatencyObservable {
            get { return latency.AsObservable(); }
        }
        ReactiveProperty<int> latency = new ReactiveProperty<int>(99999);

        readonly long intervalMillis;

        public PingSender(long intervalMillis)
        {
            this.intervalMillis = intervalMillis;
        }

        public void Setup()
        {
            ConnectionManager.Instance.ReadyObservable.Subscribe(async conn =>
            {
                RegisterHandler(conn);

                var delay = TimeSpan.FromMilliseconds(intervalMillis);
                while (true)
                {
                    SendPing(conn);
                    await Task.Delay(delay);
                }
            });
        }

        public void Cleanup()
        {
            var mgr = ConnectionManager.Instance;
            if (mgr)
            {
                var conn = mgr.Conn;
                UnRegisterHandler(conn);
            }
        }

        void SendPing(Connection conn)
        {
            var packet = new StatusPingPacket()
            {
                millis = TimeUtils.NowMillis,
            };
            conn.EmitPacket(Events.STATUS_PING, packet);
        }

        void RegisterHandler(Connection conn)
        {
            conn.OnPacket<StatusPongPacket>(Events.STATUS_PONG, (packet) =>
            {
                int now = TimeUtils.NowMillis;
                int diff = now - packet.millis;
                latency.Value = diff;
            });
        }

        void UnRegisterHandler(Connection conn)
        {
            conn.Off(Events.STATUS_PONG);
        }
    }
}
