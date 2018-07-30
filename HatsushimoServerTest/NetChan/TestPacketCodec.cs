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
    }

    public class TestPacketSlicer
    {
        [Fact]
        public void TestEmpty()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { }));
            var slicer = new PacketSlicer(reader);

            Assert.False(slicer.MoveNext());
        }


        [Fact]
        public void TestSinglePacket()
        {
            var codec = new PacketCodec();
            var a = new ConnectPacket();
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var data = codec.Encode(a);

            var reader = new BinaryReader(new MemoryStream(data));
            var slicer = new PacketSlicer(reader);

            Assert.True(slicer.MoveNext());
            Assert.Equal((short)a.Type, slicer.CurrentType);
            ConnectPacket p;
            Assert.True(slicer.GetCurrent(out p));
            Assert.Equal(a, p);
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
            var slicer = new PacketSlicer(reader);

            // 1st packet
            {
                Assert.True(slicer.MoveNext());
                ConnectPacket p;
                Assert.True(slicer.GetCurrent(out p));
                Assert.Equal(a, p);
            }

            // 2nd packet
            {
                Assert.True(slicer.MoveNext());
                DisconnectPacket p;
                Assert.True(slicer.GetCurrent(out p));
                Assert.Equal(b, p);
            }

            // end of stream
            {
                Assert.False(slicer.MoveNext());
            }
        }
    }
}
