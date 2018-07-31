using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Food : Actor
    {
        public Vec2 Position { get; private set; }
        public override ActorType Type => ActorType.Food;

        public Food(int id, Vec2 pos)
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
            return new ReplicationCreateFoodPacket() { status = status };
        }
    }

}
