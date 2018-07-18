using System;

namespace Assets.Game
{
    class TimeUtils
    {
        public static long GetTimestamp()
        {
            var now = DateTime.UtcNow;
            var zero = new DateTime(1970, 1, 1, 0, 0, 0);
            var diff = now - zero;
            var ts = (long)(diff.TotalMilliseconds);
            return ts;
        }
    }
}
