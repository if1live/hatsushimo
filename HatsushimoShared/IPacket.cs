using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HatsushimoShared
{
    public interface IPacket
    {
        PacketType Type { get; }
        byte[] Serialize();
        IPacket Deserialize(byte[] bytes);
        IPacket CreateBlank();
    }

    // https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
    class ByteJoin
    {
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }

        public static byte[] Combine(IEnumerable<byte[]> arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            return Combine(arrays as IEnumerable<byte[]>);
        }
    }

    class PacketReader
    {
        int offset = 0;
        readonly byte[] bytes;

        public PacketReader(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public void ReadMediumString(out string s)
        {
            short len = 0;
            Read(out len);

            s = Encoding.UTF8.GetString(bytes, offset, len);
            offset += len;
        }

        public void Read(out bool v)
        {
            v = BitConverter.ToBoolean(bytes, offset);
            offset += 1;
        }

        public void Read(out char v)
        {
            v = BitConverter.ToChar(bytes, offset);
            offset += 1;
        }

        public void Read(out short v)
        {
            v = BitConverter.ToInt16(bytes, offset);
            offset += 2;
        }

        public void Read(out int v)
        {
            v = BitConverter.ToInt32(bytes, offset);
            offset += 4;
        }

        public void Read(out long v)
        {
            v = BitConverter.ToInt64(bytes, offset);
            offset += 8;
        }
    }

    class PacketWriter
    {
        readonly List<byte[]> chunks = new List<byte[]>();

        public void WriteMediumString(string s)
        {
            Write((short)s.Length);
            chunks.Add(Encoding.UTF8.GetBytes(s));
        }

        public void Write(bool v) { chunks.Add(BitConverter.GetBytes(v)); }
        public void Write(char v) { chunks.Add(BitConverter.GetBytes(v)); }
        public void Write(short v) { chunks.Add(BitConverter.GetBytes(v)); }
        public void Write(int v) { chunks.Add(BitConverter.GetBytes(v)); }
        public void Write(long v) { chunks.Add(BitConverter.GetBytes(v)); }

        public byte[] Data
        {
            get { return ByteJoin.Combine(chunks); }
        }
    }
}
