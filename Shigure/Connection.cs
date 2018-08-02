using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Optional;
using WebSocketSharp;

namespace Shigure
{
    public class Connection
    {
        readonly WebSocket ws;
        readonly PacketCodec codec = new PacketCodec();
        readonly PacketDispatcher<int> dispatcher = new PacketDispatcher<int>();

        // send-receive 구조로 돌아가는 패킷을 받는 큐
        readonly Queue<IPacket> commonQueue = new Queue<IPacket>();

        // notify 계열 패킷 받는 큐. 서버에서 패킷을 계속 보낸다
        readonly Queue<MoveNotifyPacket> moveQueue = new Queue<MoveNotifyPacket>();
        readonly Queue<AttackNotifyPacket> attackQueue = new Queue<AttackNotifyPacket>();
        readonly Queue<LeaderboardPacket> leaderboardQueue = new Queue<LeaderboardPacket>();

        public Connection(WebSocket sock)
        {
            ws = sock;

            dispatcher.Ping += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.Welcome += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.SignUpResult += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.AuthenticationResult += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.WorldJoinResult += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.WorldLeaveResult += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.PlayerReady += (p) => commonQueue.Enqueue(p.Packet);
            dispatcher.Disconnect += (p) => commonQueue.Enqueue(p.Packet);

            dispatcher.AttackNotify += (p) => attackQueue.Enqueue(p.Packet);
            dispatcher.MoveNotify += (p) => moveQueue.Enqueue(p.Packet);
            dispatcher.Leaderboard += (p) => leaderboardQueue.Enqueue(p.Packet);
        }

        public void Shutdown()
        {
            ws.Close();
        }

        public void Send<T>(T p) where T : IPacket
        {
            var data = codec.Encode(p);
            ws.Send(data);
        }

        public void PushReceivedData(byte[] data)
        {
            dispatcher.Dispatch(data, 0);
        }

        public async Task<Option<MoveNotifyPacket>> RecvMove()
        {
            return await RecvFromQueue(moveQueue);
        }

        public async Task<Option<AttackNotifyPacket>> RecvAttack()
        {
            return await RecvFromQueue(attackQueue);
        }

        public async Task<Option<LeaderboardPacket>> RecvLeaderboard()
        {
            return await RecvFromQueue(leaderboardQueue);
        }

        public void FlushMove() { moveQueue.Clear(); }
        public void FlushAttack() { attackQueue.Clear(); }
        public void FlushLeaderboard() { leaderboardQueue.Clear(); }

        async Task<Option<T>> RecvFromQueue<T>(Queue<T> queue) where T : IPacket
        {
            var tryCount = 1000;
            for (var i = 0; i < tryCount; i++)
            {
                if (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    return Option.Some(packet);
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            Debug.Assert(false, $"timeout : packet type={typeof(T).Name}");
            return Option.None<T>();
        }

        public async Task<Option<TPacket>> Recv<TPacket>() where TPacket : IPacket, new()
        {
            var unusedPacketList = new List<IPacket>();

            var dummy = new TPacket();
            var expectedType = (PacketType)dummy.Type;
            var tryCount = 1000;
            for (int i = 0; i < tryCount; i++)
            {
                while (commonQueue.Count > 0)
                {
                    var packet = commonQueue.Dequeue();
                    var t = (PacketType)packet.Type;
                    if (t == expectedType)
                    {
                        foreach (var x in unusedPacketList)
                        {
                            commonQueue.Enqueue(x);
                        }

                        return Option.Some((TPacket)packet);
                    }
                    else
                    {
                        unusedPacketList.Add(packet);
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            Debug.Assert(false, $"timeout : expected packet type: {expectedType}");
            return Option.None<TPacket>();
        }
    }
}
