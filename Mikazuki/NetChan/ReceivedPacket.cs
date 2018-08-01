using System.IO;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;

namespace Mikazuki.NetChan
{
    // 패킷을 해석해서 들고있으면 레이어를 건너다닐떄 IPacket가 박싱될수있다
    // 패밋을 해석하는데 필요한 데이터를 갖고 다니자
    public struct ReceivedPacket<TPacket>
        where TPacket : IPacket, new()
    {
        public readonly Session Session;
        public readonly byte[] Data;

        public PacketType Type
        {
            get
            {
                var stream = new MemoryStream(Data);
                var reader = new BinaryReader(stream);
                var codec = new PacketCodec();
                return (PacketType)codec.ReadPacketType(reader);
            }
        }
        public TPacket Packet
        {
            get
            {
                var stream = new MemoryStream(Data);
                var reader = new BinaryReader(stream);
                var codec = new PacketCodec();
                var type = codec.ReadPacketType(reader);
                TPacket packet;
                codec.TryDecode<TPacket>(type, reader, out packet);
                return packet;
            }
        }

        public ReceivedPacket(Session s, byte[] data)
        {
            this.Session = s;
            this.Data = data;
        }
    }
}
