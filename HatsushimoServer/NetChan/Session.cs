using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Utils;

namespace HatsushimoServer.NetChan
{
    public class Session
    {
        public const string DefaultNickname = "[Blank]";

        static readonly PacketCodec codec = new PacketCodec();
        readonly ITransport<string> transport;
        public long LastHeartbeatTimestamp { get; private set; } = 0;

        public int ID { get; private set; }
        public string WorldID { get; set; }
        public string Nickname { get; set; }
        public int UserID { get; set; }

        internal string TransportID { get { return transport.ID; } }

        public Session(int id, ITransport<string> transport)
        {
            this.ID = id;
            this.transport = transport;
            this.Nickname = DefaultNickname;
            RefreshHeartbeat();
        }

        public void RefreshHeartbeat()
        {
            LastHeartbeatTimestamp = TimeUtils.NowTimestamp;
        }

        public void Send<T>(T p) where T : IPacket
        {
            var bytes = codec.Encode(p);
            transport.Send(bytes);
        }

        internal void CloseTransport()
        {
            UserID = -1;
            transport.Close();
        }
    }

    // 세션 레이어를 통해서 유저를 관리한다
    public class SessionLayer : ServerPacketReceiver
    {
        // 연결이 생기거나 끊어지면 연결 목록이 바뀐다
        // 이것은 멀티 쓰레드에서 문제가 생길 가능성이 있어보이니 락을 걸자
        Object writeLock = new Object();
        readonly IDictionary<int, Session> sessionIDSessionTable = new Dictionary<int, Session>();
        readonly IDictionary<string, int> transportIDSessionIDTable = new Dictionary<string, int>();

        readonly IEnumerator<int> sessionIDEnumerator = IDGenerator.MakeSessionID().GetEnumerator();

        public SessionLayer(ITransportLayer<string> transport)
        {
            transport.Received.Subscribe(datagram =>
            {
                int sessionID = 0;
                var idFound = transportIDSessionIDTable.TryGetValue(datagram.ID, out sessionID);
                Debug.Assert(idFound == true, "session must be registered");

                Session session = null;
                var sessionFound = TryGetSession(sessionID, out session);
                Debug.Assert(sessionFound == true, "session must be registered");

                OnNext(session, datagram.Data);
            });
        }

        public Session CreateSessionWithLock(ITransport<string> transport)
        {
            lock (writeLock) { return CreateSession(transport); }
        }

        Session CreateSession(ITransport<string> transport)
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

        public void RemoveSessionWithLock(Session s)
        {
            lock (writeLock) { RemoveSession(s); }
        }
        void RemoveSession(Session s)
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
    }
}
