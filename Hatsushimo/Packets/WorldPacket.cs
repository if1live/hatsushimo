using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public enum PlayerMode
    {
        None = 0,
        Player,
        Observer,
    }

    public struct WorldJoinPacket : IPacket
    {
        public string WorldID { get { return _worldID; } }
        string _worldID;

        public string Nickname { get { return _nickname; } }
        string _nickname;

        public PlayerMode Mode { get { return (PlayerMode)_mode; } }
        byte _mode;

        public WorldJoinPacket(string worldID, string nickname, PlayerMode mode)
        {
            _worldID = worldID;
            _nickname = nickname;
            _mode = (byte)mode;
        }

        public short Type => (short)PacketType.WorldJoin;

        public void Deserialize(BinaryReader r)
        {
            r.ReadString(out _worldID);
            r.ReadString(out _nickname);
            r.Read(out _mode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteString(WorldID);
            w.WriteString(Nickname);
            w.Write(_mode);
        }
    }

    public struct WorldJoinResultPacket : IPacket
    {
        public int ResultCode { get { return _resultCode; } }
        int _resultCode;

        public int PlayerID { get { return _playerID; } }
        int _playerID;

        public WorldJoinResultPacket(int resultCode, int playerID)
        {
            _playerID = playerID;
            _resultCode = resultCode;
        }

        public short Type => (short)PacketType.WorldJoinResult;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _resultCode);
            r.Read(out _playerID);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ResultCode);
            w.Write(PlayerID);
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
        public int PlayerID { get { return _playerID; } }
        int _playerID;

        public WorldLeaveResultPacket(int playerID)
        {
            _playerID = playerID;
        }

        public short Type => (short)PacketType.WorldLeaveResult;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _playerID);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(PlayerID);
        }
    }
}
