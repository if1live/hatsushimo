using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;

namespace Shigure.Agents
{
    // 맵을 적당히 돌아다니는 봇
    // 이동 테스트용
    public class WanderAgent : BaseAgent
    {
        readonly float lifetime;
        float remainLifetime;

        public WanderAgent(Connection conn, float lifetime)
        : base(conn, "wander")
        {
            this.lifetime = lifetime;
            this.remainLifetime = lifetime;
        }

        protected override async Task<bool> Think(List<int> removeIDList)
        {
            if (remainLifetime < 0)
            {
                return false;
            }

            float x = (float)(rand.NextDouble() - 0.5) * Config.RoomWidth;
            float y = (float)(rand.NextDouble() - 0.5) * Config.RoomHeight;
            runner.Move(x, y);

            float interval = (float)rand.NextDouble() * 5;
            remainLifetime -= interval;
            await Task.Delay(TimeSpan.FromSeconds(interval));
            return true;
        }
    }
}
