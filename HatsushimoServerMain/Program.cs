using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using HatsushimoShared;
using HatsushimoServer;

namespace HatsushimoServerMain
{
    class Program
    {
        static void Main(string[] args)
        {
            var svr = new Server();
            svr.Run(args);
        }
    }
}
