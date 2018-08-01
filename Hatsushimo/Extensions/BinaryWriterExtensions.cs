using System.IO;
using Hatsushimo.NetChan;
using System.Numerics;

namespace Hatsushimo.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void WriteString(this BinaryWriter w, string v)
        {
            // bool (null check) + string
            var hasValue = (v != null);
            w.Write(hasValue);
            if (hasValue)
            {
                w.Write(v);
            }
        }

        public static void WriteVector(this BinaryWriter w, Vector2 v)
        {
            w.Write(v.X);
            w.Write(v.Y);
        }

        public static void WriteVector(this BinaryWriter w, Vector3 v)
        {
            w.Write(v.X);
            w.Write(v.Y);
            w.Write(v.X);
        }


        // value type은 null 검사가 필요없다
        // object는 null이 될수도 있다
        // 둘을 구분하면 패킷 크기를 줄일수 있다
        public static void WriteValue<T>(this BinaryWriter w, T v)
        where T : ISerialize
        {
            v.Serialize(w);
        }

        public static void WriteObject<T>(this BinaryWriter w, T v)
        where T : ISerialize
        {
            bool notNull = (v != null);
            w.Write(notNull);
            if (notNull)
            {
                v.Serialize(w);
            }
        }

        public static void WriteValues<T>(this BinaryWriter w, T[] v)
        where T : ISerialize
        {
            if (v == null)
            {
                w.Write((short)-1);
                return;
            }

            short len = (short)v.Length;
            w.Write(len);

            for (var i = 0; i < len; i++)
            {
                w.WriteValue(v[i]);
            }
        }

        public static void WriteObjects<T>(this BinaryWriter w, T[] v)
        where T : ISerialize
        {
            if (v == null)
            {
                w.Write((short)-1);
                return;
            }

            short len = (short)v.Length;
            w.Write(len);

            for (var i = 0; i < len; i++)
            {
                w.WriteObject(v[i]);
            }
        }

        public static void WriteArray(this BinaryWriter w, int[] arr)
        {
            if (arr == null)
            {
                w.Write((short)-1);
                return;
            }

            short len = (short)arr.Length;
            w.Write(len);

            for (var i = 0; i < len; i++)
            {
                w.Write(arr[i]);
            }
        }
    }
}
