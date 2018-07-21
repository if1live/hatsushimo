using System;
using Xunit;
using HatsushimoShared;

namespace HatsushimoServerTest
{
    public class TestPacket
    {
        [Fact]
        public void TestPingPacket()
        {
            var a = new PingPacket()
            {
                millis = 1234,
            };
            var b = new PingPacket();
            b.Deserialize(a.Serialize());

            Assert.Equal(a, b);
        }

        [Fact]
        public void TestWelcomePacket()
        {
            var a = new WelcomePacket(){
                version = 1234,
            };
            var b = new WelcomePacket();
            b.Deserialize(a.Serialize());

            Assert.Equal(a, b);
        }
    }
}
