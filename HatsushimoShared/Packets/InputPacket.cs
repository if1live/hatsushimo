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
        // 속도보다 목표 지점을 보내는게 보정하기 좋다더라
        public Vec2 TargetPos;

        public short Type => (short)PacketType.InputMove;

        public void Deserialize(BinaryReader r)
        {
            r.ReadValue(ref TargetPos);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteValue(TargetPos);
        }
    }
}
