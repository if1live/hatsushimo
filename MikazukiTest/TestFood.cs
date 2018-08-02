using System;
using System.Linq;
using System.Numerics;
using Mikazuki;
using Xunit;

namespace MikazukiTest
{
    public class TestFoodLayer
    {
        [Fact]
        public void TestGetFoods()
        {
            var broadcaster = new Broadcaster(null);
            var foodlayer = new FoodManager(broadcaster);

            foodlayer.Create(new Vector2(5.600638f, -2.408553f));
            foodlayer.RefreshGrid();

            {
                var pos = new Vector2(5.600638f, -2.408553f);
                var foods = foodlayer.GetFoods(pos, 1);
                Assert.Equal(1, foods.Count());
            }

            {
                var pos = new Vector2(5.420763f, -2.279034f);
                var foods = foodlayer.GetFoods(pos, 1);
                Assert.Equal(1, foods.Count());
            }
        }
    }
}
