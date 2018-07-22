using System.IO;
using Hatsushimo.NetChan;

namespace Hatsushimo.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void Write<T>(this BinaryWriter w, T v)
        where T : ISerialize
        {
            v.Serialize(w);
        }

        public static void Write<T>(this BinaryWriter w, T[] v)
        where T : ISerialize
        {
            short len = 0;
            if(v != null) {
                len = (short)v.Length;
            }
            w.Write(len);

            for (var i = 0; i < len; i++)
            {
                w.Write(v[i]);
            }
        }
    }
}
