using System;
using System.IO;
using System.Text;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public struct RoomJoinRequestPacket : IPacket
    {
        public short Type => (short)PacketType.RoomJoinReq;

        public string RoomID;
        public string Nickname;

        public IPacket CreateBlank()
        {
            return new RoomJoinRequestPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out RoomID);
            r.Read(out Nickname);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(RoomID);
            w.Write(Nickname);
        }
    }

    public struct RoomJoinResponsePacket : IPacket
    {
        public int PlayerID;
        public string RoomID;
        public string Nickname;

        public short Type => (short)PacketType.RoomJoinResp;

        public IPacket CreateBlank()
        {
            return new RoomJoinResponsePacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out PlayerID);
            r.Read(out RoomID);
            r.Read(out Nickname);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(PlayerID);
            w.Write(RoomID);
            w.Write(Nickname);
        }
    }

    public struct RoomLeavePacket : IPacket
    {
        public int PlayerID;

        public short Type => (short)PacketType.RoomLeave;

        public IPacket CreateBlank()
        {
            return new RoomLeavePacket();
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

    public struct PlayerReadyPacket : IPacket
    {
        public short Type => (short)PacketType.PlayerReady;

        public IPacket CreateBlank()
        {
            return new PlayerReadyPacket();
        }

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }
}
