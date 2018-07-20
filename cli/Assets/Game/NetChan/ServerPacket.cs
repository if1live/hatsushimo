using System;

namespace Assets.Game.NetChan
{
    public class WelcomePacket
    {
        public int version;

        public int room_width;
        public int room_height;
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
