using System;

namespace Assets.Game
{
    class TimeUtils
    {
        static readonly DateTime BaseTime = DateTime.UtcNow;

        public static long NowTimestamp {
            get { return GetTimestamp(DateTime.UtcNow); }
        }
        public static int NowMillis {
            get { return GetRunningMillis(DateTime.UtcNow); }
        }

        public static long GetTimestamp(DateTime t)
        {
            var zero = new DateTime(1970, 1, 1, 0, 0, 0);
            var diff = t - zero;
            var ts = (long)(diff.TotalMilliseconds);
            return ts;
        }

        public static int GetRunningMillis(DateTime t)
        {
            var baseTs = GetTimestamp(BaseTime);
            var nowTs = NowTimestamp;
            int diff = (int)(nowTs - baseTs);
            return diff;
        }
    }
}
