using System;
using System.Diagnostics;
using Hatsushimo.NetChan;

namespace Mikazuki.NetChan
{
    public class NetworkStack
    {
        public static ITransportLayer<string> Transport
        {
            get
            {
                Debug.Assert(_webSocketTransport != null, "websocket transport layer not registered");
                return _webSocketTransport;
            }
        }

        static WebSocketTransportLayer _webSocketTransport;
        public static WebSocketTransportLayer WebSocketTransportLayer
        {
            get { return _webSocketTransport; }
        }

        public static SessionLayer Session
        {
            get
            {
                Debug.Assert(_session != null, "session layer not registered");
                return _session;
            }
        }
        static SessionLayer _session;

        public static void Register(WebSocketTransportLayer layer)
        {
            _webSocketTransport = layer;
        }

        public static void Register(SessionLayer layer)
        {
            _session = layer;
        }
    }
}
