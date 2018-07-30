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



}
