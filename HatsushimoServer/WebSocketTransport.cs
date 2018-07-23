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
            // TODO websockt 초기화까지 기다리는건
            // 다른거로 바꿀수 있을거같은데
            while (Sessions == null)
            {
                var interval = TimeSpan.FromMilliseconds(100);
                await Task.Delay(interval);
            }

            while (true)
            {
                WebSocketDatagram datagram = new WebSocketDatagram();
                while (sendQueue.TryDequeue(out datagram))
                {
                    HandleSend(datagram);
                }

                // TODO 전송 큐에 뭐가 들어있을떄만 돌아가독 할수 있을같은데
                var interval = TimeSpan.FromMilliseconds(10);
                await Task.Delay(interval);
            }
        }
    }
}
