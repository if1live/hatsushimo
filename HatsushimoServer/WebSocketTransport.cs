using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using WebSocketSharp;
using WebSocketSharp.Server;

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
            // 연결 끊은것을 연결 종료 패킷처럼 다루면
            // 상위 레이어에서의 처리가 간단해진다
            var p = new DisconnectPacket();
            session.Send(p);

            // session layer
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

        readonly ConcurrentQueue<WebSocketDatagram> sendQueue = new ConcurrentQueue<WebSocketDatagram>();
        readonly ConcurrentQueue<WebSocketDatagram> recvQueue = new ConcurrentQueue<WebSocketDatagram>();

        public void Recv(string id, byte[] data)
        {
            var pair = new WebSocketDatagram(id, data);
            recvQueue.Enqueue(pair);
        }

        public void Send(string id, byte[] data)
        {
            var pair = new WebSocketDatagram(id, data);
            sendQueue.Enqueue(pair);
        }

        public void Close(string id)
        {
            IWebSocketSession s = null;
            if (Sessions.TryGetSession(id, out s))
            {
                Sessions.CloseSession(s.ID);
            }
        }

        public List<WebSocketDatagram> FlushReceivedDatagrams()
        {
            var retval = new List<WebSocketDatagram>();
            WebSocketDatagram datagram = new WebSocketDatagram();
            while (recvQueue.TryDequeue(out datagram))
            {
                retval.Add(datagram);
            }
            return retval;
        }

        void HandleSend(WebSocketDatagram p)
        {
            Debug.Assert(Sessions != null, "sessions not exist!");
            IWebSocketSession session = null;
            if (Sessions.TryGetSession(p.ID, out session))
            {
                Sessions.SendTo(p.Data, session.ID);
            }
        }

        public async void StartSendLoop()
        {
            while (true)
            {
                if (Sessions == null)
                {
                    var interval = TimeSpan.FromMilliseconds(100);
                    await Task.Delay(interval);
                    continue;
                }

                WebSocketDatagram datagram = new WebSocketDatagram();
                if (sendQueue.TryDequeue(out datagram))
                {
                    HandleSend(datagram);
                }
                else
                {
                    var interval = TimeSpan.FromMilliseconds(1000 / 100);
                    await Task.Delay(interval);
                }
            }
        }
    }
}
