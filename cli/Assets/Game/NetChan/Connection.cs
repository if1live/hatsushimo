using Assets.Game.Packets;
using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Client;
using System;
using UnityEngine;

namespace Assets.Game.NetChan
{
    interface IConnectionInner
    {
        void Emit<TContext>(Events ev, TContext ctx);
        void EmitBytes(Events ev, byte[] bytes);
        void Emit(Events ev);

        void On<TContext>(Events ev, Action<TContext> fn);
        void OnBytes(Events ev, Action<byte[]> fn);
        void On(Events ev, Action fn);

        void Off(Events ev);
        void Off(Events ev, IListener fn);
    }

    class Connection
    {
        // socket
        public int PlayerID { get; set; }
        public string RoomID { get; set; }
        public string Nickname { get; set; }

        public Socket RawSocket {
            set {
                if(value != null)
                {
                    inner = new SocketConnection(value);
                }
                else
                {
                    inner = new NullConnection();
                }
            }
        }
        IConnectionInner inner = new NullConnection();

        public void Emit(Events ev) { inner.Emit(ev); }
        public void Emit<TContext>(Events ev, TContext ctx) { inner.Emit(ev, ctx); }
        public void EmitBytes(Events ev, byte[] bytes) { inner.EmitBytes(ev, bytes); }
        
        public void EmitPacket(Events ev, ISerializePacket packet) { EmitBytes(ev, packet.Serialize()); }

        public void On(Events ev, Action fn) { inner.On(ev, fn); }
        public void On<TContext>(Events ev, Action<TContext> fn) { inner.On(ev, fn); }
        public void OnBytes(Events ev, Action<byte[]> fn) { inner.OnBytes(ev, fn); }

        public void OnPacket<TPacket>(Events ev, Action<TPacket> fn)
            where TPacket : IDeserializePacket, new()
        {
            inner.OnBytes(ev, (data) =>
            {
                var bytes = data as byte[];
                Debug.Assert(bytes != null, "cannot casting to byte array");
                TPacket packet = new TPacket();
                packet.Deserialize(bytes);
                fn(packet);
            });
        }

        public void Off(Events ev) { inner.Off(ev); }
        public void Off(Events ev, IListener fn) { inner.Off(ev, fn); }
    }

    class SocketConnection : IConnectionInner
    {
        Socket sock;

        public SocketConnection(Socket sock)
        {
            this.sock = sock;
        }

        public void Emit<TContext>(Events ev, TContext ctx)
        {
            var jobj = JObject.FromObject(ctx);
            sock.Emit(ev.MakeString(), jobj);
        }

        public void EmitBytes(Events ev, byte[] bytes)
        {
            sock.Emit(ev.MakeString(), bytes);
        }

        public void Emit(Events ev)
        {
            sock.Emit(ev.MakeString());
        }

        public void On<TContext>(Events ev, Action<TContext> fn)
        {
            sock.On(ev.MakeString(), (data) =>
            {
                var jobj = data as JObject;
                Debug.Assert(jobj != null, "cannot casting to JObject");
                var ctx = jobj.ToObject<TContext>();
                fn(ctx);
            });
        }

        public void OnBytes(Events ev, Action<byte[]> fn)
        {
            sock.On(ev.MakeString(), (data) =>
            {
                var bytes = data as byte[];
                Debug.Assert(bytes != null, "cannot casting to byte array");
                fn(bytes);
            });
        }

        public void On(Events ev, Action fn)
        {
            sock.On(ev.MakeString(), fn);
        }

        public void Off(Events ev)
        {
            sock.Off(ev.MakeString());
        }

        public void Off(Events ev, IListener fn)
        {
            sock.Off(ev.MakeString(), fn);
        }
    }

    class NullConnection : IConnectionInner
    {
        public void Emit<TContext>(Events ev, TContext ctx) { }
        public void Emit(Events ev) { }
        public void EmitBytes(Events ev, byte[] bytes) { }

        public void Off(Events ev) { }
        public void Off(Events ev, IListener fn) { }

        public void On<TContext>(Events ev, Action<TContext> fn) { }
        public void On(Events ev, Action fn) { }
        public void OnBytes(Events ev, Action<byte[]> fn) { }
    }
}
