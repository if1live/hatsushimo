using System.IO;
using Hatsushimo.NetChan;

namespace Hatsushimo.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static void Read(this BinaryReader r, out bool v) { v = r.ReadBoolean(); }
        public static void Read(this BinaryReader r, out char v) { v = r.ReadChar(); }
        public static void Read(this BinaryReader r, out short v) { v = r.ReadInt16(); }
        public static void Read(this BinaryReader r, out int v) { v = r.ReadInt32(); }
        public static void Read(this BinaryReader r, out long v) { v = r.ReadInt64(); }
        public static void Read(this BinaryReader r, out float v) { v = r.ReadSingle(); }
        public static void Read(this BinaryReader r, out double v) { v = r.ReadDouble(); }

        public static void ReadString(this BinaryReader r, out string s)
        {
            bool hasValue = false;
            r.Read(out hasValue);
            if (hasValue)
            {
                s = r.ReadString();
            }
            else
            {
                s = null;
            }
        }

        public static void ReadValue<T>(this BinaryReader r, ref T v)
        where T : ISerialize
        {
            v.Deserialize(r);
        }


        public static void ReadObject<T>(this BinaryReader r, out T v)
        where T : ISerialize, new()
        {
            bool notNull = false;
            r.Read(out notNull);
            if (notNull)
            {
                v = new T();
                v.Deserialize(r);
            }
            else
            {
                v = default(T);
            }
        }

        public static void ReadValues<T>(this BinaryReader r, out T[] v)
        where T : ISerialize, new()
        {
            short len = 0;
            r.Read(out len);

            if (len == -1)
            {
                v = null;
                return;
            }

            v = new T[len];
            for (var i = 0; i < len; i++)
            {
                v[i] = new T();
                r.ReadValue(ref v[i]);
            }
        }

        public static void ReadObjects<T>(this BinaryReader r, out T[] v)
        where T : ISerialize, new()
        {
            short len = 0;
            r.Read(out len);
            if (len == -1)
            {
                v = null;
                return;
            }

            v = new T[len];
            for (var i = 0; i < len; i++)
            {
                T x;
                r.ReadObject(out x);
                v[i] = x;
            }
        }
    }
}
