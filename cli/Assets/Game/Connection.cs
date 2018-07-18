using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System;

namespace Assets.Game
{
    class Connection
    {
        // socket
        public int PlayerID { get; set; }
        public string RoomID { get; set; }
        public string Nickname { get; set; }

        public Socket RawSocket { set; private get; }

        public void Emit<TContext>(string eventString, TContext ctx)
        {
            if(RawSocket == null) { return; }
            var jobj = JObject.FromObject(ctx);
            RawSocket.Emit(eventString, jobj);
        }

        public void Emit(string eventString)
        {
            if (RawSocket == null) { return; }
            RawSocket.Emit(eventString);
        }

        public void On<TContext>(string eventString, Action<TContext> fn)
        {
            if (RawSocket == null) { return; }
            RawSocket.On(eventString, (data) =>
            {
                var jobj = data as JObject;
                var ctx = jobj.ToObject<TContext>();
                fn(ctx);
            });
        }

        public void On(string eventString, Action fn)
        {
            if (RawSocket == null) { return; }
            RawSocket.On(eventString, fn);
        }

        public void Off(string eventString)
        {
            if (RawSocket == null) { return; }
            RawSocket.Off(eventString);
        }
    }
}
