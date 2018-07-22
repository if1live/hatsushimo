using System.IO;
using Hatsushimo.NetChan;

namespace Hatsushimo.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter w, ISerialize v)
        {
            v.Serialize(w);
        }
    }
}
