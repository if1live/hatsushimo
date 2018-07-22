using System.Linq;
using Xunit;
using HatsushimoServer;

namespace HatsushimoServerTest
{
    public class TestLeaderboard
    {
        static Player MakePlayer(int id, int score)
        {
            var player = new Player(id, null);
            player.Score = score;
            return player;
        }

        static readonly Player p1 = MakePlayer(1, 10);
        static readonly Player p2 = MakePlayer(2, 20);
        static readonly Player p3 = MakePlayer(3, 30);
        static readonly Player[] players = new Player[] { p1, p2, p3 };

        [Fact]
        public void TestGetTopRanks_sorted()
        {
            var board = new Leaderboard(players, 2);
            var tops = board.GetTopRanks(2).ToArray();

            Assert.Equal(p3.ID, tops[0].ID);
            Assert.Equal(p2.ID, tops[1].ID);
        }

        [Fact]
        public void TestGetTopRanks_1st_ranks_is_1()
        {
            var board = new Leaderboard(players, 2);
            var tops = board.GetTopRanks(2).ToArray();

            Assert.Equal(1, tops[0].Ranking);
            Assert.Equal(2, tops[1].Ranking);
        }

        [Fact]
        public void IsLeaderboardEqual_equal()
        {
            var boardA = new Leaderboard(players, 2);
            var boardB = new Leaderboard(players, 2);
            Assert.True(boardA.IsLeaderboardEqual(boardB));
        }

        [Fact]
        public void IsLeaderboardEqual_different_player_size()
        {
            var boardA = new Leaderboard(new Player[]{ p1, p2, p3 }, 2);
            var boardB = new Leaderboard(new Player[]{ p2, p3 }, 2);
            Assert.False(boardA.IsLeaderboardEqual(boardB));
        }

        [Fact]
        public void IsLeaerboardEqual_top_ranks_equal()
        {
            var top1 = MakePlayer(1, 100);
            var top2 = MakePlayer(2, 200);
            var remain1 = MakePlayer(3, 1);
            var remain2 = MakePlayer(4, 2);

            var boardA = new Leaderboard(new Player[]{ top1, top2, remain1 }, 2);
            var boardB = new Leaderboard(new Player[]{ top1, top2, remain2 }, 2);

            Assert.True(boardA.IsLeaderboardEqual(boardB));
        }
    }
}
