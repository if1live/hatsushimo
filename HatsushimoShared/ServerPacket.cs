using System;

namespace HatsushimoShared
{
    public struct WelcomePacket : IPacket
    {
        public int version;

        public PacketType Type => PacketType.Welcome;

        public IPacket CreateBlank()
        {
            return new WelcomePacket();
        }

        public void Deserialize(byte[] bytes)
        {
            version = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] Serialize()
        {
            byte[] bytes = BitConverter.GetBytes(version);
            return bytes;
        }
    }

    public struct PingPacket : IPacket
    {
        public int millis;

        public PacketType Type => PacketType.Ping;

        public IPacket CreateBlank()
        {
            return new PingPacket();
        }

        public void Deserialize(byte[] bytes)
        {
            millis = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] Serialize()
        {
            byte[] bytes = BitConverter.GetBytes(millis);
            return bytes;
        }
    }

    /*
    public struct RoomJoinRequestPacket
    {
        public string nickname;
        public string room_id;
    }

    public struct RoomJoinResponsePacket{
        public int player_id;
        public string room_id;
        public string nickname;
    }

    public struct RoomLeavePacket {
        public int player_id;
    }
    */
}