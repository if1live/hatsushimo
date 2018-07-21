using System;
using System.Collections;
using System.Collections.Generic;

namespace HatsushimoShared
{
    public class PacketFactory
    {
        readonly Dictionary<PacketType, IPacket> table = new Dictionary<PacketType, IPacket>();

        public static PacketFactory Create(){
            var f = new PacketFactory();
            f.Register<PingPacket>(); 
            f.Register<WelcomePacket>();
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
            var packetBytes = packet.Serialize();

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
            var packetType = (PacketType)BitConverter.ToInt16(bytes, 0);

            // packet 식별자 2바이트를 제외한 나머지만 사용하고싶다
            // TODO 더 빠르게 잘라내는 방법?
            var packetBytes = new byte[bytes.Length];
            for(var i = 0 ; i < bytes.Length - 2 ; i++) {
                var from = i + 2;
                var to = i;
                packetBytes[to] = bytes[from];
            }

            var packet = GetSkeleton(packetType);
            packet.Deserialize(packetBytes);

            return packet;
        }
    }
}