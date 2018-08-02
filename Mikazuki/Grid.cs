using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;

namespace Mikazuki
{
    public struct CellCoord
    {
        public int X;
        public int Y;

        // 인접한 셀
        public static readonly CellCoord[] Offsets = new CellCoord[]
        {
            new CellCoord(-1, -1),
            new CellCoord(-1, 0),
            new CellCoord(-1, +1),

            new CellCoord(0, -1),
            new CellCoord(0, 0),
            new CellCoord(0, +1),

            new CellCoord(+1, -1),
            new CellCoord(+1, 0),
            new CellCoord(+1, +1),
        };

        public CellCoord(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static CellCoord operator +(CellCoord a, CellCoord b)
        {
            var x = a.X + b.X;
            var y = a.Y + b.Y;
            return new CellCoord(x, y);
        }
    }

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
        public readonly int CellSize;

        readonly int halfWidth;
        readonly int halfHeight;

        public readonly int HorizontalCount;
        public readonly int VerticalCount;

        public int MinCellX { get { return -(HorizontalCount / 2); } }
        public int MaxCellX { get { return HorizontalCount / 2 - 1; } }
        public int MinCellY { get { return -(VerticalCount / 2); } }
        public int MaxCellY { get { return VerticalCount / 2 - 1; } }


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

            this.CellSize = size;
            this.halfWidth = hx;
            this.halfHeight = hy;

            this.HorizontalCount = (halfWidth / CellSize) * 2;
            this.VerticalCount = (halfHeight / CellSize) * 2;
            var gridCount = HorizontalCount * VerticalCount;
            cells = new List<Cell<T>>(gridCount);
            for (var i = 0; i < gridCount; i++) { cells.Add(new Cell<T>()); }
        }

        int ToCellIndex(int x, int y)
        {
            if (x < MinCellX)
            {
                var msg = $"min cell x={MinCellX}, input={x}";
                throw new IndexOutOfRangeException(msg);
            }
            if (x > MaxCellX)
            {
                var msg = $"max cell x={MaxCellX}, input={x}";
                throw new IndexOutOfRangeException(msg);
            }
            if (y < MinCellY)
            {
                var msg = $"min cell y={MinCellY}, input={y}";
                throw new IndexOutOfRangeException(msg);
            }
            if (y > MaxCellY)
            {
                var msg = $"max cell y={MaxCellY}, input={y}";
                throw new IndexOutOfRangeException(msg);
            }

            var cellX = HorizontalCount / 2 + x;
            var cellY = VerticalCount / 2 + y;
            return cellX * HorizontalCount + cellY;
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
                var idx = floor / CellSize;
                return idx;
            }
            else
            {
                var absval = -v;
                var floor = (int)absval;
                var idx = floor / CellSize + 1;
                return -idx;
            }
        }
        public CellCoord GetCellCoord(Vector2 v)
        {
            var x = GetCellIndex(v.X);
            var y = GetCellIndex(v.Y);
            return new CellCoord(x, y);
        }

        public void Clear()
        {
            foreach (var sub in cells) { sub.Clear(); }
        }

        public void Add(Vector2 pos, T el)
        {
            var coord = GetCellCoord(pos);
            var cell = GetCell(coord);
            cell.Add(el);
        }

        public Cell<T> GetCell(CellCoord coord)
        {
            var subIdx = ToCellIndex(coord.X, coord.Y);
            var cell = cells[subIdx];
            return cell;
        }

        public IEnumerable<CellCoord> FilterCellCoords(IEnumerable<CellCoord> iter)
        {
            return iter.Where(coord => coord.X >= MinCellX)
                .Where(coord => coord.X <= MaxCellX)
                .Where(coord => coord.Y >= MinCellY)
                .Where(coord => coord.Y <= MaxCellY);
        }

        public int Count { get { return cells.Sum(cell => cell.Count); } }
    }
}
