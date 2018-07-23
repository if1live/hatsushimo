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

        public int Score;

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
            // 범위 확인
            var halfw = Config.RoomWidth * 0.5f;
            var halfh = Config.RoomHeight * 0.5f;
            var x = pos[0];
            var y = pos[1];
            if (x < -halfw) { x = -halfw; }
            if (x > halfw) { x = halfw; }
            if (y < -halfh) { y = -halfh; }
            if (y > halfh) { y = halfh; }

            Position = new Vec2(x, y);
        }

        public void UpdateMove(float dt)
        {
            var diff = TargetPosition - Position;

            // delta로 이동하면 목표지점 근처에서 진동하는 현상이 발생할수 있다
            // dt동안 이동할수 있는 곳에 목표가 있으면 순간이동하기
            var limit = dt * Speed;
            var sqrLimit = limit * limit;
            var sqrDistance = diff.SqrMagnitude;
            if(sqrLimit > sqrDistance)
            {
                SetPosition(TargetPosition);
                return;
            }

            var dir = diff.Normalize();
            var dx = dir[0] * Speed * dt;
            var dy = dir[1] * Speed * dt;
            var delta = new Vec2(dx, dy);
            var nextPos = Position + delta;
            SetPosition(nextPos);
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
