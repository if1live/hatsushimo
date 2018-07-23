using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public abstract class Actor
    {
        public int ID { get; protected set; }
        public abstract ActorType Type { get; }

        public abstract ReplicationActionPacket GenerateCreatePacket();
        public ReplicationActionPacket GenerateRemovePacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Remove,
                ID = ID,
                ActorType = Type,
                Pos = Vec2.Zero,
                TargetPos = Vec2.Zero,
                Speed = 0,
                Extra = "",
            };
        }
    }
}
