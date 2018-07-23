using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Reactive;
using System.Reactive.Subjects;

namespace HatsushimoServer
{
    using WebSocketDatagram = Datagram<string>;

    public class WebSocketSession : WebSocketBehavior
    {
        WebSocketTransport transport;
        Session session;

        protected override void OnOpen()
        {
            // transport layer
            var transportLayer = WebSocketTransportLayer.Layer;
            transportLayer.Sessions = this.Sessions;
            transport = new WebSocketTransport(transportLayer, this);

            // session layer
            var sessionLayer = SessionLayer.Layer;
            session = sessionLayer.CreateSession(transport);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            var p = new DisconnectPacket();
            session.Send(p);

            SessionLayer.Layer.CloseSession(session);
            session = null;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            transport.Recv(e.RawData);
        }
        protected override void OnError(ErrorEventArgs e)
        {
            // TODO ?
        }
    }

    public class WebSocketTransport : ITransport<string>
    {
        readonly WebSocketBehavior session;
        readonly WebSocketTransportLayer layer;

        public string ID => session.ID;

        public WebSocketTransport(WebSocketTransportLayer layer, WebSocketBehavior session)
        {
            this.session = session;
            this.layer = layer;
        }

        public void Recv(byte[] data)
        {
            layer.Recv(session.ID, data);
        }

        public void Send(byte[] data)
        {
            layer.Send(session.ID, data);
        }

        public void Close()
        {
            layer.Close(session.ID);
        }
    }

    public class WebSocketTransportLayer
    {
        public static readonly WebSocketTransportLayer Layer = new WebSocketTransportLayer();

        public WebSocketSessionManager Sessions { get; set; }

        Subject<WebSocketDatagram> _sent = new Subject<WebSocketDatagram>();

        Subject<WebSocketDatagram> _received = new Subject<WebSocketDatagram>();
        public IObservable<WebSocketDatagram> Received { get { return _received; } }

        public WebSocketTransportLayer()
        {
            _sent.Subscribe(datagram => HandleSend(datagram));
        }

        public void Recv(string id, byte[] data)
        {
            var pair = new WebSocketDatagram(id, data);
            _received.OnNext(pair);
        }

        public void Send(string id, byte[] data)
        {
            var pair = new WebSocketDatagram(id, data);
            _sent.OnNext(pair);
        }

        public void Close(string id)
        {
            IWebSocketSession s = null;
            if (Sessions.TryGetSession(id, out s))
            {
                Sessions.CloseSession(s.ID);
            }
        }

        void HandleSend(WebSocketDatagram p)
        {
            Debug.Assert(Sessions != null, "sessions not exist!");
            IWebSocketSession session = null;
            if (Sessions.TryGetSession(p.ID, out session))
            {
                if (session.ConnectionState == WebSocketState.Open)
                {
                    Sessions.SendTo(p.Data, session.ID);
                }
                else
                {
                    Console.WriteLine($"cannot send: id={p.ID} state={session.ConnectionState}");
                }
            }
        }
    }
}
