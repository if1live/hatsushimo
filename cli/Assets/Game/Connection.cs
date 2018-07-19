using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Client;
using System;
using UnityEngine;

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

        public void EmitBytes(string eventString, byte[] bytes)
        {
            if(RawSocket == null) { return; }
            RawSocket.Emit(eventString, bytes);
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
                Debug.Assert(jobj != null, "cannot casting to JObject");
                var ctx = jobj.ToObject<TContext>();
                fn(ctx);
            });
        }

        public void OnBytes(string eventString, Action<byte[]> fn)
        {
            if (RawSocket == null) { return; }
            RawSocket.On(eventString, (data) =>
            {
                var bytes = data as byte[];
                Debug.Assert(bytes != null, "cannot casting to byte array");
                fn(bytes);
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

        public void Off(string eventString, IListener fn)
        {
            if(RawSocket == null) { return; }
            RawSocket.Off(eventString, fn);
        }
    }
}
