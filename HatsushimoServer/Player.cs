using System;
using System.Threading.Tasks;
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

        // 방향을 sin/cos로 표현할수 있도록 +x를 기준으로 잡는다
        // 공격을 구현할 경우 방향을 알아야한다
        public Vec2 Direction { get; private set; } = new Vec2(1, 0);

        public override ActorType Type => ActorType.Player;

        public int Score { get; private set; }

        // skill은 2개면 되겠지?
        public bool SkillPrimary { get; private set; } = true;
        public bool SKillSecondary { get; private set; } = true;

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
            var next = VectorHelper.FilterPosition(pos, w, h);
            var prev = Position;

            if (next != prev)
            {
                var diff = next - prev;
                Direction = diff.Normalize();
            }

            Position = next;
        }

        public void UpdateMove(float dt)
        {
            if (Speed == 0)
            {
                return;
            }

            var limit = dt * Speed;
            var nextPos = VectorHelper.MoveToTarget(Position, TargetPosition, limit);
            SetPosition(nextPos);
        }

        public void GainScore(int s)
        {
            this.Score += s;
        }

        async void RunSkillPrimaryCoolTimer()
        {
            if (SkillPrimary == false) { return; }

            SkillPrimary = false;

            var dueTime = TimeSpan.FromSeconds(Config.CoolTimeCommandPrimary);
            await Task.Delay(dueTime);

            SkillPrimary = true;
        }

        async void RunSkillSecondaryCoolTimer()
        {
            if (SKillSecondary == false) { return; }

            SKillSecondary = false;

            var dueTime = TimeSpan.FromSeconds(Config.CoolTimeCommandSecondary);
            await Task.Delay(dueTime);

            SKillSecondary = true;
        }

        void UseSkillPrimary()
        {
            if (!SkillPrimary) { return; }
            Console.WriteLine($"use skill primary: id={ID}");
            RunSkillPrimaryCoolTimer();

            // TODO 적당히 공격
        }

        void UseSKillSecondary()
        {
            if (!SKillSecondary) { return; }
            Console.WriteLine($"use skill secondary: id={ID}");
            RunSkillSecondaryCoolTimer();
        }

        public void UseSkill(int mode)
        {
            switch (mode)
            {
                case 1:
                    UseSkillPrimary();
                    break;
                case 2:
                    UseSKillSecondary();
                    break;
                default:
                    break;
            }
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
