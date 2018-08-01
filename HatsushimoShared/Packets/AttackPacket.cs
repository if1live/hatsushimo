using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;
using Hatsushimo.Types;

namespace Hatsushimo.Packets
{
    public struct AttackPacket : IPacket
    {
        public short Mode;
        public short Type => (short)PacketType.Attack;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out Mode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Mode);
        }
    }


    public struct AttackNotifyPacket : IPacket
    {
        ProjectileStatus inner;

        public short Type => (short)PacketType.AttackNotify;
        public void Deserialize(BinaryReader r) { r.ReadValue(ref inner); }
        public void Serialize(BinaryWriter w) { w.WriteValue(inner); }
    }
}
