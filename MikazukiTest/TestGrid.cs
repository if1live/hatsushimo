using System;
using System.Numerics;
using Mikazuki;
using Xunit;

namespace MikazukiTest
{
    public class TestGrid
    {
        [Fact]
        public void TestGetCellIndex()
        {
            var zone = new Grid<int>(5, 100, 100);

            Assert.Equal(0, zone.GetCellIndex(0));

            Assert.Equal(0, zone.GetCellIndex(1));
            Assert.Equal(0, zone.GetCellIndex(4));

            Assert.Equal(1, zone.GetCellIndex(5));
            Assert.Equal(2, zone.GetCellIndex(10));

            Assert.Equal(-1, zone.GetCellIndex(-1));
            Assert.Equal(-1, zone.GetCellIndex(-4.9f));

            Assert.Equal(-2, zone.GetCellIndex(-5f));
            Assert.Equal(-2, zone.GetCellIndex(-9.9f));
        }

        [Fact]
        public void TestAdd()
        {
            var zone = new Grid<int>(5, 10, 15);

            zone.Add(new Vector2(2, 7), 1);
            zone.Add(new Vector2(-2, 7), 2);
            zone.Add(new Vector2(2, -7), 3);
            zone.Add(new Vector2(-2, -7), 4);

            {
                var coord = zone.GetCellCoord(new Vector2(2, 7));
                var cell = zone.GetCell(coord.Item1, coord.Item2);
                Assert.Equal(1, cell.Count);
                Assert.Equal(1, cell.ToArray()[0]);
            }

            {
                var coord = zone.GetCellCoord(new Vector2(-2, 7));
                var cell = zone.GetCell(coord.Item1, coord.Item2);
                Assert.Equal(1, cell.Count);
                Assert.Equal(2, cell.ToArray()[0]);
            }

            {
                var coord = zone.GetCellCoord(new Vector2(2, -7));
                var cell = zone.GetCell(coord.Item1, coord.Item2);
                Assert.Equal(1, cell.Count);
                Assert.Equal(3, cell.ToArray()[0]);
            }

            {
                var coord = zone.GetCellCoord(new Vector2(-2, -7));
                var cell = zone.GetCell(coord.Item1, coord.Item2);
                Assert.Equal(1, cell.Count);
                Assert.Equal(4, cell.ToArray()[0]);
            }
        }

        [Fact]
        public void TestAdd_OutOfIndex()
        {
            var zone = new Grid<int>(5, 10, 15);
            Assert.Throws<IndexOutOfRangeException>(() => zone.Add(new Vector2(11, 0), 1));
            Assert.Throws<IndexOutOfRangeException>(() => zone.Add(new Vector2(-11, 0), 1));
            Assert.Throws<IndexOutOfRangeException>(() => zone.Add(new Vector2(0, 16), 1));
            Assert.Throws<IndexOutOfRangeException>(() => zone.Add(new Vector2(0, -16), 1));
        }
    }
}
