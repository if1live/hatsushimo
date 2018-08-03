using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using NLog;

namespace Mikazuki
{

    public class FoodManager
    {
        static readonly Logger log = LogManager.GetLogger("FoodLayer");

        readonly Grid<Food> grid = new Grid<Food>(Config.CellSize, Config.RoomWidth / 2, Config.RoomHeight / 2);
        // TODO room id를 seed로 쓴다면 버그 재연이 쉬울거같다
        readonly Random rand = new Random();
        readonly IDPool foodIDPool = IDPool.MakeFoodID();
        readonly Broadcaster broadcaster;
        readonly FoodPacketFactory packetFactory = new FoodPacketFactory();

        List<Food> foods = new List<Food>();

        public Grid<Food> Grid { get { return grid; } }

        public FoodManager(Broadcaster broadcaster)
        {
            this.broadcaster = broadcaster;
        }

        public void Spawn(int count)
        {
            if (count == 0) { return; }
            for (var i = 0; i < count; i++)
            {
                var pos = GenerateRandomPosition();
                var food = Create(pos);
                BroadcastFoodCreate(food);
            }
            RefreshGrid();
        }

        public Food Create(Vector2 pos)
        {
            var id = foodIDPool.Next();
            var food = new Food(id, pos);
            foods.Add(food);
            return food;
        }

        public void RefreshGrid()
        {
            grid.Clear();
            foreach (var food in foods)
            {
                grid.Add(food.Position, food);
            }
        }

        public Vector2 GenerateRandomPosition()
        {
            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            var x = (float)(rand.NextDouble() - 0.5) * w;
            var y = (float)(rand.NextDouble() - 0.5) * h;
            return new Vector2(x, y);
        }

        public void Update()
        {
            GenerateFoodLoop();
        }

        // 음식 생성. 맵에 어느정도의 먹을게 남아있도록 하는게 목적
        // TODO 조금 더 그럴싸하게 만들기
        void GenerateFoodLoop()
        {
            var requiredFoodCount = Config.FoodCount - foods.Count;
            Spawn(requiredFoodCount);
        }

        void BroadcastFoodCreate(Food food)
        {
            var packet = packetFactory.Create(food);
            broadcaster.BroadcastLazy(food.Position, packet);
        }

        void BroadcastFoodRemove(Food food)
        {
            var packet = new ReplicationRemovePacket(food.ID);
            broadcaster.BroadcastLazy(food.Position, packet);
            log.Info($"sent food remove packet: {packet.ID}");
        }

        public IEnumerable<FoodStatus> StatusList
        {
            get
            {
                return foods.Select(f => new FoodStatus()
                {
                    ID = f.ID,
                    Pos = f.Position,
                });
            }
        }

        public IEnumerable<Food> GetFoods(Vector2 pos, float range)
        {
            var cellcoord = grid.GetCellCoord(pos);
            var cellcoords = grid.FilterCellCoords(
                CellCoord.Offsets.Select(offset => offset + cellcoord)
            );
            var cells = cellcoords.Select(coord => grid.GetCell(coord));
            var foods = cells.SelectMany(cell => cell.ToArray())
                .Where(food => VectorHelper.IsInRange(food.Position, pos, range));
            return foods;
        }

        public void Remove(IEnumerable<int> ids)
        {
            if (ids.Count() < 0) { return; }

            var removeFoods = from food in foods
                              where ids.Contains(food.ID)
                              select food;
            foreach (var food in removeFoods)
            {
                foodIDPool.Release(food.ID);
                BroadcastFoodRemove(food);
            }

            var existFoods = from food in foods
                             where ids.Contains(food.ID) == false
                             select food;
            foods = existFoods.ToList();
            RefreshGrid();
        }
    }

    class FoodPacketFactory
    {
        public ReplicationCreateFoodPacket Create(Food food)
        {
            var status = new FoodStatus()
            {
                ID = food.ID,
                Pos = food.Position,
            };
            return new ReplicationCreateFoodPacket(status);
        }
    }
}
