namespace Hatsushimo.Packets
{
    public enum PacketType
    {
        Connect = 1,
        Disconnect,

        Welcome,

        Heartbeat,
        Ping,

        WorldJoinReq,
        WorldJoinResp,
        WorldLeaveReq,
        WorldLeaveResp,

        PlayerReady,

        Leaderboard,

        InputMove,
        InputCommand,

        ReplicationAll,
        ReplicationAction,
        ReplicationBulkAction,
    }
}
