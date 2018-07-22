using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;
using Hatsushimo.Types;

namespace Hatsushimo.Packets
{
    public struct InputCommandPacket : IPacket
    {
        public int Mode;

        public short Type => (short)PacketType.InputCommand;

        public IPacket CreateBlank()
        {
            return new InputCommandPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out Mode);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Mode);
        }
    }


    public struct InputMovePacket : IPacket
    {
        public Vec2 Dir;

        public short Type => (short)PacketType.InputMove;

        public IPacket CreateBlank()
        {
            return new InputMovePacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(ref Dir);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Dir);
        }
    }
}
