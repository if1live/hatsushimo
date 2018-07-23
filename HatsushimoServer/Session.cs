using System.Collections.Generic;
using System.Diagnostics;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;

namespace HatsushimoServer
{
    public class Session
    {
        static readonly PacketCodec codec = MyPacketCodec.Create();
        readonly ITransport<string> transport;

        public string ID { get { return transport.ID; } }

        public Session(ITransport<string> transport)
        {
            this.transport = transport;

            var layer = SessionLayer.Layer;
            layer.Register(this);
        }

        public void Send(IPacket p)
        {
            var bytes = codec.Encode(p);
            transport.Send(bytes);
        }

        public void Close()
        {
            var layer = SessionLayer.Layer;
            layer.UnRegister(this);

            transport.Close();
        }
    }

    public class SessionLayer
    {
        public readonly static SessionLayer Layer = new SessionLayer();

        public readonly PacketCodec codec = MyPacketCodec.Create();

        public readonly Dictionary<string, Session> table = new Dictionary<string, Session>();

        public void Register(Session s) {
            table[s.ID] = s;
        }
        public void UnRegister(Session s) {
            table.Remove(s.ID);
        }
        public bool TryGetSession(string sessionID, out Session s) {
            return table.TryGetValue(sessionID, out s);
        }
        public Session Get(string sessionID)
        {
            return table[sessionID];
        }

        public struct ReceivedPacket
        {
            public Session Session;
            public IPacket Packet;
        }

        public List<ReceivedPacket> FlushReceivedPackets()
        {
            var transport = WebSocketTransportLayer.Layer;
            var datagrams = transport.FlushReceivedDatagrams();
            var packets = new List<ReceivedPacket>(datagrams.Count);
            foreach (var d in datagrams)
            {
                Session session = null;
                var found = TryGetSession(d.ID, out session);
                Debug.Assert(found == true, "session must be registered");

                var packet = codec.Decode(d.Data);
                packets.Add(new ReceivedPacket()
                {
                    Session = session,
                    Packet = packet,
                });
            }
            return packets;
        }
    }
}
