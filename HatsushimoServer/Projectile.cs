using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Projectile : Actor
    {
        public override ActorType Type => ActorType.Projectile;

        // 죽창을 던진 유저가 자신의 창을 맞고 자살하는걸 방지하고 싶다
        public int OwnerID { get; private set; }
        public Vec2 Position { get; private set; }
        public Vec2 Direction { get; private set; }
        public float Lifetime { get; private set; }
        public float Speed { get; private set; }

        public bool Alive { get { return (Lifetime >= 0); } }

        public Projectile(int id, int ownerID, Vec2 pos, Vec2 dir)
        {
            this.ID = id;
            this.OwnerID = ownerID;
            this.Position = pos;
            this.Direction = dir;

            this.Lifetime = Config.ProjectileLifetime;
            this.Speed = Config.ProjectileSpeed;
        }

        public bool DecreaseLifetime(float dt)
        {
            this.Lifetime -= dt;
            if (Lifetime <= 0) { return false; }
            return true;
        }

        public void UpadteMove(float dt)
        {
            var diff = Direction * Speed * dt;
            var nextPos = Position + diff;

            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            nextPos = VectorHelper.FilterPosition(nextPos, w, h);
            this.Position = nextPos;
        }

        public ReplicationCreateProjectilePacket GenerateCreatePacket()
        {
            var status = new ProjectileStatus()
            {
                ID = ID,
                Position = Position,
                Direction = Direction,
                LifetimeMillis = (short)(Lifetime * 1000),
                Speed = Speed,
            };
            return new ReplicationCreateProjectilePacket() { status = status };
        }
    }
}
