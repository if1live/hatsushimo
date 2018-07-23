using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Food : Actor
    {
        public int Score { get; private set; }
        public Vec2 Position { get; private set; }
        public override ActorType Type => ActorType.Food;

        public Food(int id, Vec2 pos, int score)
        {
            this.ID = id;
            this.Position = pos;
            this.Score = score;
        }

        public override ReplicationActionPacket GenerateCreatePacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Create,
                ID = ID,
                ActorType = Type,
                Pos = Position,
                // TODO remove optional field
                TargetPos = Vec2.Zero,
                Speed = 0,
                Extra = "",
            };
        }
    }

}
