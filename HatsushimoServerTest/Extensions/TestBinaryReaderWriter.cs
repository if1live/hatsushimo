using System.IO;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Extensions;
using Xunit;
using System.Collections.Generic;

namespace HatsushimoServerTest.Extensions
{
    public class TestBinaryReaderWriter
    {
        [Fact]
        public void TestString_null()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            string a = null;
            writer.WriteString(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            string b = null;
            reader.ReadString(out b);

            Assert.Equal(a, b);
        }

        class DummyStruct : ISerialize
        {
            public int v;
            public void Deserialize(BinaryReader r) { r.Read(out v); }
            public void Serialize(BinaryWriter w) { w.Write(v); }

            public class Comparer : IEqualityComparer<DummyStruct>
            {
                public bool Equals(DummyStruct x, DummyStruct y)
                {
                    return x.v == y.v;
                }

                public int GetHashCode(DummyStruct obj)
                {
                    return obj.v.GetHashCode();
                }
            }
        }

        class DummyClass : ISerialize
        {
            public int v;
            public void Deserialize(BinaryReader r) { r.Read(out v); }
            public void Serialize(BinaryWriter w) { w.Write(v); }

            public class Comparer : IEqualityComparer<DummyClass>
            {
                public bool Equals(DummyClass x, DummyClass y)
                {
                    return x.v == y.v;
                }

                public int GetHashCode(DummyClass obj)
                {
                    return obj.v.GetHashCode();
                }
            }

        }

        [Fact]
        public void TestValue()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyStruct a = new DummyStruct() { v = 123 };
            writer.WriteValue(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyStruct b = new DummyStruct();
            reader.ReadValue(ref b);

            Assert.Equal(a, b, new DummyStruct.Comparer());
        }

        [Fact]
        public void TestObject_not_null()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyClass a = new DummyClass() { v = 12 };
            writer.WriteObject(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyClass b = null;
            reader.ReadObject(out b);

            Assert.Equal(a, b, new DummyClass.Comparer());
        }

        [Fact]
        public void TestObject_null()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyClass a = null;
            writer.WriteObject(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyClass b = null;
            reader.ReadObject(out b);

            Assert.Null(b);
        }

        [Fact]
        public void TestValues_empty()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyStruct[] a = new DummyStruct[0];
            writer.WriteValues(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyStruct[] b = null;
            reader.ReadValues(out b);

            Assert.Empty(b);
        }

        [Fact]
        public void TestValues_basic()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyStruct[] a = new DummyStruct[]
            {
                new DummyStruct() { v = 1 },
                new DummyStruct() { v = 2 },
            };
            writer.WriteValues(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyStruct[] b = null;
            reader.ReadValues(out b);

            Assert.Equal(a, b, new DummyStruct.Comparer());
        }

        [Fact]
        public void TestValues_array_is_null()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyStruct[] a = null;
            writer.WriteValues(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyStruct[] b = null;
            reader.ReadValues(out b);

            Assert.Null(b);
        }

        [Fact]
        public void TestObjects_empty()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyClass[] a = new DummyClass[0];
            writer.WriteObjects(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyClass[] b = null;
            reader.ReadObjects(out b);

            Assert.Empty(b);
        }

        [Fact]
        public void TestObjects_basic()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyClass[] a = new DummyClass[]
            {
                new DummyClass() { v = 12 },
                new DummyClass() { v = 34 },
                null,
            };
            writer.WriteObjects(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyClass[] b = null;
            reader.ReadObjects(out b);

            AssertEqualArray(a, b, new DummyClass.Comparer());
        }

        [Fact]
        public void TestObjects_array_is_null()
        {
            var sw = new MemoryStream();
            var writer = new BinaryWriter(sw);

            DummyClass[] a = null;
            writer.WriteObjects(a);

            var sr = new MemoryStream(sw.ToArray());
            var reader = new BinaryReader(sr);

            DummyClass[] b = null;
            reader.ReadObjects(out b);

            Assert.Null(b);
        }

        void AssertEqualArray<T>(T[] a, T[] b, IEqualityComparer<T> comp)
        {
            Assert.Equal(a.Length, b.Length);
            for (var i = 0; i < a.Length; i++)
            {
                var x = a[i];
                var y = b[i];
                if (x == null && y == null)
                {
                    continue;
                }
                else if (x == null && y != null)
                {
                    Assert.True(false);
                }
                else if (x != null && y == null)
                {
                    Assert.True(false);
                }
                else
                {
                    Assert.Equal(a[i], b[i], comp);
                }
            }
        }
    }
}
