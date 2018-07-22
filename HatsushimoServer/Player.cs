using Hatsushimo;
using Hatsushimo.Types;

namespace HatsushimoServer
{
    public class Player
    {
        public int ID;
        public string RoomID;
        public string Nickname;

        public Vec2 Position { get; private set; }
        public Vec2 Direction { get; private set; }
        public float Speed { get; private set; }
        public int Score;

        public Player(int id) {
            this.ID = id;
        }

        public void Reset()
        {
            Position = Vec2.Zero;
            Direction = Vec2.Zero;
            Speed = 0;
            Score = 0;
        }

        public void SetVelocity(Vec2 dir, float speed) {
            Direction = dir;
            Speed = speed;
        }

        public void SetPosition(Vec2 pos) {
            Position = pos;
        }
    }
}
