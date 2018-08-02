using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Optional.Unsafe;
using System.Collections.Generic;

namespace Shigure
{
    public class SimpleThinker : BaseThinker
    {
        public SimpleThinker(Connection conn)
        : base(conn, "simple")
        {
        }

        public override async Task<bool> Run()
        {
            var playerID = await ConnectAndLogin();
            Console.WriteLine($"player_id = {playerID}");

            var join = await WorldJoin();
            var ready = await runner.PlayerReady();

            for (var i = 0; i < 10; i++)
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
                await Task.Delay(TimeSpan.FromSeconds(2));
            }


            var leave = await WorldLeave();
            LogoutAndDisconnect();

            return true;
        }
    }
}
