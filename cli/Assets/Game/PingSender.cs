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
            var millis = TimeUtils.NowMillis;
            byte[] bytes = BitConverter.GetBytes(millis);
            conn.EmitBytes(Events.STATUS_PING, bytes);
        }

        void RegisterHandler(Connection conn)
        {
            conn.OnBytes(Events.STATUS_PONG, (bytes) =>
            {
                int millis = BitConverter.ToInt32(bytes, 0);
                int now = TimeUtils.NowMillis;
                int diff = now - millis;
                latency.Value = diff;
            });
        }

        void UnRegisterHandler(Connection conn)
        {
            conn.Off(Events.STATUS_PONG);
        }
    }
}
