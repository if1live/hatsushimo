using System.IO;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using Xunit;

namespace HatsushimoTest.NetChan
{
    public class TestPacketEnumerator
    {
        [Fact]
        public void TestEmpty()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { }));
            var iter = new PacketEnumerator(reader);

            Assert.False(iter.MoveNext());
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
            var iter = new PacketEnumerator(reader);

            Assert.True(iter.MoveNext());
            Assert.Equal((short)a.Type, iter.CurrentType);
            ConnectPacket p;
            Assert.True(iter.GetCurrent(out p));
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
            var iter = new PacketEnumerator(reader);

            // 1st packet
            {
                Assert.True(iter.MoveNext());
                ConnectPacket p;
                Assert.True(iter.GetCurrent(out p));
                Assert.Equal(a, p);
            }

            // 2nd packet
            {
                Assert.True(iter.MoveNext());
                DisconnectPacket p;
                Assert.True(iter.GetCurrent(out p));
                Assert.Equal(b, p);
            }

            // end of stream
            {
                Assert.False(iter.MoveNext());
            }
        }
    }
}
