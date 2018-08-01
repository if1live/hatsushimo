using System.Collections;
using System.Collections.Generic;

namespace Hatsushimo.Utils
{
    public class IDPool
    {
        const int ID_PLAYER_FIRST = 1;
        const int ID_PLAYER_RANGE = 100000;

        const int ID_FOOD_FIRST = 100001;
        const int ID_FOOD_RANGE = 100000;

        const int ID_PROJECTILE_FIRST = 200001;
        const int ID_PROJECTILE_RANGE = 100000;


        readonly Stack<int> pool = new Stack<int>();
        readonly int first;
        readonly int last;
        readonly int expand;
        int curr;

        public IDPool(int first, int range, int expand)
        {
            this.first = first;
            this.last = first + range - 1;
            this.curr = first;
            this.expand = expand;
        }

        public int Next()
        {
            if (pool.Count == 0)
            {
                ExpandPool(expand);
            }

            return pool.Pop();
        }

        void ExpandPool(int size)
        {
            while (pool.Count < size)
            {
                pool.Push(curr);
                curr += 1;
            }
        }

        public void Release(int id)
        {
            pool.Push(id);
        }

        public static IDPool MakeSessionID()
        {
            return new IDPool(ID_PLAYER_FIRST, ID_FOOD_RANGE, 1);
        }

        public static IDPool MakeFoodID()
        {
            return new IDPool(ID_FOOD_FIRST, ID_FOOD_RANGE, 1);
        }

        public static IDPool MakeProjectileID()
        {
            return new IDPool(ID_PROJECTILE_FIRST, ID_PROJECTILE_RANGE, 1);
        }
    }
}
