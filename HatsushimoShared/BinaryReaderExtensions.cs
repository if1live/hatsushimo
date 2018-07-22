using System.IO;

namespace HatsushimoShared
{
    public static class BinaryReaderExtensions
    {
        public static void Read(this BinaryReader r, out string s) { s = r.ReadString(); }
        public static void Read(this BinaryReader r, out bool v) { v = r.ReadBoolean(); }
        public static void Read(this BinaryReader r, out char v) { v = r.ReadChar(); }
        public static void Read(this BinaryReader r, out short v) { v = r.ReadInt16(); }
        public static void Read(this BinaryReader r, out int v) { v = r.ReadInt32(); }
        public static void Read(this BinaryReader r, out long v) { v = r.ReadInt64(); }
        public static void Read(this BinaryReader r, out float v) { v = r.ReadSingle(); }
        public static void Read(this BinaryReader r, out double v) { v = r.ReadDouble(); }
    }
}
