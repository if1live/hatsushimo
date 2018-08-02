using System;
using WebSocketSharp;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.NetChan;
using System.IO;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shigure
{
    public class Client
    {
        IThinker CreateThinker(Connection conn, string[] args)
        {
            //IThinker thinker = new AssertThinker(conn);
            //IThinker thinker = new SimpleThinker(conn);
            IThinker thinker = new WanderThinker(conn, 30);
            return thinker;
        }

        public void Run(string[] args)
        {
            var ws = new WebSocket($"ws://127.0.0.1:{Config.ServerPort}/game");
            var conn = new Connection(ws);
            var thinker = CreateThinker(conn, args);

            ws.OnMessage += (sender, e) =>
            {
                conn.PushReceivedData(e.RawData);
            };
            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine("on close");
            };
            ws.OnError += (sender, e) =>
            {
                Console.WriteLine("on error");
            };
            ws.Connect();

            var task = thinker.Run();
            task.Wait();
        }
    }
}
