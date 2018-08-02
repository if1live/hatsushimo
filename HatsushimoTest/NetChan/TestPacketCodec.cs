using System;
using Xunit;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System.IO;
using Hatsushimo.Utils;

namespace HatsushimoTest.NetChan
{
    public class TestPacketCodec
    {
        [Fact]
        public void TestEncodeDecode()
        {
            var codec = new PacketCodec();

            var a = new WelcomePacket(12, 34);
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var data = codec.Encode(a);

            var reader = new BinaryReader(new MemoryStream(data));
            var type = codec.ReadPacketType(reader);
            WelcomePacket b;
            var ok = codec.TryDecode(type, reader, out b);
            Assert.True(ok);
            Assert.Equal(a, b);
        }
    }
}
