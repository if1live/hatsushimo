using Hatsushimo.Packets;
using Xunit;

namespace HatsushimoTest.Packets
{

    public class TestLeaderboardPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var players = 10;
            var top = new Rank[]
            {
                new Rank() { ID=1, Score=2, Ranking=3 },
                new Rank() { ID=4, Score=5, Ranking=6 },
            };
            var a = new LeaderboardPacket(players, top);
            var b = SerializeAndDeserialize(a);

            Assert.Equal(a, b);
        }

    }
}
