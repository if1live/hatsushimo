using System;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using HatsushimoServer.NetChan;
using NLog;

namespace HatsushimoServer
{
    public class Player : Actor
    {
        static readonly Logger log = LogManager.GetLogger("Player");

        public Session Session { get; private set; }

        public Vec2 Position { get; private set; }
        public Vec2 TargetPosition { get; set; }
        public float Speed { get; set; }

        // 방향을 sin/cos로 표현할수 있도록 +x를 기준으로 잡는다
        // 공격을 구현할 경우 방향을 알아야한다
        public Vec2 Direction { get; private set; } = new Vec2(1, 0);

        public override ActorType Type => ActorType.Player;

        public int Score
        {
            get
            {
                var score = 0;
                score += score_food;
                score += score_kill * 100;
                return score;
            }
        }

        int score_food = 0;
        int score_kill = 0;

        // skill은 2개면 되겠지?
        public bool SkillPrimary { get; private set; } = true;
        public bool SKillSecondary { get; private set; } = true;

        public Player(int id, Session session)
        {
            this.ID = id;
            this.Session = session;

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

                log.Info($"pos={next.X}\t{next.Y}");
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

        // TODO score 계산 공식은 레벨과 묶여서 복잡해질 가능성이 있으니 분리하기
        public void GainFoodScore(int score)
        {
            score_food += score;
        }
        public void GainKillScore(int score)
        {
            score_kill += score;
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
            log.Info($"use skill primary: id={ID}");
            RunSkillPrimaryCoolTimer();

            // TODO 적당히 공격
        }

        void UseSKillSecondary()
        {
            if (!SKillSecondary) { return; }
            log.Info($"use skill secondary: id={ID}");
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

        public ReplicationCreatePlayerPacket GenerateCreatePacket()
        {
            var status = new PlayerStatus()
            {
                ID = ID,
                Pos = Position,
                TargetPos = TargetPosition,
                Speed = Speed,
                Nickname = Session.Nickname,
            };
            return new ReplicationCreatePlayerPacket() { status = status };
        }
    }
}
