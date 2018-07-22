using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public class MyPacketCodec
    {
        // TODO 특정 상황에 맞는 패킷 변환기니까 밖으로 뺴는게 맞을듯
        public static PacketCodec Create()
        {
            var f = new PacketCodec();
            f.Register<PingPacket>();
            f.Register<WelcomePacket>();

            f.Register<ConnectPacket>();
            f.Register<DisconnectPacket>();

            f.Register<InputCommandPacket>();
            f.Register<InputMovePacket>();

            f.Register<ReplicationActionPacket>();
            f.Register<ReplicationBulkActionPacket>();

            f.Register<RoomJoinRequestPacket>();
            f.Register<RoomJoinResponsePacket>();
            f.Register<RoomLeavePacket>();
            f.Register<PlayerReadyPacket>();

            f.Register<LeaderboardPacket>();
            return f;
        }
    }
}
