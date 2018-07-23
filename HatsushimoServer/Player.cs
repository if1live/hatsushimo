using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Player : Actor
    {
        public Session Session { get; private set; }
        public string RoomID;
        public string Nickname;

        public Vec2 Position { get; private set; }
        public Vec2 Direction { get; private set; }
        public float Speed { get; private set; }
        public override ActorType Type => ActorType.Player;

        public int Score;

        public Player(int id, Session session)
        {
            this.ID = id;
            this.Session = session;
            this.RoomID = null;

            Reset();
        }

        public void Reset()
        {
            Nickname = "[Blank]";
            Score = 0;
            SetVelocity(Vec2.Zero, 0);
            SetPosition(Vec2.Zero);
        }

        public void SetVelocity(Vec2 dir, float speed)
        {
            Direction = dir;
            Speed = speed;
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

        public void MoveDelta(float dx, float dy)
        {
            var x = Position[0] + dx;
            var y = Position[1] + dy;
            SetPosition(new Vec2(x, y));
        }

        public override ReplicationActionPacket GenerateCreatePacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Create,
                ID = ID,
                ActorType = Type,
                Pos = Position,
                Dir = Direction,
                Speed = Speed,
                Extra = Nickname,
            };
        }
    }
}
