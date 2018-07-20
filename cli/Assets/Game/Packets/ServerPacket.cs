using System;

namespace Assets.Game.Packets
{
    public class WelcomePacket
    {
        public int version;
    }

    public class StatusPingPacket : ISerializePacket
    {
        public int millis;

        public byte[] Serialize()
        {
            byte[] bytes = BitConverter.GetBytes(millis);
            return bytes;
        }
    }

    public class StatusPongPacket : IDeserializePacket
    {
        public int millis;

        public void Deserialize(byte[] bytes)
        {
            millis = BitConverter.ToInt32(bytes, 0);
        }
    }

}
