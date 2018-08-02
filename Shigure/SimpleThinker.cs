using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Optional.Unsafe;

namespace Shigure
{
    public class SimpleThinker : IThinker
    {
        readonly Connection conn;
        readonly Random rand = new Random();
        readonly APIRunner runner;

        int playerID = 0;

        const string worldID = "default";
        readonly string uuid;
        readonly string nickname;

        public SimpleThinker(Connection conn)
        {
            this.conn = conn;
            this.runner = new APIRunner(conn);

            var id = Thread.CurrentThread.ManagedThreadId;
            uuid = $"bot-simple-{id}";
            nickname = $"bot-simple-{id}";
        }

        public async Task<bool> Run()
        {
            playerID = await runner.Connect();
            Console.WriteLine($"player_id = {playerID}");

            var signUp = await runner.SignUp(uuid);
            var auth = await runner.Authentication(uuid);
            var join = await runner.WorldJoin(worldID, nickname);
            var ready = await runner.PlayerReady();

            for (var i = 0; i < 10; i++)
            {
                var remove = conn.TryRecv<ReplicationRemovePacket>();
                if (remove.HasValue)
                {
                    var p = remove.ValueOrFailure();
                    if (p.ID == playerID)
                    {
                        Console.WriteLine("dead");
                        break;
                    }
                }

                var removeBulk = conn.TryRecv<ReplicationBulkRemovePacket>();
                if (removeBulk.HasValue)
                {
                    var p = removeBulk.ValueOrFailure();
                    var dead = p.IDList.ToList().Where(id => id == playerID).Any();
                    if (dead)
                    {
                        Console.WriteLine("dead - bulk");
                        break;
                    }
                }

                float x = (float)(rand.NextDouble() - 0.5) * Config.RoomWidth;
                float y = (float)(rand.NextDouble() - 0.5) * Config.RoomHeight;
                runner.Move(x, y);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            var leave = await runner.WorldLeave();
            runner.Disconnect();
            conn.Shutdown();

            return true;
        }
    }
}
