using System.Linq;

namespace Assets.Game.Packets
{
    public class PlayerInfo
    {
        public int id;
        public string nickname;
        public float pos_x;
        public float pos_y;
    }

    public class PlayerListPacket
    {
        public PlayerInfo[] players;

        public PlayerInfo[] GetEnemyPlayers(int id)
        {
            return players.Where(x => x.id != id).ToArray();
        }
        public PlayerInfo GetMyPlayer(int id)
        {
            return players.Where(x => x.id == id).First();
        }
    }

    // TODO 이동 관련 정보는 최적화 가능할거같은데
    public class PlayerStatus
    {
        public int id;

        public float pos_x;
        public float pos_y;
        public float dir_x;
        public float dir_y;
        public float speed;
    }

    public class PlayerStatusPacket
    {
        public PlayerStatus[] players;

        public PlayerStatus[] GetEnemyPlayers(int id)
        {
            return players.Where(x => x.id != id).ToArray();
        }
        public PlayerStatus GetMyPlayer(int id)
        {
            return players.Where(x => x.id == id).First();
        }
    }

    public class PlayerSpawnPacket
    {
        public int id;
        public string nickname;
        public float pos_x;
        public float pos_y;
    }

    public class PlayerLeavePacket
    {
        public int id;
    }

    public class PlayerDeadPacket
    {
        public int id;
    }
}
