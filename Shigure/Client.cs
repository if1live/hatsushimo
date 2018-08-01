using System;
using WebSocketSharp;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.NetChan;
using System.IO;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shigure
{
    public class Client
    {
        readonly PacketDispatcher<int> dispatcher = new PacketDispatcher<int>();
        readonly PacketCodec codec = new PacketCodec();

        int ping = 1234;
        string botUuid = "bot-uuid";
        string worldID = "default";
        string nickname = "bot-nick";

        int playerID = 0;

        WebSocket ws;

        public Client()
        {
            dispatcher.Ping += (p) => queue.Enqueue(p.Packet);
            dispatcher.Welcome += (p) => queue.Enqueue(p.Packet);
            dispatcher.SignUpResult += (p) => queue.Enqueue(p.Packet);
            dispatcher.AuthenticationResult += (p) => queue.Enqueue(p.Packet);
            dispatcher.WorldJoinResult += (p) => queue.Enqueue(p.Packet);
            dispatcher.WorldLeaveResult += (p) => queue.Enqueue(p.Packet);
            dispatcher.PlayerReady += (p) => queue.Enqueue(p.Packet);
            dispatcher.Disconnect += (p) => queue.Enqueue(p.Packet);
            dispatcher.AttackNotify += (p) => queue.Enqueue(p.Packet);
            dispatcher.MoveNotify += (p) => queue.Enqueue(p.Packet);
            dispatcher.Leaderboard += (p) => queue.Enqueue(p.Packet);
        }

        void HandleWelcome(WelcomePacket p)
        {
            Debug.Assert(p.Version == Config.Version);

            playerID = p.UserID;
            Debug.Assert(playerID > 0);
        }

        void HandlePing(PingPacket p)
        {
            Debug.Assert(p.millis == ping);
        }

        void HandleSignUp(SignUpResultPacket p)
        {
            Debug.Assert(0 == p.ResultCode);
        }

        void HandleAuthentication(AuthenticationResultPacket p)
        {
            Debug.Assert(0 == p.ResultCode);
        }

        void HandleWorldJoin(WorldJoinResultPacket p)
        {
            Debug.Assert(nickname == p.Nickname);
            Debug.Assert(worldID == p.WorldID);
        }

        void HandleWorldLeave(WorldLeaveResultPacket p)
        {
        }

        void HandlePlayerReady(PlayerReadyPacket p)
        {
        }

        void HandleDisconnect(DisconnectPacket p)
        {
        }

        void HandleAttackNotify(AttackNotifyPacket p)
        {

        }

        void HandleMoveNotify(MoveNotifyPacket p)
        {
            var my = p.list.Where(el => el.ID == playerID).ToArray();
            Debug.Assert(my.Length == 1);
        }

        void HandleLeaderboard(LeaderboardPacket p)
        {
            Debug.Assert(p.Top.Length >= 0);
            Debug.Assert(p.Players >= 0);
        }

        readonly Queue<IPacket> queue = new Queue<IPacket>();

        void Shutdown()
        {
            ws.Close();
        }

        void SendImmediate<T>(T p) where T : IPacket
        {
            var data = codec.Encode(p);
            ws.Send(data);
        }

        async Task<bool> RunTest()
        {
            SendImmediate(new ConnectPacket());

            var welcome = await Recv<WelcomePacket>();
            HandleWelcome(welcome);

            SendImmediate(new PingPacket() { millis = this.ping });
            var ping = await Recv<PingPacket>();
            HandlePing(ping);

            SendImmediate(new SignUpPacket() { Uuid = botUuid });
            var signup = await Recv<SignUpResultPacket>();
            HandleSignUp(signup);

            SendImmediate(new AuthenticationPacket() { Uuid = botUuid });
            var auth = await Recv<AuthenticationResultPacket>();
            HandleAuthentication(auth);

            SendImmediate(new WorldJoinPacket()
            {
                WorldID = worldID,
                Nickname = nickname,
            });
            var join = await Recv<WorldJoinResultPacket>();
            HandleWorldJoin(join);

            SendImmediate(new PlayerReadyPacket());
            var ready = await Recv<PlayerReadyPacket>();
            HandlePlayerReady(ready);

            // TODO game loop 테스트 구현?
            await RunLeaderboardTest();
            await RunMoveTest();

            SendImmediate(new WorldLeavePacket());
            var leave = await Recv<WorldLeaveResultPacket>();
            HandleWorldLeave(leave);

            SendImmediate(new DisconnectPacket());
            var disconnect = await Recv<DisconnectPacket>();
            HandleDisconnect(disconnect);


            SendImmediate(new DisconnectPacket());
            Shutdown();

            Console.WriteLine("test completed");
            return true;
        }

        async Task<int> RunLeaderboardTest()
        {
            // 아무것도 안해도 리더보드 정보를 받는다
            for (var i = 0; i < 2; i++)
            {
                var leaderboard = await Recv<LeaderboardPacket>();
                HandleLeaderboard(leaderboard);
            }
            return 0;
        }

        async Task<int> RunMoveTest()
        {
            var targetPos = new System.Numerics.Vector2(1, 2);

            {
                // 아무것도 안해도 이동패킷이 온다
                var moveNotify = await Recv<MoveNotifyPacket>();
                var my = moveNotify.list.Where(el => el.ID == playerID).ToArray();
                Debug.Assert(my.Length == 1);
            }

            // 이동 요청 보내기전에 받은 패킷은 테스트에 필요없다
            FlushQueue<MoveNotifyPacket>();

            // 이동 요청
            SendImmediate(new MovePacket()
            {
                TargetPos = targetPos,
            });

            // 요청한 좌표를 받았는지 확인하기
            // send 요청이 들어가기전에 이동 패킷을 받았을지모른다
            // 받은 패킷 몇개에 대해서 검증하기
            var tryCount = 5;
            for (var i = 0; i < tryCount; i++)
            {
                // 요청된 좌표 정보를 돌려받는다
                var moveNotify = await Recv<MoveNotifyPacket>();
                var my = moveNotify.list.Where(el => el.ID == playerID).ToArray();
                Debug.Assert(my.Length == 1);
                var myNotify = my[0];
                // TODO 최초의 targetPos가 0,0이어도 상관없나?
                if (myNotify.TargetPos == System.Numerics.Vector2.Zero)
                {
                    continue;
                }
                Debug.Assert(myNotify.TargetPos == targetPos);
            }
            return 0;
        }

        void FlushQueue<TPacket>() where TPacket : IPacket, new()
        {
            var unusedPacketList = new List<IPacket>();
            var filterType = (new TPacket()).Type;
            while (queue.Count > 0)
            {
                var packet = queue.Dequeue();
                var t = packet.Type;
                if (t != filterType)
                {
                    unusedPacketList.Add(packet);
                }
            }

            foreach (var x in unusedPacketList)
            {
                queue.Enqueue(x);
            }
        }

        async Task<TPacket> Recv<TPacket>() where TPacket : IPacket, new()
        {
            var unusedPacketList = new List<IPacket>();

            var dummy = new TPacket();
            var expectedType = (PacketType)dummy.Type;
            var tryCount = 1000;
            for (int i = 0; i < tryCount; i++)
            {
                while (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    var t = (PacketType)packet.Type;
                    if (t == expectedType)
                    {
                        foreach (var x in unusedPacketList)
                        {
                            queue.Enqueue(x);
                        }

                        return (TPacket)packet;
                    }
                    else
                    {
                        unusedPacketList.Add(packet);
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            Debug.Assert(false, $"timeout : expected packet type: {expectedType}");
            return dummy;
        }

        public void Run(string[] args)
        {
            ws = new WebSocket($"ws://127.0.0.1:{Config.ServerPort}/game");
            ws.OnMessage += (sender, e) =>
            {
                dispatcher.Dispatch(e.RawData, 0);
            };
            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine("on close");
            };
            ws.OnError += (sender, e) =>
            {
                Console.WriteLine("on error");
            };
            ws.Connect();

            var task = RunTest();
            task.Wait();
        }
    }
}
