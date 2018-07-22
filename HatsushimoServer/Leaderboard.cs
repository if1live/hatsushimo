using Hatsushimo;
using Hatsushimo.Packets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HatsushimoServer
{
    class Leaderboard
    {
        IEnumerable<Rank> all;
        int topSize;

        public Leaderboard(Player[] players, int topSize)
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

            this.all = allRanks;
            this.topSize = topSize;
        }

        public IEnumerable<Rank> GetTopRank(int size)
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


    }
}
