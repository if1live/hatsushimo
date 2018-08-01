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
            var a = new MovePacket()
            {
                TargetPos = new Vector2(1, 2),
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

    }
}
