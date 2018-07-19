using Assets.Game.Packets;
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

        public void Emit<TContext>(Events ev, TContext ctx)
        {
            if(RawSocket == null) { return; }
            var jobj = JObject.FromObject(ctx);
            RawSocket.Emit(ev.MakeString(), jobj);
        }

        public void EmitBytes(Events ev, byte[] bytes)
        {
            if(RawSocket == null) { return; }
            RawSocket.Emit(ev.MakeString(), bytes);
        }

        public void EmitPacket(Events ev, ISerializePacket packet)
        {
            if (RawSocket == null) { return; }
            EmitBytes(ev, packet.Serialize());
        }

        public void Emit(Events ev)
        {
            if (RawSocket == null) { return; }
            RawSocket.Emit(ev.MakeString());
        }

        public void On<TContext>(Events ev, Action<TContext> fn)
        {
            if (RawSocket == null) { return; }
            RawSocket.On(ev.MakeString(), (data) =>
            {
                var jobj = data as JObject;
                Debug.Assert(jobj != null, "cannot casting to JObject");
                var ctx = jobj.ToObject<TContext>();
                fn(ctx);
            });
        }

        public void OnBytes(Events ev, Action<byte[]> fn)
        {
            if (RawSocket == null) { return; }
            RawSocket.On(ev.MakeString(), (data) =>
            {
                var bytes = data as byte[];
                Debug.Assert(bytes != null, "cannot casting to byte array");
                fn(bytes);
            });
        }

        public void OnPacket<TPacket>(Events ev, Action<TPacket> fn)
            where TPacket : IDeserializePacket, new()
        {
            if(RawSocket == null) { return; }
            RawSocket.On(ev.MakeString(), (data) =>
            {
                var bytes = data as byte[];
                Debug.Assert(bytes != null, "cannot casting to byte array");
                TPacket packet = new TPacket();
                packet.Deserialize(bytes);
                fn(packet);
            });
        }

        public void On(Events ev, Action fn)
        {
            if (RawSocket == null) { return; }
            RawSocket.On(ev.MakeString(), fn);
        }

        public void Off(Events ev)
        {
            if (RawSocket == null) { return; }
            RawSocket.Off(ev.MakeString());
        }

        public void Off(Events ev, IListener fn)
        {
            if(RawSocket == null) { return; }
            RawSocket.Off(ev.MakeString(), fn);
        }
    }
}
