using Xunit;
using System.Collections;
using System.Collections.Generic;
using Hatsushimo;
using Hatsushimo.Utils;

namespace HatsushimoTest.Utils
{
    public class TestIDPool
    {
        [Fact]
        public void TestNext()
        {
            var pool = new IDPool(10, 10, 1);
            Assert.Equal(10, pool.Next());
            Assert.Equal(11, pool.Next());
            Assert.Equal(12, pool.Next());
            Assert.Equal(13, pool.Next());
        }

        [Fact]
        public void TestRelease()
        {
            var pool = new IDPool(10, 10, 1);
            Assert.Equal(10, pool.Next());
            Assert.Equal(11, pool.Next());

            pool.Release(10);
            pool.Release(11);

            Assert.Equal(11, pool.Next());
            Assert.Equal(10, pool.Next());
        }
    }
}
