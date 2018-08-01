using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public struct WorldJoinPacket : IPacket
    {
        public short Type => (short)PacketType.WorldJoin;

        public string WorldID;
        public string Nickname;

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

    public struct WorldJoinResultPacket : IPacket
    {
        public int PlayerID;
        public string WorldID;
        public string Nickname;

        public short Type => (short)PacketType.WorldJoinResult;

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

    public struct WorldLeavePacket : IPacket
    {
        public short Type => (short)PacketType.WorldLeave;

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }

    public struct WorldLeaveResultPacket : IPacket
    {
        public int PlayerID;

        public short Type => (short)PacketType.WorldLeaveResult;

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
