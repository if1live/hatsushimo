using Hatsushimo.Packets;
using Xunit;

namespace HatsushimoTest.Packets
{

    public class TestLeaderboardPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new LeaderboardPacket()
            {
                Players = 10,
                Top = new Rank[]
                {
                    new Rank() { ID=1, Score=2, Ranking=3 },
                    new Rank() { ID=4, Score=5, Ranking=6 },
                },
            };
            var b = SerializeAndDeserialize(a);

            Assert.Equal(a, b);
        }

    }
}
