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
            f.Register<HeartbeatPacket>();

            f.Register<ConnectPacket>();
            f.Register<DisconnectPacket>();

            f.Register<InputCommandPacket>();
            f.Register<InputMovePacket>();

            f.Register<ReplicationAllPacket>();
            f.Register<ReplicationActionPacket>();
            f.Register<ReplicationBulkActionPacket>();

            f.Register<WorldJoinRequestPacket>();
            f.Register<WorldJoinResponsePacket>();
            f.Register<WorldLeaveRequestPacket>();
            f.Register<WorldLeaveResponsePacket>();

            f.Register<PlayerReadyPacket>();

            f.Register<LeaderboardPacket>();
            return f;
        }
    }
}
