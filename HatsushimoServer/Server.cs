using System;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using HatsushimoShared;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HatsushimoServer
{
    class GameSession : WebSocketBehavior
    {
        readonly PacketFactory factory = PacketFactory.Create();

        protected override void OnOpen()
        {
            GameService.Instance.Sessions = Sessions;
        }

        protected override void OnClose(CloseEventArgs e)
        {
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var bytes = e.RawData;
            var packet = factory.Deserialize(bytes);
            HandlePacket(packet);
        }

        void HandlePacket(IPacket packet)
        {
            var service = GameService.Instance;
            switch (packet.Type)
            {
                case PacketType.Ping:
                    service.HandlePing(this, (PingPacket)packet);
                    break;

                case PacketType.Connect:
                    service.HandleConnect(this, (ConnectPacket)packet);
                    break;

                case PacketType.Disconnect:
                    service.HandleDisconnect(this, (DisconnectPacket)packet);
                    break;

                default:
                    Console.WriteLine($"packet handler not exist: {packet.Type}");
                    break;
            }
        }

        public void SendPacket(IPacket packet)
        {
            Send(factory.Serialize(packet));
        }

        public void Broadcast(IPacket packet)
        {
            var data = factory.Serialize(packet);
            var sessions = Sessions.Sessions.Where(s => s.ID != ID);
            foreach(var s in sessions) {
                Sessions.SendTo(data, s.ID);
            }
        }

        public void BroadcastAll(IPacket packet)
        {
            Sessions.Broadcast(factory.Serialize(packet));
        }

        public void SendPacketAsync(IPacket packet, Action<bool> completed)
        {
            // TODO async 사용시 예외 발생
            // System.PlatformNotSupportedException: Operation is not supported on this platform.
            // .Net core의 문제인가? OS의 문제인가? 라이브러리의 문제인가?
            SendAsync(factory.Serialize(packet), completed);
        }

        public void BroadcastAsync(IPacket packet, Action<bool> completed) {
            var data = factory.Serialize(packet);
            var sessions = Sessions.Sessions.Where(s => s.ID != ID);
            foreach(var s in sessions) {
                Sessions.SendToAsync(data, s.ID, completed);
            }
        }

        public void BroadcastAllAsync(IPacket packet, Action completed) {
            var data = factory.Serialize(packet);
            Sessions.BroadcastAsync(data, completed);
        }
    }

    public class Server
    {
        public void Run(string[] args)
        {
            var game = GameService.Instance;
            game.StartUpdateLoop();

            var port = Config.ServerPort;
            var wssv = new WebSocketServer($"ws://127.0.0.1:{port}");
            wssv.AddWebSocketService<GameSession>("/game");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
