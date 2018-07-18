namespace Assets.Game.Packets
{
    class Events
    {
        public const string STATUS_PING = "status-ping";
        public const string STATUS_PONG = "status-pong";

        public const string ROOM_JOIN = "room-join";
        public const string ROOM_LEAVE = "room-leave";

        public const string LEADERBOARD = "leaderboard";

        public const string MOVE = "move";

        public const string PLAYER_LIST = "player-list";
        public const string PLAYER_STATUS = "player-status";

        public const string PLAYER_SPAWN = "player-spawn";
        public const string PLAYER_DEAD = "player-dead";
        public const string PLAYER_LEAVE = "player-leave";
        public const string PLAYER_READY = "player-ready";
    }
}
