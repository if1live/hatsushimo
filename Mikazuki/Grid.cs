using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;

namespace Mikazuki
{
    public class Cell<T>
    {
        readonly List<T> elems = new List<T>();

        public void Clear() { elems.Clear(); }
        public void Add(T el) { elems.Add(el); }
        public int Count { get { return elems.Count; } }
        public T[] ToArray() { return elems.ToArray(); }
    }

    public class Grid<T>
    {
        // width, height가 동일한 정사각형을 grid로 정의
        readonly int cellSize;

        readonly int halfWidth;
        readonly int halfHeight;

        readonly int horizontalCount;
        readonly int verticalCount;

        readonly List<Cell<T>> cells;

        public Grid(int size, int hx, int hy)
        {
            // 크기는 격자의 배수로
            // 경계값 처리를 복잡하게 하고싶지 않다
            Debug.Assert(size > 0);
            Debug.Assert(hx > 0);
            Debug.Assert(hy > 0);
            Debug.Assert(hx % size == 0);
            Debug.Assert(hy % size == 0);

            this.cellSize = size;
            this.halfWidth = hx;
            this.halfHeight = hy;

            this.horizontalCount = (halfWidth / cellSize) * 2;
            this.verticalCount = (halfHeight / cellSize) * 2;
            var gridCount = horizontalCount * verticalCount;
            cells = new List<Cell<T>>(gridCount);
            for (var i = 0; i < gridCount; i++) { cells.Add(new Cell<T>()); }
        }

        int ToCellIndex(int x, int y)
        {
            var halfX = horizontalCount / 2;
            var halfY = verticalCount / 2;
            if (x < -halfX)
            {
                var msg = $"min cell x={-halfX}, input={x}";
                throw new IndexOutOfRangeException(msg);
            }
            if (x >= halfX)
            {
                var msg = $"max cell x={halfX-1}, input={x}";
                throw new IndexOutOfRangeException(msg);
            }
            if (y < -halfY)
            {
                var msg = $"min cell y={-halfY}, input={y}";
                throw new IndexOutOfRangeException(msg);
            }
            if (y >= halfY)
            {
                var msg = $"max cell y={halfY-1}, input={y}";
                throw new IndexOutOfRangeException(msg);
            }

            var cellX = halfX + x;
            var cellY = halfY + y;
            return cellX * horizontalCount + cellY;
        }

        // ...
        // -size ~ 0: -1
        // 0 ~ size: 0
        // ...
        public int GetCellIndex(float v)
        {
            if (v >= 0)
            {
                var floor = (int)v;
                var idx = floor / cellSize;
                return idx;
            }
            else
            {
                var absval = -v;
                var floor = (int)absval;
                var idx = floor / cellSize + 1;
                return -idx;
            }
        }
        public Tuple<int, int> GetCellCoord(Vector2 v)
        {
            var x = GetCellIndex(v.X);
            var y = GetCellIndex(v.Y);
            return new Tuple<int, int>(x, y);
        }

        public void Clear()
        {
            foreach (var sub in cells) { sub.Clear(); }
        }

        public void Add(Vector2 pos, T el)
        {
            var cellX = GetCellIndex(pos.X);
            var cellY = GetCellIndex(pos.Y);
            var subIdx = ToCellIndex(cellX, cellY);
            var cell = cells[subIdx];
            cell.Add(el);
        }

        public Cell<T> GetCell(int cellX, int cellY)
        {
            var subIdx = ToCellIndex(cellX, cellY);
            var cell = cells[subIdx];
            return cell;
        }
    }
}
