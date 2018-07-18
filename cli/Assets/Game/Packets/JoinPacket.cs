namespace Assets.Game.Packets
{
    struct JoinRequestPacket
    {
        public string nickname;
        public string room_id;
    }

    class JoinResponsePacket
    {
        public int player_id;
        public string room_id;
        public string nickname;
    }
}
