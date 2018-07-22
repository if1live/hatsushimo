using System;
using System.IO;
using System.Linq;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public struct ConnectPacket : IPacket
    {
        public short Type => (short)PacketType.Connect;

        public IPacket CreateBlank()
        {
            return new ConnectPacket();
        }

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }

    public struct DisconnectPacket : IPacket
    {
        public short Type => (short)PacketType.Disconnect;

        public IPacket CreateBlank()
        {
            return new DisconnectPacket();
        }

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }

    public struct WelcomePacket : IPacket
    {
        public int UserID;
        public int Version;

        public short Type => (short)PacketType.Welcome;

        public IPacket CreateBlank()
        {
            return new WelcomePacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out UserID);
            r.Read(out Version);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(UserID);
            w.Write(Version);
        }
    }

    public struct PingPacket : IPacket
    {
        public int millis;

        public short Type => (short)PacketType.Ping;

        public IPacket CreateBlank()
        {
            return new PingPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out millis);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(millis);
        }
    }
}
