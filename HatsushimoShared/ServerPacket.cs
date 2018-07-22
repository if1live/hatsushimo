using System;
using System.Linq;

namespace HatsushimoShared
{
    public struct ConnectPacket : IPacket
    {
        public PacketType Type => PacketType.Connect;

        public IPacket CreateBlank()
        {
            return new ConnectPacket();
        }

        public IPacket Deserialize(byte[] bytes) { return this; }
        public byte[] Serialize() { return new byte[] { }; }
    }

    public struct DisconnectPacket : IPacket
    {
        public PacketType Type => PacketType.Disconnect;

        public IPacket CreateBlank()
        {
            return new DisconnectPacket();
        }

        public IPacket Deserialize(byte[] bytes) { return this; }
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

        public IPacket Deserialize(byte[] bytes)
        {
            var r = new PacketReader(bytes);
            r.Read(out UserID);
            r.Read(out Version);
            return this;
        }

        public byte[] Serialize()
        {
            var w = new PacketWriter();
            w.Write(UserID);
            w.Write(Version);
            return w.Data;
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

        public IPacket Deserialize(byte[] bytes)
        {
            var r = new PacketReader(bytes);
            r.Read(out millis);
            return this;
        }

        public byte[] Serialize()
        {
            var w = new PacketWriter();
            w.Write(millis);
            return w.Data;
        }
    }
}
