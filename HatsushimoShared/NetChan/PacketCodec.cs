using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hatsushimo.Packets;

/*
header
2 byte : packet type
X byte : packet content
*/
namespace Hatsushimo.NetChan
{
    public class PacketCodec
    {
        public byte[] Encode<T>(T packet) where T : IPacket
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write((short)packet.Type);

            packet.Serialize(writer);

            return stream.ToArray();
        }

        public short ReadPacketType(BinaryReader reader)
        {
            try
            {
                return reader.ReadInt16();
            }
            catch (EndOfStreamException)
            {
                return 0;
            }
        }

        public bool TryDecode<T>(short type, BinaryReader reader, out T packet) where T : IPacket, new()
        {
            packet = new T();
            var expectedPacketType = packet.Type;
            if (type == expectedPacketType)
            {
                packet.Deserialize(reader);
                return true;
            }
            return false;
        }
    }

    public class PacketSlicer
    {
        BinaryReader reader;
        readonly PacketCodec codec = new PacketCodec();

        short currType;
        public int CurrentType { get { return currType; } }

        public PacketSlicer(BinaryReader reader)
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
