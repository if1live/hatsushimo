using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Projectile : Actor
    {
        public override ActorType Type => ActorType.Projectile;

        public Vec2 Position { get; private set; }
        public Vec2 Direction { get; private set; }
        public float Lifetime { get; private set; }
        public float Speed { get; private set; }

        public Projectile(int id, Vec2 pos, Vec2 dir)
        {
            this.ID = id;
            this.Position = pos;
            this.Direction = dir;

            this.Lifetime = 3;
            this.Speed = 10;
        }

        public bool DecreaseLifetime(float dt)
        {
            this.Lifetime -= dt;
            if (Lifetime <= 0) { return false; }
            return true;
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
