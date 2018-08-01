using Hatsushimo.Packets;
using Xunit;

namespace HatsushimoTest.Packets
{

    public class TestWorldJoinPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new WorldJoinPacket()
            {
                WorldID = "foo",
                Nickname = "test",
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestWorldJoinResultPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new WorldJoinResultPacket()
            {
                PlayerID = 123,
                WorldID = "foo",
                Nickname = "test",
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestWorldLeaveResultPacket : PacketTestCase
    {

        [Fact]
        public void TestSerde()
        {
            var a = new WorldLeaveResultPacket()
            {
                PlayerID = 123,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestWorldLeavePacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new WorldLeavePacket() { };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }
}
