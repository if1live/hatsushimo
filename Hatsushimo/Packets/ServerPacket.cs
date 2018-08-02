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
        public int UserID { get { return _userID; } }
        int _userID;

        public int Version { get { return _version; } }
        int _version;

        public WelcomePacket(int userID, int version)
        {
            this._userID = userID;
            this._version = version;
        }

        public short Type => (short)PacketType.Welcome;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _userID);
            r.Read(out _version);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(UserID);
            w.Write(Version);
        }
    }

    public struct PingPacket : IPacket
    {
        public int Millis { get { return _millis; } }
        int _millis;

        public PingPacket(int millis)
        {
            _millis = millis;
        }

        public short Type => (short)PacketType.Ping;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _millis);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Millis);
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
        public string Uuid { get { return _uuid; } }
        string _uuid;

        public SignUpPacket(string uuid)
        {
            this._uuid = uuid;
        }

        public short Type => (short)PacketType.SignUpResult;

        public void Deserialize(BinaryReader r)
        {
            r.ReadString(out _uuid);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteString(Uuid);
        }
    }

    public struct SignUpResultPacket : IPacket
    {
        public int ResultCode { get { return _resultCode; } }
        int _resultCode;

        public SignUpResultPacket(int resultcode)
        {
            _resultCode = resultcode;
        }

        public short Type => (short)PacketType.SignUp;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _resultCode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ResultCode);
        }
    }

    public struct AuthenticationPacket : IPacket
    {
        public string Uuid { get { return _uuid; } }
        string _uuid;

        public AuthenticationPacket(string uuid)
        {
            this._uuid = uuid;
        }

        public short Type => (short)PacketType.Authentication;

        public void Deserialize(BinaryReader r)
        {
            r.ReadString(out _uuid);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteString(Uuid);
        }
    }

    public struct AuthenticationResultPacket : IPacket
    {
        public int ResultCode { get { return _resultCode; } }
        int _resultCode;

        public AuthenticationResultPacket(int resultcode)
        {
            _resultCode = resultcode;
        }

        public short Type => (short)PacketType.AuthenticationResult;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _resultCode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ResultCode);
        }
    }
}

