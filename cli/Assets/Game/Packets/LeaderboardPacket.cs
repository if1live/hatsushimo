using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Game.Packets
{
    class Rank
    {
        public int id;
        public int score;
        public int rank;
    }

    class LeaderboardPacket
    {
        public Rank[] leaderboard;
        public Rank my;
    }
}
