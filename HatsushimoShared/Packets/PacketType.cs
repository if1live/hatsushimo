namespace Hatsushimo.Packets
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
        PlayerReady,

        Leaderboard,

        InputMove,
        InputCommand,

        ReplicationAll,
        ReplicationAction,
        ReplicationBulkAction,
    }
}
