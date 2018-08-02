using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Optional.Unsafe;
using System.Collections.Generic;

namespace Shigure.Agents
{
    public class SimpleAgent : BaseAgent
    {
        public SimpleAgent(Connection conn)
        : base(conn, "simple")
        {
        }

        // TODO
        protected override async Task<bool> Think(List<int> removeIDList)
        {
            float x = (float)(rand.NextDouble() - 0.5) * Config.RoomWidth;
            float y = (float)(rand.NextDouble() - 0.5) * Config.RoomHeight;
            runner.Move(x, y);
            await Task.Delay(TimeSpan.FromSeconds(2));
            return true;
        }
    }
}
