using System.Numerics;
using Hatsushimo.Packets;
using Xunit;

namespace HatsushimoTest.Packets
{
    public class TestMovePacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var targetPos = new Vector2(1, 2);
            var a = new MovePacket(targetPos);
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

    }
}
