using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Optional.Unsafe;

namespace Shigure
{
    public interface IThinker
    {
        Task<bool> Run();
    }

    public abstract class BaseThinker : IThinker
    {
        protected readonly Connection conn;
        protected readonly Random rand = new Random();
        protected readonly APIRunner runner;

        const string worldID = "default";
        readonly string uuid;
        readonly string nickname;

        protected int PlayerID { get; private set; }

        public BaseThinker(Connection conn, string botType)
        {
            this.conn = conn;
            this.runner = new APIRunner(conn);

            var id = Thread.CurrentThread.ManagedThreadId;
            uuid = $"bot-{botType}-{id}";
            nickname = $"bot-{botType}-{id}";
        }

        public abstract Task<bool> Run();

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
    }
}
