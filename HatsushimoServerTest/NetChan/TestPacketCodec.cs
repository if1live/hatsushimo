using System;
using Xunit;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System.IO;
using Hatsushimo.Utils;

namespace HatsushimoServerTest.NetChan
{
    public class TestPacketCodec
    {
        [Fact]
        public void TestEncodeDecode()
        {
            var codec = new PacketCodec();

            var a = new WelcomePacket()
            {
                UserID = 12,
                Version = 34,
            };
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

        [Fact]
        public void TestMultiplePacket()
        {
            var codec = new PacketCodec();

            var a = new ConnectPacket();
            var b = new DisconnectPacket();

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var data = ByteJoin.Combine(codec.Encode(a), codec.Encode(b));

            var reader = new BinaryReader(new MemoryStream(data));

            // 1st packet
            {
                var type = codec.ReadPacketType(reader);
                ConnectPacket p;
                var ok = codec.TryDecode(type, reader, out p);
                Assert.True(ok);
                Assert.Equal(a, p);
            }

            // 2nd packet
            {
                var type = codec.ReadPacketType(reader);
                DisconnectPacket p;
                var ok = codec.TryDecode(type, reader, out p);
                Assert.True(ok);
                Assert.Equal(b, p);
            }

            // end of stream
            {
                var type = codec.ReadPacketType(reader);
                Assert.Equal((short)PacketType.Invalid, type);
            }
        }
    }
}
