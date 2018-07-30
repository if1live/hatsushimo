using Hatsushimo.NetChan;
using Xunit;

namespace HatsushimoServerTest.NetChan
{
    public class TestBandwidthChecker
    {
        [Fact]
        public void TestGetBytesPerSeconds()
        {
            var checker = new BandwidthChecker();
            var now = 10000;

            checker.AddSent(1, now);
            checker.AddSent(2, now + 999);
            checker.AddSent(4, now + 1000);
            checker.AddSent(8, now + 1010);

            Assert.Equal(1 + 2, checker.GetSentBytesPerSecond(now));
            Assert.Equal(2 + 4, checker.GetSentBytesPerSecond(now + 1));
            Assert.Equal(2 + 4 + 8, checker.GetSentBytesPerSecond(now + 20));
        }

        [Fact]
        public void TestFlush()
        {
            var checker = new BandwidthChecker();
            var now = 10000;

            checker.AddSent(1, now);
            checker.AddSent(2, now + 800);
            checker.AddSent(4, now + 900);
            checker.AddSent(8, now + 999);

            Assert.Equal(4, checker.GetSentPacketsPerSecond(now));

            checker.Flush(now + 1);
            Assert.Equal(3, checker.GetSentPacketsPerSecond(now));

            checker.Flush(now + 999);
            Assert.Equal(1, checker.GetSentPacketsPerSecond(now));
        }
    }
}
