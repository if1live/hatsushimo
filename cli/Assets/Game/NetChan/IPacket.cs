namespace Assets.Game.NetChan
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
