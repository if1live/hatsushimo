using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shigure.Agents
{
    public abstract class BaseAgent : IAgent
    {
        protected readonly Connection conn;
        protected readonly Random rand = new Random();
        protected readonly APIRunner runner;

        const string worldID = "default";
        readonly string uuid;
        readonly string nickname;

        protected int PlayerID { get; private set; }

        public BaseAgent(Connection conn, string botType)
        {
            this.conn = conn;
            this.runner = new APIRunner(conn);

            var id = Thread.CurrentThread.ManagedThreadId;
            uuid = $"bot-{botType}-{id}";
            nickname = $"bot-{botType}-{id}";
        }

        public async Task<bool> Run()
        {
            var playerID = await ConnectAndLogin();
            Console.WriteLine($"player_id = {playerID}");

            var join = await WorldJoin();
            var ready = await runner.PlayerReady();

            while (true)
            {
                // 죽었으면 루프 탈출
                var removeIDList = runner.GetRemovedIDs();
                if (DoesDead(playerID, removeIDList))
                {
                    Console.WriteLine("dead - bulk");
                    break;
                }

                // agent가 알아서 재주껏 행동한다
                var task = Think(removeIDList);
                task.Wait();
                if (task.Result == false)
                {
                    break;
                }
            }

            var leave = await WorldLeave();
            LogoutAndDisconnect();
            return true;
        }

        protected abstract Task<bool> Think(List<int> removeIDList);

        public async Task<int> ConnectAndLogin()
        {
            PlayerID = await runner.Connect();
            var signUp = await runner.SignUp(uuid);
            var auth = await runner.Authentication(uuid);
            return PlayerID;
        }

        public async Task<bool> WorldJoin()
        {
            var join = await runner.WorldJoin(worldID, nickname);
            return true;
        }

        public async Task<bool> WorldLeave()
        {
            var leave = await runner.WorldLeave();
            return true;
        }

        public void LogoutAndDisconnect()
        {
            runner.Disconnect();
            conn.Shutdown();
        }

        public bool DoesDead(int playerID, List<int> ids)
        {
            return ids.Where(id => id == playerID).Any();
        }
    }
}
