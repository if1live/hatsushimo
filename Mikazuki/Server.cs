using System;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using Hatsushimo;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Hatsushimo.Packets;
using Hatsushimo.NetChan;
using Mikazuki.NetChan;

namespace Mikazuki
{
    public class Server
    {
        public void Run(string[] args)
        {
            // initialize network stack
            var transport = new WebSocketTransportLayer();
            var session = new SessionLayer(transport);

            NetworkStack.Register(transport);
            NetworkStack.Register(session);

            var _2 = GameService.Instance;

            var port = Config.ServerPort;
            //var wssv = new WebSocketServer($"ws://127.0.0.1:{port}");
            var wssv = new WebSocketServer($"ws://0.0.0.0:{port}");
            wssv.AddWebSocketService<WebSocketSession>("/game");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
