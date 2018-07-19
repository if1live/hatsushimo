namespace Assets.Game.Packets
{
    class Events
    {
        public const string STATUS_PING = "status-ping";
        public const string STATUS_PONG = "status-pong";

        public const string HELLO = "hello";
        public const string WELCOME = "welcome";

        public const string ROOM_JOIN = "room-join";
        public const string ROOM_LEAVE = "room-leave";

        public const string LEADERBOARD = "leaderboard";

        public const string MOVE = "move";

        public const string REPLICATION_ALL = "replication-all";
        public const string REPLICATION_ACTION = "replication-action";
        public const string REPLICATION_BULK_ACTION = "replication-bulk-action";

        public const string PLAYER_READY = "player-ready";
    }
}
