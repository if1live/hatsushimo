using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;
using HatsushimoServer.NetChan;
using WebSocketSharp.Server;
using System.IO;
using HatsushimoServer.Models;
using NLog;

namespace HatsushimoServer
{
    class GameService
    {
        public static readonly GameService Instance;

        readonly IDBConnection conn;
        static readonly Logger log = LogManager.GetLogger("GameService");

        static GameService()
        {
            Instance = new GameService();
        }

        public GameService()
        {
            var connfactory = new DBConnectionFactory();
            conn = connfactory.Connect();
            conn.CreateTable();

            var worlds = InstanceWorldManager.Instance;
            var defaultWorld = worlds.Get(InstanceWorldManager.DefaultID);

            var sessionLayer = NetworkStack.Session;

            sessionLayer.Connect.Received.Subscribe(d => HandleConnect(d.Session, d.Packet));
            sessionLayer.Disconnect.Received.Subscribe(d => HandleDisconnect(d.Session, d.Packet));
            sessionLayer.Ping.Received.Subscribe(d => HandlePing(d.Session, d.Packet));
            sessionLayer.Heartbeat.Received.Subscribe(d => HandleHeartbeat(d.Session, d.Packet));

            sessionLayer.SignUp.Received.Subscribe(d => HandleSignUp(d.Session, d.Packet));
            sessionLayer.Authentication.Received.Subscribe(d => HandleAuthentication(d.Session, d.Packet));

            sessionLayer.WorldJoin.Received.Subscribe(d => HandleWorldJoin(d.Session, d));
            sessionLayer.WorldLeave.Received.Subscribe(d => HandleWorldLeave(d.Session, d));

            sessionLayer.InputCommand.Received.Subscribe(d => EnqueueWorldPacket(d));
            sessionLayer.InputMove.Received.Subscribe(d => EnqueueWorldPacket(d));
            sessionLayer.PlayerReady.Received.Subscribe(d => EnqueueWorldPacket(d));
        }

        void HandlePing(Session session, PingPacket p)
        {
            log.Debug($"ping packet received : {p.millis}");
            session.SendImmediate(p);
        }

        void HandleHeartbeat(Session session, HeartbeatPacket p)
        {
            session.RefreshHeartbeat();
            log.Info($"heartbeat: id={session.ID}");
        }

        void HandleConnect(Session session, ConnectPacket p)
        {
            var welcome = new WelcomePacket()
            {
                UserID = session.ID,
                Version = Config.Version,
            };
            log.Info($"connected: welcome id={session.ID}");
            session.SendLazy(welcome);
        }

        void HandleDisconnect(Session session, DisconnectPacket p)
        {
            if (session.State == SessionState.Disconnected)
            {
                log.Info($"already disconnected: id={session.ID}");
                return;
            }

            // 연결 종료는 소켓이 끊어질떄도 있고
            // 유저가 직접 종료시키는 경우도 있다
            // disconnect를 여러번 호출해도 꺠지지 않도록 하자
            // 연결 끊은것을 연결 종료 패킷처럼 다루면
            // 상위 레이어에서의 처리가 간단해진다
            if (session.WorldID != null)
            {
                var leave = new WorldLeavePacket();
                var codec = new PacketCodec();
                var bytes = codec.Encode(leave);
                var pair = new ReceivedPacket<WorldLeavePacket>(session, bytes);
                HandleWorldLeave(session, pair);
            }

            log.Info($"disconnect: id={session.ID}");
            var disconnect = new DisconnectPacket();
            session.SendLazy(disconnect);

            // 연결 끊어도 된다는 표시 해두기. 언제 끊어도 상관 없어야한다
            NetworkStack.Session.CloseSessionActive(session);
        }

        async void HandleSignUp(Session session, SignUpPacket packet)
        {
            var user = await conn.GetOrCreateUser(packet.Uuid);
            var result = new SignUpResultPacket() { ResultCode = 0 };
            session.SendLazy(result);
        }

        async void HandleAuthentication(Session session, AuthenticationPacket packet)
        {
            var option = await conn.GetUser(packet.Uuid);
            option.MatchSome(user =>
            {
                session.UserID = user.ID;
                log.Info($"authentication: uuid={packet.Uuid} user_id={session.UserID}");
                var result = new AuthenticationResultPacket() { ResultCode = 0 };
                session.SendLazy(result);
            });
            option.MatchNone(() =>
            {
                log.Info($"authentication: uuid={packet.Uuid} not found");
                var notFound = new AuthenticationResultPacket() { ResultCode = -1 };
                session.SendLazy(notFound);
            });
        }

        void HandleWorldJoin(Session session, ReceivedPacket<WorldJoinPacket> received)
        {
            if (session.UserID < 0) { return; }

            var p = received.Packet;
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(p.WorldID);
            world.EnqueueRecv(session, received.Data);
        }

        void HandleWorldLeave(Session session, ReceivedPacket<WorldLeavePacket> received)
        {
            if (session.UserID < 0) { return; }

            var p = received.Packet;
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(session.WorldID);
            world.EnqueueRecv(session, received.Data);
        }

        void EnqueueWorldPacket<TPacket>(ReceivedPacket<TPacket> packet)
        where TPacket : IPacket, new()
        {
            if (packet.Session.UserID < 0) { return; }

            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(packet.Session.WorldID);
            world.EnqueueRecv(packet.Session, packet.Data);
        }
    }
}
