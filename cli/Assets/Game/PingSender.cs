using Newtonsoft.Json;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class PingSender
    {
        const string EVENT_STATUS_PING = "status-ping";
        const string EVENT_STATUS_PONG = "status-pong";

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
                var socket = mgr.Socket;
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
                var socket = mgr.Socket;
                UnRegisterHandler(socket);
            }
        }

        struct StatusPing
        {
            public long ts;
        }

        struct StatusPong
        {
            public long ts;
        }

        long GetTimestamp()
        {
            var now = DateTime.UtcNow;
            var zero = new DateTime(1970, 1, 1, 0, 0, 0);
            var diff = now - zero;
            var ts = (long)(diff.TotalMilliseconds);
            return ts;
        }

        void SendPing(Socket socket)
        {
            var ts = GetTimestamp();
            var ctx = new StatusPing() { ts = ts };
            var json = JsonConvert.SerializeObject(ctx);
            socket.Emit(EVENT_STATUS_PING, json);
        }

        void RegisterHandler(Socket socket)
        {
            socket.On(EVENT_STATUS_PONG, (data) =>
            {
                string str = data.ToString();
                var ctx = JsonConvert.DeserializeObject<StatusPong>(str);
                var now = GetTimestamp();
                var latency = now - ctx.ts;
                Latency.Value = latency;
            });
        }

        void UnRegisterHandler(Socket socket)
        {
            socket.Off(EVENT_STATUS_PONG);
        }
    }
}
