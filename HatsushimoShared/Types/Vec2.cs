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

        public Vec2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Vec2 Normalize()
        {
            var len = Magnitude;
            if (len == 0)
            {
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

        public static bool operator ==(Vec2 a, Vec2 b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (ReferenceEquals(a, null)) { return false; }
            if (ReferenceEquals(b, null)) { return false; }

            return (a.X == b.X)
                && (a.Y == b.Y);
        }

        public static bool operator !=(Vec2 a, Vec2 b)
        {
            return !(a == b);
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            var x = a.X + b.X;
            var y = a.Y + b.Y;
            return new Vec2(x, y);
        }
        public static Vec2 operator -(Vec2 a)
        {
            return new Vec2(-a.X, -a.Y);
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return a + (-b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((Vec2)obj);
        }

        public bool Equals(Vec2 other)
        {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return X.Equals(other.X)
                && Y.Equals(other.Y);
        }

        public override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = hash ^ Y.GetHashCode();
            return hash;
        }
    }
}
