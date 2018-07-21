namespace HatsushimoShared
{
    public enum PacketType
    {
        Welcome = 1,

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