using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System;

namespace Assets.Game
{
    class MySocket
    {
        Socket _socket;

        public Socket RawSocket { get { return _socket; } }

        public MySocket(Socket socket)
        {
            this._socket = socket;
        }

        public void Emit<TContext>(string eventString, TContext ctx)
        {
            var jobj = JObject.FromObject(ctx);
            _socket.Emit(eventString, jobj);
        }

        public void Emit(string eventString)
        {
            _socket.Emit(eventString);
        }

        public void On<TContext>(string eventString, Action<TContext> fn)
        {
            _socket.On(eventString, (data) =>
            {
                var jobj = data as JObject;
                var ctx = jobj.ToObject<TContext>();
                fn(ctx);
            });
        }

        public void On(string eventString, Action fn)
        {
            _socket.On(eventString, fn);
        }

        public void Off(string eventString)
        {
            _socket.Off(eventString);
        }
    }
}
