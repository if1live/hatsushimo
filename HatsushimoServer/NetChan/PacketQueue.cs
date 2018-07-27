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
        readonly ConcurrentQueue<DataPair> queue = new ConcurrentQueue<DataPair>();

        public PacketQueue() { }

        public void Enqueue(Session s, IPacket p)
        {
            var pair = new DataPair()
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
            DataPair pair = new DataPair();
            var found = queue.TryDequeue(out pair);
            if (found)
            {
                s = pair.Session;
                p = pair.Packet;
            }
            return found;
        }
    }

    struct DataPair
    {
        internal Session Session;
        internal IPacket Packet;
    }
}
