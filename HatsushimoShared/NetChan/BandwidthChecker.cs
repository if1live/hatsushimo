using System.Collections.Generic;
using C5;
using System.Linq;

namespace Hatsushimo.NetChan
{
    class BandwidthList
    {
        struct Pair
        {
            internal long ts;
            internal int bytes;

            internal class Comparer : IComparer<Pair>
            {
                public int Compare(Pair x, Pair y)
                {
                    var diff = x.ts - y.ts;
                    if (diff > 0) { return 1; }
                    else if (diff < 0) { return -1; }
                    return 0;
                }
            }
        }

        readonly ISorted<Pair> list = new SortedArray<Pair>(new Pair.Comparer());

        internal void Add(int bytes, long ts)
        {
            var p = new Pair()
            {
                ts = ts,
                bytes = bytes,
            };
            list.Add(p);
        }

        internal void Flush(long ts)
        {
            while (!list.IsEmpty)
            {
                var first = list.FindMin();
                if (first.ts < ts)
                {
                    list.DeleteMin();
                }
                else
                {
                    break;
                }
            }
        }

        internal int GetBytes(long ts, int millis)
        {
            var bottom = new Pair() { ts = ts, bytes = 0 };
            var top = new Pair() { ts = ts + millis, bytes = 0 };
            var iter = list.RangeFromTo(bottom, top);
            return iter.Select(p => p.bytes).Sum();
        }

        internal int GetPackets(long ts, int millis)
        {
            var bottom = new Pair() { ts = ts, bytes = 0 };
            var top = new Pair() { ts = ts + millis, bytes = 0 };
            var iter = list.RangeFromTo(bottom, top);
            var count = 0;
            foreach (var x in iter) { count += 1; }
            return count;
        }
    }

    public class BandwidthChecker
    {
        readonly BandwidthList sent = new BandwidthList();
        readonly BandwidthList received = new BandwidthList();

        public void AddSent(int bytes, long ts)
        {
            sent.Add(bytes, ts);
        }

        public void AddReceived(int bytes, long ts)
        {
            received.Add(bytes, ts);
        }

        public void Flush(long ts)
        {
            sent.Flush(ts);
            received.Flush(ts);
        }

        public int GetSentBytesPerSecond(long ts)
        {
            return sent.GetBytes(ts, 1000);
        }
        public int GetSentPacketsPerSecond(long ts)
        {
            return sent.GetPackets(ts, 1000);
        }

        public int GetReceivedBytesPerSeconds(long ts)
        {
            return received.GetBytes(ts, 1000);
        }
        public int GetReceivedPacketsPerSeconds(long ts)
        {
            return received.GetPackets(ts, 1000);
        }
    }
}
