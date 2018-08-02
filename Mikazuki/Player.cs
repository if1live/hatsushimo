using System;
using System.Numerics;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Mikazuki.NetChan;
using NLog;

namespace Mikazuki
{
    public class Player : Actor, IRankable
    {
        static readonly Logger log = LogManager.GetLogger("Player");

        public Session Session { get; private set; }
        public PlayerMode Mode { get; private set; }

        public Vector2 Position { get; private set; }
        public Vector2 TargetPosition { get; set; }
        public float Speed { get; set; }

        // 방향을 sin/cos로 표현할수 있도록 +x를 기준으로 잡는다
        // 공격을 구현할 경우 방향을 알아야한다
        public Vector2 Direction { get; private set; } = new Vector2(1, 0);

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

        internal readonly PlayerPacketFactory Packets;

        public Player(int id, Session session, PlayerMode mode)
        {
            this.ID = id;
            this.Session = session;
            this.Mode = mode;

            SetPosition(Vector2.Zero);
            TargetPosition = Vector2.Zero;

            Packets = new PlayerPacketFactory(this);
        }

        public void SetPosition(Vector2 pos)
        {
            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            var next = VectorHelper.FilterPosition(pos, w, h);
            var prev = Position;

            if (next != prev)
            {
                var diff = next - prev;
                Direction = Vector2.Normalize(diff);

                //log.Info($"pos={next.X}\t{next.Y}");
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
    }

    class PlayerPacketFactory
    {
        readonly Player player;
        internal PlayerPacketFactory(Player p)
        {
            this.player = p;
        }

        public ReplicationCreatePlayerPacket Create()
        {
            var status = new PlayerStatus()
            {
                ID = player.ID,
                Pos = player.Position,
                TargetPos = player.TargetPosition,
                Speed = player.Speed,
                Nickname = player.Session.Nickname,
            };
            return new ReplicationCreatePlayerPacket(status);
        }
    }
}
