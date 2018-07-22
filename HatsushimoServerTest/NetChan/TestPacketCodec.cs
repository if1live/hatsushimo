using System;
using Xunit;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;

namespace HatsushimoServerTest.NetChan
{
    public class TestPacketCodec
    {
        [Fact]
        public void TestEncodeDecode()
        {
            var codec = new PacketCodec();
            codec.Register<WelcomePacket>();

            var a = new WelcomePacket()
            {
                UserID = 12,
                Version = 34,
            };
            var b = codec.Decode(codec.Encode(a));
            Assert.Equal(a, b);
        }
    }
}
