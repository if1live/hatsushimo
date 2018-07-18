using System.Linq;

namespace Assets.Game.Packets
{
    public class PlayerInfo
    {
        public int id;
        public string nickname;
    }

    public class PlayerListPacket
    {
        public PlayerInfo[] players;
    }


    public class PlayerStatus
    {
        public int id;

        public float pos_x;
        public float pos_y;
        public float dir_x;
        public float dir_y;
        public float speed;
    }

    public class PlayerPacket
    {
        public PlayerStatus[] players;

        public PlayerStatus[] GetEnemyPlayers(int id) {
            return players.Where(x => x.id != id).ToArray();
        }

        public PlayerStatus GetMyPlayer(int id)
        {
            return players.Where(x => x.id == id).First();
        }
    }
}
