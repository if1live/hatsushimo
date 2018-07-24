using System.Collections.Concurrent;
using System.Collections.Generic;
using Hatsushimo.NetChan;

namespace HatsushimoServer.NetChan
{
    // session layer에서는 packet을 다룰거다
    // TODO channel 같은거 도입하면 더 간소화 할수 있을거같은데
    // 일단은 간단하게 큐로
    public class PacketQueue
    {
        readonly ConcurrentQueue<PacketPair> queue = new ConcurrentQueue<PacketPair>();

        public PacketQueue() { }

        public void Enqueue(Session s, IPacket p)
        {
            var pair = new PacketPair()
            {
                Session = s,
                Packet = p,
            };
            queue.Enqueue(pair);
        }

        public bool TryDequeue(out Session s, out IPacket p)
        {
            s = null;
            p = null;
            PacketPair pair = new PacketPair();
            var found = queue.TryDequeue(out pair);
            if (found)
            {
                s = pair.Session;
                p = pair.Packet;
            }
            return found;
        }
    }

    struct PacketPair
    {
        internal Session Session;
        internal IPacket Packet;
    }
}
