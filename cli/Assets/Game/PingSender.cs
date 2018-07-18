using Assets.Game.Packets;
using System;
using System.Threading.Tasks;
using UniRx;

namespace Assets.Game
{
    class PingSender
    {
        public IObservable<long> LatencyObservable {
            get { return latency.AsObservable(); }
        }
        ReactiveProperty<long> latency = new ReactiveProperty<long>(99999);

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
            var ts = TimeUtils.GetTimestamp();
            var ctx = new StatusPing() { ts = ts };
            conn.Emit(Events.STATUS_PING, ctx);
        }

        void RegisterHandler(Connection conn)
        {
            conn.On<StatusPong>(Events.STATUS_PONG, (ctx) =>
            {
                var now = TimeUtils.GetTimestamp();
                var diff = now - ctx.ts;
                latency.Value = diff;
            });
        }

        void UnRegisterHandler(Connection conn)
        {
            conn.Off(Events.STATUS_PONG);
        }
    }
}
