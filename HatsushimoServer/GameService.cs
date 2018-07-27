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
using SQLite;
using System.IO;
using HatsushimoServer.Models;

namespace HatsushimoServer
{
    class GameService
    {
        public static readonly GameService Instance;

        readonly SQLiteAsyncConnection conn;

        static GameService()
        {
            Instance = new GameService();
        }

        public GameService()
        {
            var dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "hatsushimo.db");
            conn = new SQLiteAsyncConnection(dbpath);
            PrepareDatabase(conn);

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

        async void PrepareDatabase(SQLiteAsyncConnection conn)
        {
            await conn.CreateTableAsync<User>();
            Console.WriteLine("create user table");
        }

        void HandlePing(Session session, PingPacket p)
        {
            // Console.WriteLine($"ping packet received : {p.millis}");
            session.Send(p);
        }

        void HandleHeartbeat(Session session, HeartbeatPacket p)
        {
            session.RefreshHeartbeat();
            Console.WriteLine($"heartbeat: id={session.ID}");
        }

        void HandleConnect(Session session, ConnectPacket p)
        {
            var welcome = new WelcomePacket()
            {
                UserID = session.ID,
                Version = Config.Version,
            };
            Console.WriteLine($"connected: welcome id={session.ID}");
            session.Send(welcome);
        }

        void HandleDisconnect(Session session, DisconnectPacket p)
        {
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

            Console.WriteLine($"disconnected: id={session.ID}");
            NetworkStack.Session.RemoveSessionWithLock(session);
        }

        async void HandleSignUp(Session session, SignUpPacket packet)
        {
            var query = conn.Table<User>().Where(u => u.Uuid == packet.Uuid);
            var prevUser = await query.FirstOrDefaultAsync();
            if (prevUser == null)
            {
                var user = new User()
                {
                    Uuid = packet.Uuid,
                };
                await conn.InsertAsync(user);
                Console.WriteLine($"create user: uuid={packet.Uuid}");
            }

            Console.WriteLine($"sign up: uuid={packet.Uuid}");
            var result = new SignUpResultPacket() { Success = true };
            session.Send(result);
        }

        async void HandleAuthentication(Session session, AuthenticationPacket packet)
        {
            var query = conn.Table<User>().Where(u => u.Uuid == packet.Uuid);
            var user = await query.FirstOrDefaultAsync();
            if (user == null)
            {
                Console.WriteLine($"authentication: uuid={packet.Uuid} not found");
                var notFound = new AuthenticationResultPacket() { Success = false };
                session.Send(notFound);
                return;
            }

            session.UserID = user.ID;
            Console.WriteLine($"authentication: uuid={packet.Uuid} user_id={session.UserID}");
            var result = new AuthenticationResultPacket() { Success = true };
            session.Send(result);
        }

        void HandleWorldJoin(Session session, ReceivedPacket<WorldJoinPacket> received)
        {
            if(session.UserID < 0) { return; }

            var p = received.Packet;
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(p.WorldID);
            world.EnqueueRecv(session, received.Data);
        }

        void HandleWorldLeave(Session session, ReceivedPacket<WorldLeavePacket> received)
        {
            if(session.UserID < 0) { return; }

            var p = received.Packet;
            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(session.WorldID);
            world.EnqueueRecv(session, received.Data);
        }

        void EnqueueWorldPacket<TPacket>(ReceivedPacket<TPacket> packet)
        where TPacket : IPacket, new()
        {
            if(packet.Session.UserID < 0) { return; }

            var worlds = InstanceWorldManager.Instance;
            var world = worlds.Get(packet.Session.WorldID);
            world.EnqueueRecv(packet.Session, packet.Data);
        }
    }
}
