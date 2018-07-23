using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Utils;

namespace HatsushimoServer
{
    public class Session
    {
        static readonly PacketCodec codec = MyPacketCodec.Create();
        readonly ITransport<string> transport;

        public int ID { get; private set; }
        public string TransportID { get { return transport.ID; } }

        public Session(int id, ITransport<string> transport)
        {
            this.ID = id;
            this.transport = transport;
        }

        public void Send(IPacket p)
        {
            var bytes = codec.Encode(p);
            transport.Send(bytes);
        }

        internal void CloseTransport()
        {
            transport.Close();
        }
    }

    // 세션 레이어를 통해서 유저를 관리한다
    public class SessionLayer
    {
        public readonly static SessionLayer Layer = new SessionLayer();

        public readonly PacketCodec codec = MyPacketCodec.Create();

        readonly Dictionary<int, Session> sessionIDSessionTable = new Dictionary<int, Session>();
        readonly Dictionary<string, int> transportIDSessionIDTable = new Dictionary<string, int>();

        readonly IEnumerator<int> sessionIDEnumerator = IDGenerator.MakeSessionID().GetEnumerator();

        public Session CreateSession(ITransport<string> transport)
        {
            // 플레이어에게는 고유번호 붙인다
            // 어떤 방에 들어가든 id는 유지된다
            sessionIDEnumerator.MoveNext();
            var sessionID = sessionIDEnumerator.Current;
            var session = new Session(sessionID, transport);

            // register
            Debug.Assert(sessionIDSessionTable.ContainsKey(sessionID) == false);
            Debug.Assert(transportIDSessionIDTable.ContainsKey(transport.ID) == false);
            sessionIDSessionTable[sessionID] = session;
            transportIDSessionIDTable[transport.ID] = sessionID;

            return session;
        }

        public void CloseSession(Session s)
        {
            // 패킷은 큐에 넣어서 처리된다
            // 패킷을 큐에 넣은후 연결이 닫힐 경우
            // 세션 정보를 즉시 없애면 누가 보냈는지 알수없다
            // trasport 쪽에서는 즉시 닫혀도 세션은 객체를 잠시 들고있다가 따로 삭제하자
            s.CloseTransport();
        }

        public void RemoveSession(Session s)
        {
            Debug.Assert(sessionIDSessionTable.ContainsKey(s.ID) == true);
            Debug.Assert(transportIDSessionIDTable.ContainsKey(s.TransportID) == true);
            sessionIDSessionTable.Remove(s.ID);
            transportIDSessionIDTable.Remove(s.TransportID);
        }

        public bool TryGetSession(int sessionID, out Session s)
        {
            return sessionIDSessionTable.TryGetValue(sessionID, out s);
        }
        public Session Get(int sessionID)
        {
            return sessionIDSessionTable[sessionID];
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
                int sessionID = 0;
                var idFound = transportIDSessionIDTable.TryGetValue(d.ID, out sessionID);
                Debug.Assert(idFound == true, "session must be registered");

                Session session = null;
                var sessionFound = TryGetSession(sessionID, out session);
                Debug.Assert(sessionFound == true, "session must be registered");

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
