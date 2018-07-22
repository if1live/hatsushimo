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
        readonly Dictionary<short, IPacket> table = new Dictionary<short, IPacket>();

        public void Register<T>()
            where T : IPacket, new()
        {
            var skeleton = new T();
            table[skeleton.Type] = skeleton;
        }

        IPacket GetSkeleton(short t)
        {
            return table[t].CreateBlank();
        }

        public byte[] Encode(IPacket packet)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write((short)packet.Type);

            packet.Serialize(writer);

            return stream.ToArray();
        }

        public IPacket Decode(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);

            var packetType = reader.ReadInt16();
            var packet = GetSkeleton(packetType);

            packet.Deserialize(reader);

            return packet;
        }
    }
}
