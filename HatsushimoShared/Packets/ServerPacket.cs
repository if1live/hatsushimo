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

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }

    public struct DisconnectPacket : IPacket
    {
        public short Type => (short)PacketType.Disconnect;

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }

    public struct WelcomePacket : IPacket
    {
        public int UserID;
        public int Version;

        public short Type => (short)PacketType.Welcome;

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

        public void Deserialize(BinaryReader r)
        {
            r.Read(out millis);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(millis);
        }
    }

    public struct HeartbeatPacket : IPacket
    {
        public short Type => (short)PacketType.Heartbeat;

        public void Deserialize(BinaryReader r) { }

        public void Serialize(BinaryWriter w) { }
    }

    public struct SignUpPacket : IPacket
    {
        public short Type => (short)PacketType.SignUpResult;

        public string Uuid;

        public void Deserialize(BinaryReader r)
        {
            r.ReadString(out Uuid);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteString(Uuid);
        }
    }

    public struct SignUpResultPacket : IPacket
    {
        public int ResultCode;

        public short Type => (short)PacketType.SignUp;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ResultCode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ResultCode);
        }
    }

    public struct AuthenticationPacket : IPacket
    {
        public short Type => (short)PacketType.Authentication;

        public string Uuid;

        public void Deserialize(BinaryReader r)
        {
            r.ReadString(out Uuid);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteString(Uuid);
        }
    }

    public struct AuthenticationResultPacket : IPacket
    {
        public short Type => (short)PacketType.AuthenticationResult;

        public int ResultCode;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ResultCode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ResultCode);
        }
    }
}

