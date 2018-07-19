namespace Assets.Game.Packets
{
    public enum Events
    {
        HELLO = 1,
        WELCOME,

        STATUS_PING,
        STATUS_PONG,

        ROOM_JOIN,
        ROOM_LEAVE,

        LEADERBOARD,

        MOVE,

        REPLICATION_ALL,
        REPLICATION_ACTION,
        REPLICATION_BULK_ACTION,

        PLAYER_READY,
    }

    public static class EventsExtensions
    {
        public static string MakeString(this Events ev)
        {
            int val = (int)ev;
            return val.ToString();
        }
    }
}
