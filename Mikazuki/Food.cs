using System.Numerics;
using Hatsushimo;
using Hatsushimo.Packets;

namespace Mikazuki
{
    public class Food : Actor
    {
        public Vector2 Position { get; private set; }
        public override ActorType Type => ActorType.Food;

        public Food(int id, Vector2 pos)
        {
            this.ID = id;
            this.Position = pos;
        }

        public ReplicationCreateFoodPacket GenerateCreatePacket()
        {
            var status = new FoodStatus()
            {
                ID = ID,
                Pos = Position,
            };
            return new ReplicationCreateFoodPacket(status);
        }
    }

}
