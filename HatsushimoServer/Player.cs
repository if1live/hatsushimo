using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Player : Actor
    {
        public Session Session { get; private set; }

        public Vec2 Position { get; private set; }
        public Vec2 TargetPosition { get; set; }
        public float Speed { get; set; }

        public override ActorType Type => ActorType.Player;

        public int Score { get; private set; }

        public Player(int id, Session session)
        {
            this.ID = id;
            this.Session = session;

            Score = 0;
            SetPosition(Vec2.Zero);
            TargetPosition = Vec2.Zero;
        }

        public void SetPosition(Vec2 pos)
        {
            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            Position = VectorHelper.FilterPosition(pos, w, h);
        }

        public void UpdateMove(float dt)
        {
            var limit = dt * Speed;
            var nextPos = VectorHelper.MoveToTarget(Position, TargetPosition, limit);
            SetPosition(nextPos);
        }

        public void GainScore(int s)
        {
            this.Score += s;
        }

        public override ReplicationActionPacket GenerateCreatePacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Create,
                ID = ID,
                ActorType = Type,
                Pos = Position,
                TargetPos = TargetPosition,
                Speed = Speed,
                Extra = Session.Nickname,
            };
        }
    }
}
