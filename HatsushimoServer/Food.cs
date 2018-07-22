using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Food
    {
        public int ID { get; private set; }
        public int Score { get; private set; }
        public Vec2 Position { get; private set; }

        public Food(int id, Vec2 pos, int score)
        {
            this.ID = id;
            this.Position = pos;
            this.Score = score;
        }

        public ReplicationActionPacket MakeCreatePacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Create,
                ID = ID,
                ActorType = ActorType.Food,
                Pos = Position,
                // TODO remove optional field
                Dir = Vec2.Zero,
                Speed = 0,
                Extra = null,
            };
        }

        public ReplicationActionPacket MakeRemovePacket()
        {
            return ReplicationActionPacket.MakeRemovePacket(ID);
        }
    }

}
