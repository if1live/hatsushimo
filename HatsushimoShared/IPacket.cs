namespace HatsushimoShared
{
    public interface IPacket
    {
        PacketType Type { get; }
        byte[] Serialize();
        void Deserialize(byte[] bytes);
        IPacket CreateBlank();
    }
}
