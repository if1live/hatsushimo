using Hatsushimo.Packets;

namespace Mikazuki
{
    public abstract class Actor
    {
        public int ID { get; protected set; }
        public abstract ActorType Type { get; }

        public ReplicationRemovePacket GenerateRemovePacket()
        {
            return new ReplicationRemovePacket()
            {
                ID = ID,
            };
        }

        // TOOD generic 잘 쓰면 create packet 를 인터페이스로 분리할수 있을거같다
    }
}
