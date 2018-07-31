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
        public int ID;
        public Vec2 Position;
        public Vec2 Direction;
        public short LifetimeMillis;

        public short Type => (short)PacketType.AttackNotify;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadValue(ref Position);
            r.ReadValue(ref Direction);
            r.Read(out LifetimeMillis);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteValue(Position);
            w.WriteValue(Direction);
            w.Write(LifetimeMillis);
        }
    }
}
