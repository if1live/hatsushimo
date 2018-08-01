using System.IO;

namespace Hatsushimo.NetChan
{
    public interface ISerialize
    {
        void Serialize(BinaryWriter w);
        void Deserialize(BinaryReader r);
    }
}
