using System;
using System.IO;
using System.Text;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public struct PlayerReadyPacket : IPacket
    {
        public short Type => (short)PacketType.PlayerReady;

        public IPacket CreateBlank()
        {
            return new PlayerReadyPacket();
        }

        public void Deserialize(BinaryReader r) { }
        public void Serialize(BinaryWriter w) { }
    }
}
