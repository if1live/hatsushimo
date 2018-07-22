namespace HatsushimoShared
{
    public enum PacketType
    {
        Connect = 1,
        Disconnect,

        Welcome,

        Ping,

        RoomJoinReq,
        RoomJoinResp,
        RoomLeave,

        Leaderboard,

        InputMove,
        InputCommand,

        ReplicationAll,
        ReplicationAction,
        ReplicationBulkAction,

        PlayerReady,
    }
}
