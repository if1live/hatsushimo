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

        ReplicationAll,

        ReplicationRemove,
        ReplicationBulkRemove,

        ReplicationCreateFood,
        ReplicationCreatePlayer,
        ReplicationCreateProjectile,

        Move,
        MoveNotify,

        Attack,
        AttackNotify,
    }
}
