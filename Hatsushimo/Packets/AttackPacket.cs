using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public struct AttackPacket : IPacket
    {
        public short Mode { get { return _mode; } }
        short _mode;

        public AttackPacket(short mode)
        {
            _mode = mode;
        }


        public short Type => (short)PacketType.Attack;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _mode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Mode);
        }
    }


    public struct AttackNotifyPacket : IPacket
    {
        ProjectileStatus inner;
        public AttackNotifyPacket(ProjectileStatus status)
        {
            inner = status;
        }

        public short Type => (short)PacketType.AttackNotify;
        public void Deserialize(BinaryReader r) { r.ReadValue(ref inner); }
        public void Serialize(BinaryWriter w) { w.WriteValue(inner); }
    }
}
