using System.IO;

namespace HatsushimoShared
{
    public struct InputCommandPacket : IPacket
    {
        public int Mode;

        public PacketType Type => PacketType.InputCommand;

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
        public float DirX;
        public float DirY;

        public PacketType Type => PacketType.InputMove;

        public IPacket CreateBlank()
        {
            return new InputMovePacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out DirX);
            r.Read(out DirY);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(DirX);
            w.Write(DirY);
        }
    }
}
