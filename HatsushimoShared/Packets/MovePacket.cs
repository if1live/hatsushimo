using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;
using Hatsushimo.Types;

namespace Hatsushimo.Packets
{
    public struct MovePacket : IPacket
    {
        // 속도보다 목표 지점을 보내는게 보정하기 좋다더라
        public Vec2 TargetPos;

        public short Type => (short)PacketType.Move;

        public void Deserialize(BinaryReader r)
        {
            r.ReadValue(ref TargetPos);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteValue(TargetPos);
        }
    }

    public struct MoveNotify : ISerialize
    {
        public int ID;
        public Vec2 TargetPos;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadValue(ref TargetPos);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteValue(TargetPos);
        }
    }

    // 이동은 보통 1명 이상의 정보를 내려줄것이다
    // 1명만 보내는건 최적화할때 추가하자
    // TOOD
    // http://www.gabrielgambetta.com/client-server-game-architecture.html
    public struct MoveNotifyPacket : IPacket
    {
        public MoveNotify[] list;

        public short Type => (short)PacketType.MoveNotify;

        public void Deserialize(BinaryReader r)
        {
            r.ReadValues(out list);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteValues(list);
        }
    }
}
