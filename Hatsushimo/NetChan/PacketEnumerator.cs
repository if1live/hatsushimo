using System.IO;

namespace Hatsushimo.NetChan
{
    public class PacketEnumerator
    {
        BinaryReader reader;
        readonly PacketCodec codec = new PacketCodec();

        short currType;
        public int CurrentType { get { return currType; } }

        public PacketEnumerator(BinaryReader reader)
        {
            this.reader = reader;
        }

        public bool MoveNext()
        {
            currType = codec.ReadPacketType(reader);
            if (currType == 0)
            {
                return false;
            }
            return true;
        }

        public bool GetCurrent<T>(out T packet) where T : IPacket, new()
        {
            if (currType == 0)
            {
                packet = new T();
                return false;
            }
            return codec.TryDecode(currType, reader, out packet);
        }
    }
}
