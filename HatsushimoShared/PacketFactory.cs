using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HatsushimoShared
{
    public class PacketFactory
    {
        readonly Dictionary<PacketType, IPacket> table = new Dictionary<PacketType, IPacket>();

        public static PacketFactory Create(){
            var f = new PacketFactory();
            f.Register<PingPacket>();
            f.Register<WelcomePacket>();
            f.Register<ConnectPacket>();
            f.Register<DisconnectPacket>();
            return f;
        }

        public void Register<T>()
            where T: IPacket, new()
        {
            var skeleton = new T();
            table[skeleton.Type] = skeleton;
        }

        IPacket GetSkeleton(PacketType t)
        {
            return table[t].CreateBlank();
        }

        public byte[] Serialize(IPacket packet) {
            var typeBytes = BitConverter.GetBytes((short)packet.Type);

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            packet.Serialize(writer);
            var packetBytes = stream.ToArray();

            // TODO optimize
            var data = new byte[typeBytes.Length + packetBytes.Length];
            for(var i = 0 ; i < typeBytes.Length ; i++) {
                data[i] = typeBytes[i];
            }
            for(var i = 0 ; i < packetBytes.Length ; i++) {
                var offset = typeBytes.Length;
                data[offset+i] = packetBytes[i];
            }

            return data;
        }

        public IPacket Deserialize(byte[] bytes) {
            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);

            var packetType = (PacketType)reader.ReadInt16();
            var packet = GetSkeleton(packetType);

            packet.Deserialize(reader);

            return packet;
        }
    }
}
