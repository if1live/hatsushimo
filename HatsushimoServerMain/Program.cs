using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Hatsushimo;
using HatsushimoServer;
using SharpRaven;
using SharpRaven.Data;

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
