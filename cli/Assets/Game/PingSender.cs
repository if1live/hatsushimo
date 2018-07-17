using Assets.Game.Types;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class PingSender
    {
        public const string EVENT_STATUS_PING = "status-ping";
        public const string EVENT_STATUS_PONG = "status-pong";

        public ReactiveProperty<long> Latency {
            get { return _latency; }
            private set { _latency = value; }
        }
        ReactiveProperty<long> _latency = new ReactiveProperty<long>(99999);

        readonly long intervalMillis;

        public PingSender(long intervalMillis)
        {
            this.intervalMillis = intervalMillis;
        }

        public void Setup()
        {
            var mgr = SocketManager.Instance;
            Debug.Assert(mgr != null);

            mgr.IsReady.Where(isReady => isReady == true).Subscribe(async _ =>
            {
                var socket = mgr.MySocket;
                RegisterHandler(socket);

                while (true)
                {
                    SendPing(socket);
                    await Task.Delay(TimeSpan.FromMilliseconds(intervalMillis));
                }
            });
        }

        public void Cleanup()
        {
            var mgr = SocketManager.Instance;
            if (mgr)
            {
                var socket = mgr.MySocket;
                UnRegisterHandler(socket);
            }
        }

        long GetTimestamp()
        {
            var now = DateTime.UtcNow;
            var zero = new DateTime(1970, 1, 1, 0, 0, 0);
            var diff = now - zero;
            var ts = (long)(diff.TotalMilliseconds);
            return ts;
        }

        void SendPing(MySocket socket)
        {
            var ts = GetTimestamp();
            var ctx = new StatusPing() { ts = ts };
            socket.Emit(EVENT_STATUS_PING, ctx);
        }

        void RegisterHandler(MySocket socket)
        {
            socket.On<StatusPong>(EVENT_STATUS_PONG, (ctx) =>
            {
                var now = GetTimestamp();
                var latency = now - ctx.ts;
                Latency.Value = latency;
            });
        }

        void UnRegisterHandler(MySocket socket)
        {
            socket.Off(EVENT_STATUS_PONG);
        }
    }
}
