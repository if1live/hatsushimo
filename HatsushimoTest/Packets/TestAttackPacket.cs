using Hatsushimo.Packets;
using Xunit;

namespace HatsushimoTest.Packets
{
    public class TestAttackPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new AttackPacket(123);
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

    }
}
