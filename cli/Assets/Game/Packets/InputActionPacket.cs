namespace Assets.Game.Packets
{
    struct InputMovePacket
    {
        public float dir_x;
        public float dir_y;
    }

    struct InputCommandPacket
    {
        public int mode;    
    }
}
