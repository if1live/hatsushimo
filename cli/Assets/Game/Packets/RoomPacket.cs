using System.Linq;

namespace Assets.Game.Packets
{
    struct RoomJoinRequestPacket
    {
        public string nickname;
        public string room_id;
    }

    class RoomJoinResponsePacket
    {
        public int player_id;
        public string room_id;
        public string nickname;
    }

    class PlayerReplication
    {
        public int id;
        public string nickname;
        public float pos_x;
        public float pos_y;
    }

    class ItemReplication
    {
        public int id;
        public string type;
        public float pos_x;
        public float pos_y;
    }

    class ReplicationPacket
    {
        public PlayerReplication[] players;
        public ItemReplication[] items;

        public PlayerReplication[] GetOtherPlayers(int myid)
        {
            return players.Where(x => x.id != myid).ToArray();
        }

        public PlayerReplication GetMyPlayer(int myid)
        {
            return players.Where(x => x.id == myid).First();
        }
    }
}
