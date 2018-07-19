namespace Assets.Game.Packets
{
    public interface ISerializePacket
    {
        byte[] Serialize();
    }

    public interface IDeserializePacket
    {
        void Deserialize(byte[] bytes);
    }
}
