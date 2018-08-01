using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using NLog;

namespace Mikazuki.NetChan
{
    public enum SessionState
    {
        None = 0,
        Connecting,
        Connected,
        Disconnected,
    }

    public class Session
    {
        public const string DefaultNickname = "[Blank]";

        static readonly PacketCodec codec = new PacketCodec();
        readonly ITransport<string> transport;
        public long LastHeartbeatTimestamp { get; private set; } = 0;

        public SessionState State { get; internal set; }
        public int ID { get; private set; }
        public string WorldID { get; set; }
        public string Nickname { get; set; }
        public int UserID { get; set; }

        internal string TransportID { get { return transport.ID; } }

        public Session(int id, ITransport<string> transport)
        {
            this.State = SessionState.Connected;
            this.ID = id;
            this.transport = transport;
            this.Nickname = DefaultNickname;
            RefreshHeartbeat();

            // TODO unsubscribe? dispose?
            var flushInterval = TimeSpan.FromMilliseconds(300);
            Observable.Interval(flushInterval).Subscribe(_ => Flush());
        }

        public void RefreshHeartbeat()
        {
            LastHeartbeatTimestamp = TimeUtils.NowTimestamp;
        }

        public bool SendImmediate<T>(T p) where T : IPacket
        {
            if (State != SessionState.Connected) { return false; }
            var bytes = codec.Encode(p);
            transport.Send(bytes);
            return true;
        }

        // 즉시 처리할 보낼 필요 없는 패킷을 모아서 한번에 보내기
        readonly List<byte[]> queue = new List<byte[]>();
        readonly Object lockobj = new Object();

        public bool SendLazy<T>(T p) where T : IPacket
        {
            if (State != SessionState.Connected) { return false; }
            var bytes = codec.Encode(p);
            lock (lockobj)
            {
                queue.Add(bytes);
            }
            return true;
        }

        void Flush()
        {
            List<byte[]> cloned = null;
            lock (lockobj)
            {
                if (queue.Count == 0) { return; }
                cloned = new List<byte[]>(queue);
                queue.Clear();
            }

            var bulk = ByteJoin.Combine(cloned);
            transport.Send(bulk);
        }

        public void CloseTransport()
        {
            UserID = -1;
            transport.Close();
        }
    }

    // 세션 레이어를 통해서 유저를 관리한다
    public class SessionLayer : RxPacketDispatcher
    {
        static readonly Logger log = LogManager.GetLogger("SessionLayer");

        // 연결이 생기거나 끊어지면 연결 목록이 바뀐다
        // 이것은 멀티 쓰레드에서 문제가 생길 가능성이 있어보이니 락을 걸자
        Object writeLock = new Object();
        readonly IDictionary<int, Session> sessionIDSessionTable = new Dictionary<int, Session>();
        readonly IDictionary<string, int> transportIDSessionIDTable = new Dictionary<string, int>();

        readonly IDPool sessionIDPool = IDPool.MakeSessionID();

        public SessionLayer(ITransportLayer<string> transport)
        {
            transport.Received.Subscribe(datagram =>
            {
                log.Debug($"datagram recv: tranposrt_id={datagram.ID} size={datagram.Data.Length}");

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
            var sessionID = sessionIDPool.Next();
            var session = new Session(sessionID, transport);

            // register
            Debug.Assert(sessionIDSessionTable.ContainsKey(sessionID) == false);
            Debug.Assert(transportIDSessionIDTable.ContainsKey(transport.ID) == false);
            sessionIDSessionTable[sessionID] = session;
            transportIDSessionIDTable[transport.ID] = sessionID;

            return session;
        }

        // transport layer에서 연결이 닫혀서 session이 닫히는 경우
        public void CloseSessionPassive(Session s)
        {
            log.Info($"close session passive: id={s.ID}");

            // session을 닫아도 transport는 연결되어있다
            // transport는 나중에 직접 닫을수 있다
            s.State = SessionState.Disconnected;

            // 연결이 처리보다 빨리 닫히기때문에
            // 마지막 패킷의 전송은 실패할수있다
            RemoveSessionWithLock(s);
        }

        // session layer에서 연결을 닫고 trasnport layer를 닫는 경우
        public void CloseSessionActive(Session s)
        {
            log.Info($"close session active: id={s.ID}");
            s.State = SessionState.Disconnected;
        }

        public void RemoveSessionWithLock(Session s)
        {
            lock (writeLock) { RemoveSession(s); }
        }
        void RemoveSession(Session s)
        {
            Debug.Assert(sessionIDSessionTable.ContainsKey(s.ID) == true);
            Debug.Assert(transportIDSessionIDTable.ContainsKey(s.TransportID) == true);
            sessionIDPool.Release(s.ID);
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
