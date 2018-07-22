using Xunit;
using System.Collections;
using System.Collections.Generic;
using Hatsushimo;
using Hatsushimo.Utils;

namespace HatsushimoServerTest.Utils
{
    public class TestIDGenerator
    {
        [Fact]
        public void TestNextID()
        {
            var idgen = IDGenerator.Make(10, 10);
            var iter = idgen.GetEnumerator();

            AssertEnumeratorValueExist(iter, 10);
            AssertEnumeratorValueExist(iter, 11);
            AssertEnumeratorValueExist(iter, 12);
            AssertEnumeratorValueExist(iter, 13);
        }

        [Fact]
        public void TestLoopRange()
        {
            var idgen = IDGenerator.Make(10, 3);
            var iter = idgen.GetEnumerator();

            AssertEnumeratorValueExist(iter, 10);
            AssertEnumeratorValueExist(iter, 11);
            AssertEnumeratorValueExist(iter, 12);
            AssertEnumeratorValueExist(iter, 10);
        }

        void AssertEnumeratorValueExist(IEnumerator<int> iter, int val)
        {
            Assert.True(iter.MoveNext());
            Assert.Equal(val, iter.Current);
        }
    }
}
