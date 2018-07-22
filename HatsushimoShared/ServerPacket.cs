using System;
using System.Linq;

namespace HatsushimoShared
{
    // https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
    class PacketBuilder
    {
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }
    }

    public struct ConnectPacket : IPacket
    {
        public PacketType Type => PacketType.Connect;

        public IPacket CreateBlank()
        {
            return new ConnectPacket();
        }

        public void Deserialize(byte[] bytes) { }
        public byte[] Serialize() { return new byte[] { }; }
    }

    public struct DisconnectPacket : IPacket
    {
        public PacketType Type => PacketType.Disconnect;

        public IPacket CreateBlank()
        {
            return new DisconnectPacket();
        }

        public void Deserialize(byte[] bytes) { }
        public byte[] Serialize() { return new byte[] { }; }
    }

    public struct WelcomePacket : IPacket
    {
        public int UserID;
        public int Version;

        public PacketType Type => PacketType.Welcome;

        public IPacket CreateBlank()
        {
            return new WelcomePacket();
        }

        public void Deserialize(byte[] bytes)
        {
            UserID = BitConverter.ToInt32(bytes, 0);
            Version = BitConverter.ToInt32(bytes, 4);
        }

        public byte[] Serialize()
        {
            var idBytes = BitConverter.GetBytes(UserID);
            var versionBytes = BitConverter.GetBytes(Version);
            var data = PacketBuilder.Combine(idBytes, versionBytes);
            return data;
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
