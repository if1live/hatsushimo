using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using HatsushimoShared;

namespace HatsushimoServer
{
    class GameService : WebSocketBehavior
    {
        readonly PacketFactory factory = PacketFactory.Create();

        protected override void OnMessage(MessageEventArgs e)
        {
            var bytes = e.RawData;
            var packet = factory.Deserialize(bytes);

            // TODO packet handler
            if(packet.Type == PacketType.Ping) {
                OnPingPacket((PingPacket)packet);
            }
        }

        void OnPingPacket(PingPacket p) {
            // TODO send async
            Console.WriteLine($"ping packet received : {p.millis}");
            SendPacket(p);
        }

        void SendPacket(IPacket packet) {
            Send(factory.Serialize(packet));
        }
    }

    public class Server
    {
        public void Run(string[] args)
        {
            var port = Config.ServerPort;
            var wssv = new WebSocketServer($"ws://127.0.0.1:{port}");
            wssv.AddWebSocketService<GameService>("/game");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
