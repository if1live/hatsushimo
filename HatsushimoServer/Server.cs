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

namespace HatsushimoServer
{
    public class Server
    {
        public void Run(string[] args)
        {
            var transportLayer = WebSocketTransportLayer.Layer;
            transportLayer.StartSendLoop();

            var _ = GameService.Instance;


            var port = Config.ServerPort;
            var wssv = new WebSocketServer($"ws://127.0.0.1:{port}");
            wssv.AddWebSocketService<WebSocketSession>("/game");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
