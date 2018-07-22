using System;
using System.Text;

namespace HatsushimoShared
{
    public struct RoomJoinRequestPacket : IPacket
    {
        public PacketType Type => PacketType.RoomJoinReq;

        public string RoomID;
        public string Nickname;

        public IPacket CreateBlank()
        {
            return new RoomJoinRequestPacket();
        }

        public IPacket Deserialize(byte[] bytes)
        {
            var r = new PacketReader(bytes);
            r.ReadMediumString(out RoomID);
            r.ReadMediumString(out Nickname);
            return this;
        }

        public byte[] Serialize()
        {
            var w = new PacketWriter();
            w.WriteMediumString(RoomID);
            w.WriteMediumString(Nickname);
            return w.Data;
        }
    }

    public struct RoomJoinResponsePacket : IPacket
    {
        public int PlayerID;
        public string RoomID;
        public string Nickname;

        public PacketType Type => PacketType.RoomJoinResp;

        public IPacket CreateBlank()
        {
            return new RoomJoinResponsePacket();
        }

        public IPacket Deserialize(byte[] bytes)
        {
            var r = new PacketReader(bytes);
            r.Read(out PlayerID);
            r.ReadMediumString(out RoomID);
            r.ReadMediumString(out Nickname);
            return this;
        }

        public byte[] Serialize()
        {
            var w = new PacketWriter();
            w.Write(PlayerID);
            w.WriteMediumString(RoomID);
            w.WriteMediumString(Nickname);
            return w.Data;
        }
    }

    public struct RoomLeavePacket : IPacket
    {
        public int PlayerID;

        public PacketType Type => PacketType.RoomLeave;

        public IPacket CreateBlank()
        {
            return new RoomLeavePacket();
        }

        public IPacket Deserialize(byte[] bytes)
        {
            var r = new PacketReader(bytes);
            r.Read(out PlayerID);
            return this;
        }

        public byte[] Serialize()
        {
            var w = new PacketWriter();
            w.Write(PlayerID);
            return w.Data;
        }
    }
}
