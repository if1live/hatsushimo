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
            var a = new WelcomePacket()
            {
                UserID = 12,
                Version = 34,
            };
            var b = new WelcomePacket();
            b.Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestConnectPacket()
        {
            var a = new ConnectPacket() {};
            var b = new ConnectPacket();
            b.Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestDisconnectPacket()
        {
            var a = new DisconnectPacket() {};
            var b = new DisconnectPacket();
            b.Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }
    }
}
