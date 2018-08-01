using System;
using System.IO;
using Xunit;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.NetChan;

namespace HatsushimoTest.Packets
{
    public class PacketTestCase
    {
        protected T SerializeAndDeserialize<T>(T a) where T : IPacket, new()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            a.Serialize(writer);

            var reader = new BinaryReader(new MemoryStream(stream.ToArray()));
            var b = new T();
            b.Deserialize(reader);

            return b;
        }
    }
}
