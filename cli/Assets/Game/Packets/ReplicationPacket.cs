namespace Assets.Game.Packets
{
    public class PlayerInitial
    {
        public int id;
        public string nickname;
        public float pos_x;
        public float pos_y;
    }

    public class ItemInitial
    {
        public int id;
        public string type;
        public float pos_x;
        public float pos_y;
    }

    class ReplicationAllPacket
    {
        public PlayerInitial[] players;
        public ItemInitial[] items;
    }

    class ReplicationActions
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Remove = "remove";
    }

    public class ReplicationActionPacket
    {
        public string action;
        public int id;
        public string type;
        public float pos_x;
        public float pos_y;
        public float dir_x;
        public float dir_y;
        public float speed;
        public string extra;
    }

    class ReplicationBulkActionPacket
    {
        public ReplicationActionPacket[] actions;
    }
}
