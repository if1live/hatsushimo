using Hatsushimo.NetChan;
using Xunit;

namespace HatsushimoTest.NetChan
{
    public class TestBandwidthChecker
    {
        [Fact]
        public void TestGetBytesPerSeconds()
        {
            var checker = new BandwidthChecker();
            var now = 10000;

            // 시간순서로 정렬
            // 오래된 패킷이 가장 먼저 들어간다
            checker.AddSent(1, now - 1001);
            checker.AddSent(2, now - 1000);
            checker.AddSent(4, now - 999);
            checker.AddSent(8, now - 1);
            checker.AddSent(16, now);
            checker.AddSent(32, now + 1);

            Assert.Equal(4 + 8 + 16, checker.GetSentBytesPerSecond(now));
            Assert.Equal(2 + 4 + 8, checker.GetSentBytesPerSecond(now - 1));
            Assert.Equal(1 + 2 + 4, checker.GetSentBytesPerSecond(now - 20));
        }

        [Fact]
        public void TestFlush()
        {
            var checker = new BandwidthChecker();
            var now = 10000;

            checker.AddSent(1, now - 2);
            checker.AddSent(2, now);
            checker.AddSent(4, now + 2);

            Assert.Equal(2, checker.GetSentPacketsPerSecond(now));

            checker.Flush(now - 1);
            Assert.Equal(1, checker.GetSentPacketsPerSecond(now));

            checker.Flush(now + 999);
            Assert.Equal(0, checker.GetSentPacketsPerSecond(now));
        }
    }
}
