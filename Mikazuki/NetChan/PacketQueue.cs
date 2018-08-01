using System.Collections.Concurrent;
using System.Collections.Generic;
using Hatsushimo.NetChan;

namespace Mikazuki.NetChan
{
    // session layer에서는 packet을 다룰거다
    // TODO channel 같은거 도입하면 더 간소화 할수 있을거같은데
    // 일단은 간단하게 큐로
    public class PacketQueue
    {
        readonly ConcurrentQueue<DataPair> queue = new ConcurrentQueue<DataPair>();

        public PacketQueue() { }

        public void Enqueue(Session s, byte[] d)
        {
            var pair = new DataPair()
            {
                Session = s,
                Data = d,
            };
            queue.Enqueue(pair);
        }

        public bool TryDequeue(out Session s, out byte[] d)
        {
            s = null;
            d = null;
            DataPair pair = new DataPair();
            var found = queue.TryDequeue(out pair);
            if (found)
            {
                s = pair.Session;
                d = pair.Data;
            }
            return found;
        }
    }

    struct DataPair
    {
        internal Session Session;
        internal byte[] Data;
    }
}
