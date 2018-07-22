using System.Collections;
using System.Collections.Generic;

namespace HatsushimoShared
{
    public class IDGenerator
    {
        const int ID_PLAYER_FIRST = 1;
        const int ID_PLAYER_RANGE = 100000;

        const int ID_FOOD_FIRST = 100001;
        const int ID_FOOD_RANGE = 100000;

        const int ID_PROJECTILE_FIRST = 200001;
        const int ID_PROJECTILE_RANGE = 100000;

        // TODO id의 범위를 크게 잡고 재탕되기전에 객체가 쓰일거라고 가정한 코드이다
        // 단점: 생각이상으로 객체가 살아있는 경우 id 재사용의 가능성 있음
        // 1. id pool. 단점: 사용한 id를 풀에 넣는 코드가 필요해진다
        // 2. uuid. 단점: id가 길어진다
        public static IEnumerable<int> Make(int firstID, int range)
        {
            var nextID = firstID;

            while (true)
            {
                var retval = nextID;
                yield return retval;

                nextID += 1;
                if (nextID >= firstID + range)
                {
                    nextID = firstID;
                }
            }
        }

        public static IEnumerable<int> MakePlayerID()
        {
            return Make(ID_PLAYER_FIRST, ID_FOOD_RANGE);
        }

        public static IEnumerable<int> MakeFoodID()
        {
            return Make(ID_FOOD_FIRST, ID_FOOD_RANGE);
        }

        public static IEnumerable<int> MakeProjectileID()
        {
            return Make(ID_PROJECTILE_FIRST, ID_PROJECTILE_RANGE);
        }
    }
}
