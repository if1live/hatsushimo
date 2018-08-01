using System.Linq;
using Xunit;
using Mikazuki;

namespace MikazukiTest
{
    public class TestLeaderboard
    {
        struct MockPlayer : IRankable
        {
            public int ID => _id;
            public int Score => _score;

            public int _id;
            public int _score;
        }

        static MockPlayer MakePlayer(int id, int score)
        {
            return new MockPlayer()
            {
                _id = id,
                _score = score,
            };
        }


        static readonly MockPlayer p1 = MakePlayer(1, 10);
        static readonly MockPlayer p2 = MakePlayer(2, 20);
        static readonly MockPlayer p3 = MakePlayer(3, 30);
        static readonly MockPlayer[] players = new MockPlayer[] { p1, p2, p3 };

        [Fact]
        public void TestGetTopRanks_sorted()
        {
            var board = new Leaderboard<MockPlayer>(players, 2);
            var tops = board.GetTopRanks(2).ToArray();

            Assert.Equal(p3.ID, tops[0].ID);
            Assert.Equal(p2.ID, tops[1].ID);
        }

        [Fact]
        public void TestGetTopRanks_1st_ranks_is_1()
        {
            var board = new Leaderboard<MockPlayer>(players, 2);
            var tops = board.GetTopRanks(2).ToArray();

            Assert.Equal(1, tops[0].Ranking);
            Assert.Equal(2, tops[1].Ranking);
        }

        [Fact]
        public void IsLeaderboardEqual_equal()
        {
            var boardA = new Leaderboard<MockPlayer>(players, 2);
            var boardB = new Leaderboard<MockPlayer>(players, 2);
            Assert.True(boardA.IsLeaderboardEqual(boardB));
        }

        [Fact]
        public void IsLeaderboardEqual_different_player_size()
        {
            var boardA = new Leaderboard<MockPlayer>(new MockPlayer[] { p1, p2, p3 }, 2);
            var boardB = new Leaderboard<MockPlayer>(new MockPlayer[] { p2, p3 }, 2);
            Assert.False(boardA.IsLeaderboardEqual(boardB));
        }

        [Fact]
        public void IsLeaerboardEqual_top_ranks_equal()
        {
            var top1 = MakePlayer(1, 100);
            var top2 = MakePlayer(2, 200);
            var remain1 = MakePlayer(3, 1);
            var remain2 = MakePlayer(4, 2);

            var boardA = new Leaderboard<MockPlayer>(new MockPlayer[] { top1, top2, remain1 }, 2);
            var boardB = new Leaderboard<MockPlayer>(new MockPlayer[] { top1, top2, remain2 }, 2);

            Assert.True(boardA.IsLeaderboardEqual(boardB));
        }
    }
}
