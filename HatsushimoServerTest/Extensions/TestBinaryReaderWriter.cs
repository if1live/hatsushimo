using System.IO;
using Hatsushimo;
using Hatsushimo.Extensions;
using Xunit;

namespace HatsushimoServerTest.Extensions
{
    public class TestBinaryReaderWriter {
        [Fact]
        public void TestString_null() {
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
    }
}
