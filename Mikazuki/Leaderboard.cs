using Hatsushimo;
using Hatsushimo.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikazuki
{
    public interface IRankable
    {
        int ID { get; }
        int Score { get; }
    }

    public class Leaderboard<T> where T : IRankable
    {
        List<Rank> all;
        int topSize;

        public Leaderboard(IEnumerable<T> players, int topSize)
        {
            var sortedPlayers = players.OrderBy(p => p.Score).Reverse();
            var allRanks = sortedPlayers.Select((p, idx) =>
            {
                var rank = idx + 1;
                return new Rank()
                {
                    ID = p.ID,
                    Score = p.Score,
                    Ranking = rank,
                };
            });

            this.all = allRanks.ToList();
            this.topSize = topSize;
        }

        public Leaderboard(Leaderboard<T> o)
        {
            all = o.all.ToList();
            topSize = o.topSize;
        }

        public IEnumerable<Rank> GetTopRanks(int size)
        {
            return this.all.Take(size);
        }

        public int GetRank(int id)
        {
            var found = all.Where(x => x.ID == id)
                .Select(x => x.Ranking)
                .FirstOrDefault();
            Debug.Assert(found >= 1);
            return found;
        }

        public LeaderboardPacket GenerateLeaderboardPacket()
        {
            var players = all.ToArray().Length;
            var top = GetTopRanks(topSize).ToArray();
            return new LeaderboardPacket(players, top);
        }

        public bool IsLeaderboardEqual(Leaderboard<T> o)
        {
            if (topSize != o.topSize) { return false; }

            // 접속자 숫자 바뀌면 다른거로 취급
            var thisSize = all.ToArray().Length;
            var otherSize = o.all.ToArray().Length;
            if (thisSize != otherSize) { return false; }

            var thisArr = GetTopRanks(topSize).ToArray();
            var otherArr = o.GetTopRanks(topSize).ToArray();
            if (thisArr.Length != otherArr.Length) { return false; }

            for (var i = 0; i < thisArr.Length; i++)
            {
                var a = thisArr[i];
                var b = otherArr[i];
                if (a != b) { return false; }
            }
            return true;
        }
    }
}
