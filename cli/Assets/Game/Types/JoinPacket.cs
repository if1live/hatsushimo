namespace Assets.Game.Types
{
    struct JoinRequestPacket
    {
        public string nickname;
        public string room_id;
    }

    class JoinResponseResponse
    {
        public bool ok;
        public string room_id;
        public string player_id;
        public string nickname;
    }
}
