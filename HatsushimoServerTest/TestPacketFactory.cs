using System;
using Xunit;
using HatsushimoShared;

namespace HatsushimoServerTest
{
    public class TestPacketFactory
    {
        [Fact]
        public void TestSerializeDeserialize()
        {
            var factory = PacketFactory.Create();
            var a = new WelcomePacket()
            {
                UserID = 12,
                Version = 34,
            };
            var b = factory.Deserialize(factory.Serialize(a));
            Assert.Equal(a, b);
        }
    }
}
