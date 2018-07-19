namespace Assets.Game.Packets
{
    public class StaticItemCreatePacket
    {
        public string type;
        public int id;
        public float pos_x;
        public float pos_y;
    }

    public class StaticItemRemovePacket
    {
        public string type;
        public int id;
    }

    public class StaticItemListPacket
    {
        public StaticItemCreatePacket[] items;
    }
}
