using System.Numerics;
using Hatsushimo;
using Hatsushimo.Packets;

namespace Mikazuki
{
    public class Projectile : Actor
    {
        public override ActorType Type => ActorType.Projectile;

        // 죽창을 던진 유저가 자신의 창을 맞고 자살하는걸 방지하고 싶다
        public int OwnerID { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }
        public float LifeTime { get; private set; }
        public float MoveTime { get; private set; }

        // 죽창은 적당히 날아간후 멈춘다
        // 발사 패킷 하나로 죽창의 이동과 정지까지 확실히 구현하기 위해
        // 시작과 정지 위치까지 관리한다
        public Vector2 InitialPosition { get; private set; }
        public Vector2 FinalPosition { get; private set; }

        public bool Alive { get { return (LifeTime >= 0); } }

        public Projectile(int id, int ownerID, Vector2 pos, Vector2 dir)
        {
            this.ID = id;
            this.OwnerID = ownerID;
            this.Position = pos;
            this.Direction = dir;

            this.MoveTime = Config.ProjectileMoveTime;
            this.LifeTime = Config.ProjectileLifeTime;
            this.Speed = Config.ProjectileSpeed;

            InitialPosition = pos;
            FinalPosition = InitialPosition + dir * Speed * MoveTime;
        }

        public void Update(float dt)
        {
            // TODO 생존시간같은거 관리는 async로 넘길수 있을거같은데
            this.LifeTime -= dt;
            this.MoveTime -= dt;

            if (this.MoveTime >= 0)
            {
                var diff = Direction * Speed * dt;
                var nextPos = Position + diff;

                var w = Config.RoomWidth;
                var h = Config.RoomHeight;
                nextPos = VectorHelper.FilterPosition(nextPos, w, h);
                this.Position = nextPos;
            }
            else
            {
                this.Position = FinalPosition;
            }
        }

        public ReplicationCreateProjectilePacket GenerateCreatePacket()
        {
            var status = new ProjectileStatus()
            {
                ID = ID,
                Position = Position,
                FinalPosition = FinalPosition,
                LifeTimeMillis = (short)(LifeTime * 1000),
                MoveTimeMillis = (short)(MoveTime * 1000),
            };
            return new ReplicationCreateProjectilePacket() { status = status };
        }
    }
}
