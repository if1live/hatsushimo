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
        public int players;
        public Rank[] top;
    }
}
