namespace Hatsushimo.Packets
{
    public enum PacketType
    {
        Invalid = 0,

        Connect = 1,
        Disconnect,

        SignUp,
        SignUpResult,

        Authentication,
        AuthenticationResult,

        Welcome,

        Heartbeat,
        Ping,

        WorldJoin,
        WorldJoinResult,
        WorldLeave,
        WorldLeaveResult,

        PlayerReady,

        Leaderboard,

        InputCommand,

        ReplicationAll,
        ReplicationAction,
        ReplicationBulkAction,

        ReplicationBulkRemove,

        Move,
        MoveNotify,
    }
}
