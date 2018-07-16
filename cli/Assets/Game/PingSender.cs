using Newtonsoft.Json;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Game
{
    class PingSender
    {
        const string EVENT_STATUS_PING = "status-ping";
        const string EVENT_STATUS_PONG = "status-pong";

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
                var diff = now - ctx.ts;
                Debug.Log($"ping: {diff}ms");
            });
        }

        public async void Initialize(Socket socket)
        {
            RegisterHandler(socket);

            while(true)
            {
                SendPing(socket);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
