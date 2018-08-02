using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Optional;
using WebSocketSharp;
using Optional.Unsafe;
using System.Collections.Concurrent;

namespace Shigure
{
    public class Connection
    {
        static readonly NLog.Logger log = LogManager.GetLogger("Connection");
        readonly WebSocket ws;
        readonly PacketCodec codec = new PacketCodec();
        readonly PacketDispatcher<int> dispatcher = new PacketDispatcher<int>();

        // send-receive 구조로 돌아가는 패킷을 받는 큐
        // 패킷 타입별로 각각 큐를 만들기 귀찮아서 섞어서 넣었다
        // 이름은 queue지만 실제로는 list를 쓰는 이유
        // 큐처럼 FIFO가 유지되면서 원하는 타입만 꺼내고 싶어서
        readonly List<IPacket> commonQueue = new List<IPacket>();

        readonly SizedConcurrentQueue<MoveNotifyPacket> moveQueue = new SizedConcurrentQueue<MoveNotifyPacket>(10);
        readonly SizedConcurrentQueue<AttackNotifyPacket> attackQueue = new SizedConcurrentQueue<AttackNotifyPacket>(10);
        readonly SizedConcurrentQueue<LeaderboardPacket> leaderboardQueue = new SizedConcurrentQueue<LeaderboardPacket>(10);

        readonly object lockobj = new object();

        public Connection(WebSocket sock)
        {
            ws = sock;

            dispatcher.Ping += (p) => commonQueue.Add(p.Packet);
            dispatcher.Welcome += (p) => commonQueue.Add(p.Packet);
            dispatcher.SignUpResult += (p) => commonQueue.Add(p.Packet);
            dispatcher.AuthenticationResult += (p) => commonQueue.Add(p.Packet);
            dispatcher.WorldJoinResult += (p) => commonQueue.Add(p.Packet);
            dispatcher.WorldLeaveResult += (p) => commonQueue.Add(p.Packet);
            dispatcher.PlayerReady += (p) => commonQueue.Add(p.Packet);
            dispatcher.Disconnect += (p) => commonQueue.Add(p.Packet);

            dispatcher.ReplicationAll += (p) => commonQueue.Add(p.Packet);
            dispatcher.ReplicationBulkRemove += (p) => commonQueue.Add(p.Packet);
            dispatcher.ReplicationRemove += (p) => commonQueue.Add(p.Packet);

            dispatcher.CreateFood += (p) => commonQueue.Add(p.Packet);
            dispatcher.CreatePlayer += (p) => commonQueue.Add(p.Packet);
            dispatcher.CreateProjectile += (p) => commonQueue.Add(p.Packet);

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
            lock (lockobj)
            {
                dispatcher.Dispatch(data, 0);
            }
        }

        public async Task<Option<MoveNotifyPacket>> RecvMove()
        {
            return await moveQueue.Dequeue(1000, TimeSpan.FromMilliseconds(100));
        }

        public async Task<Option<AttackNotifyPacket>> RecvAttack()
        {
            return await attackQueue.Dequeue(1000, TimeSpan.FromMilliseconds(100));
        }

        public async Task<Option<LeaderboardPacket>> RecvLeaderboard()
        {
            return await leaderboardQueue.Dequeue(1000, TimeSpan.FromMilliseconds(100));
        }

        public void ClearMove() { moveQueue.Clear(); }
        public void ClearAttack() { attackQueue.Clear(); }
        public void ClearLeaderboard() { leaderboardQueue.Clear(); }

        void FlushQueue<T>(List<T> queue)
        {
            lock (lockobj) { queue.Clear(); }
        }

        public async Task<Option<TPacket>> Recv<TPacket>() where TPacket : IPacket, new()
        {
            var tryCount = 1000;
            for (int i = 0; i < tryCount; i++)
            {
                var found = TryRecv<TPacket>();
                if (found.HasValue) { return found; }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            var dummy = new TPacket();
            var expectedType = dummy.Type;
            log.Warn($"timeout : expected packet type: {expectedType}");
            return Option.None<TPacket>();
        }

        public Option<TPacket> TryRecv<TPacket>() where TPacket : IPacket, new()
        {
            lock (lockobj) { return TryRecvNoLock<TPacket>(); }
        }

        Option<TPacket> TryRecvNoLock<TPacket>() where TPacket : IPacket, new()
        {
            var queue = commonQueue;
            var dummy = new TPacket();
            var expectedType = dummy.Type;

            for (var i = 0; i < queue.Count; i++)
            {
                var packet = queue[i];
                if (packet.Type == expectedType)
                {
                    queue.RemoveAt(i);
                    return Option.Some((TPacket)packet);
                }
            }
            return Option.None<TPacket>();
        }

        public async Task<TRecv> SendRecv<TSend, TRecv>(TSend send)
        where TSend : IPacket
        where TRecv : IPacket, new()
        {
            Send(send);
            var p = await Recv<TRecv>();
            return p.ValueOrFailure();
        }
    }
}
