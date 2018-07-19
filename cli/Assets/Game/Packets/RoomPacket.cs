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

    class RoomLeavePacket
    {
        public int player_id;
    }
}
