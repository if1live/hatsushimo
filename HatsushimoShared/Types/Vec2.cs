using System;
using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Types
{
    public struct Vec2 : ISerialize
    {
        public float X;
        public float Y;

        public static readonly Vec2 Zero = new Vec2(0, 0);

        public Vec2(float x, float y) {
            this.X = x;
            this.Y = y;
        }

        public Vec2 Normalize()
        {
            var len = Magnitude;
            if(len == 0) {
                return new Vec2(0, 0);
            }
            var inv = 1 / len;
            return new Vec2(X * inv, Y * inv);
        }

        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(SqrMagnitude);
            }
        }

        public float SqrMagnitude
        {
            get
            {
                return X * X + Y * Y;
            }
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(X);
            w.Write(Y);
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out X);
            r.Read(out Y);
        }
    }
}
