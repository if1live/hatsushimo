using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;

namespace Shigure
{
    // 맵을 적당히 돌아다니는 봇
    public class WanderThinker : BaseThinker
    {
        readonly float lifetime;

        public WanderThinker(Connection conn, float lifetime)
        : base(conn, "wander")
        {
            this.lifetime = lifetime;
        }

        public override async Task<bool> Run()
        {
            var playerID = await ConnectAndLogin();
            Console.WriteLine($"player_id = {playerID}");

            var join = await WorldJoin();
            var ready = await runner.PlayerReady();

            var remain = lifetime;
            while (remain > 0)
            {
                var removeIDList = runner.GetRemovedIDs();
                var dead = removeIDList.Where(id => id == playerID).Any();
                if (dead)
                {
                    Console.WriteLine("dead - bulk");
                    break;
                }

                float x = (float)(rand.NextDouble() - 0.5) * Config.RoomWidth;
                float y = (float)(rand.NextDouble() - 0.5) * Config.RoomHeight;
                runner.Move(x, y);

                float interval = (float)rand.NextDouble() * 5;
                remain -= interval;
                await Task.Delay(TimeSpan.FromSeconds(interval));
            }

            var leave = await WorldLeave();
            LogoutAndDisconnect();

            return true;
        }
    }
}
