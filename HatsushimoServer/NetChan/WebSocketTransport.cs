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

namespace HatsushimoServer.NetChan
{
    using WebSocketDatagram = Datagram<string>;

    public class WebSocketSession : WebSocketBehavior
    {
        WebSocketTransport transport;
        Session session;

        protected override void OnOpen()
        {
            // transport layer
            var transportLayer = NetworkStack.WebSocketTransportLayer;
            transportLayer.Sessions = this.Sessions;
            transport = new WebSocketTransport(transportLayer, this);
            transport.Received.Subscribe(data => {
                transportLayer.Recv(transport.ID, data);
            });

            // session layer
            var sessionLayer = NetworkStack.Session;
            session = sessionLayer.CreateSessionWithLock(transport);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            // 소켓이 닫히는건 아래쪽 레이어에서 감지한다
            // 상위레이어로 소켓이 닫혔다는걸 알려주기
            var p = new DisconnectPacket();
            session.Send(p);

            NetworkStack.Session.CloseSession(session);
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

        Subject<byte[]> _received = new Subject<byte[]>();
        public IObservable<byte[]> Received => _received;

        public WebSocketTransport(WebSocketTransportLayer layer, WebSocketBehavior session)
        {
            this.session = session;
            this.layer = layer;
        }

        internal void Recv(byte[] data)
        {
            _received.OnNext(data);
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

    public class WebSocketTransportLayer : ITransportLayer<string>
    {
        public WebSocketSessionManager Sessions { private get; set; }

        Subject<WebSocketDatagram> _sent = new Subject<WebSocketDatagram>();

        Subject<WebSocketDatagram> _received = new Subject<WebSocketDatagram>();
        public IObservable<WebSocketDatagram> Received => _received;

        public WebSocketTransportLayer()
        {
            _sent.Subscribe(datagram => HandleSend(datagram));
        }

        internal void Recv(string id, byte[] data)
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
