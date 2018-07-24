using System.IO;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Extensions;
using Xunit;

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
        }

        class DummyClass : ISerialize
        {
            public int v;
            public void Deserialize(BinaryReader r) { r.Read(out v); }
            public void Serialize(BinaryWriter w) { w.Write(v); }
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

            Assert.Equal(a.v, b.v);
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

            Assert.Equal(a.v, b.v);
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

            Assert.Equal(a, b);
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

            Assert.Equal(0, b.Length);
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

            Assert.Equal(2, b.Length);
            Assert.Equal(1, b[0].v);
            Assert.Equal(2, b[1].v);
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

            Assert.Equal(a, b);
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

            Assert.Equal(0, b.Length);
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

            Assert.Equal(3, b.Length);
            Assert.Equal(a[0].v, b[0].v);
            Assert.Equal(a[1].v, b[1].v);
            Assert.Equal(null, b[2]);
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

            Assert.Equal(a, b);
        }
    }
}
