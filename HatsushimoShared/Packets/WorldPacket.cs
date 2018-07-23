using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public struct WorldJoinRequestPacket : IPacket
    {
        public short Type => (short)PacketType.WorldJoinReq;

        public string WorldID;
        public string Nickname;

        public IPacket CreateBlank()
        {
            return new WorldJoinRequestPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.ReadString(out WorldID);
            r.ReadString(out Nickname);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteString(WorldID);
            w.WriteString(Nickname);
        }
    }

    public struct WorldJoinResponsePacket : IPacket
    {
        public int PlayerID;
        public string WorldID;
        public string Nickname;

        public short Type => (short)PacketType.WorldJoinResp;

        public IPacket CreateBlank()
        {
            return new WorldJoinResponsePacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out PlayerID);
            r.ReadString(out WorldID);
            r.ReadString(out Nickname);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(PlayerID);
            w.WriteString(WorldID);
            w.WriteString(Nickname);
        }
    }

    public struct WorldLeaveRequestPacket : IPacket
    {
        public short Type => (short)PacketType.WorldLeaveReq;

        public IPacket CreateBlank()
        {
            return new WorldLeaveRequestPacket();
        }

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }

    public struct WorldLeaveResponsePacket : IPacket
    {
        public int PlayerID;

        public short Type => (short)PacketType.WorldLeaveResp;

        public IPacket CreateBlank()
        {
            return new WorldLeaveResponsePacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out PlayerID);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(PlayerID);
        }
    }
}
