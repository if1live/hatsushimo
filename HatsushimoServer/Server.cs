using System;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using Hatsushimo;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hatsushimo.Packets;
using Hatsushimo.NetChan;

namespace HatsushimoServer
{
    class GameSession : WebSocketBehavior
    {
        readonly PacketCodec codec = MyPacketCodec.Create();

        protected override void OnOpen()
        {
            GameService.Instance.Sessions = Sessions;
        }

        protected override void OnClose(CloseEventArgs e)
        {
            // 연결 종료도 서버에서는 패킷처럼 받는다
            var p = new DisconnectPacket();
            HandlePacket(p);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var bytes = e.RawData;
            var packet = codec.Decode(bytes);
            HandlePacket(packet);
        }

        void HandlePacket(IPacket packet)
        {
            var service = GameService.Instance;
            switch ((PacketType)packet.Type)
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

                case PacketType.RoomJoinReq:
                    service.HandleRoomJoinReq(this, (RoomJoinRequestPacket)packet);
                    break;

                case PacketType.RoomLeave:
                    service.HandleRoomLeave(this, (RoomLeavePacket)packet);
                    break;

                case PacketType.PlayerReady:
                    service.HandlePlayerReady(this, (PlayerReadyPacket)packet);
                    break;

                case PacketType.InputCommand:
                    service.HandleInputCommand(this, (InputCommandPacket)packet);
                    break;

                case PacketType.InputMove:
                    service.HandleInputMove(this, (InputMovePacket)packet);
                    break;

                default:
                    Console.WriteLine($"packet handler not exist: {packet.Type}");
                    break;
            }
        }

        public void SendPacket(IPacket packet)
        {
            Send(codec.Encode(packet));
        }

        public void Broadcast(IPacket packet)
        {
            var data = codec.Encode(packet);
            var sessions = Sessions.Sessions.Where(s => s.ID != ID);
            foreach (var s in sessions)
            {
                Sessions.SendTo(data, s.ID);
            }
        }

        public void BroadcastAll(IPacket packet)
        {
            Sessions.Broadcast(codec.Encode(packet));
        }

        public void SendPacketAsync(IPacket packet, Action<bool> completed)
        {
            // TODO async 사용시 예외 발생
            // System.PlatformNotSupportedException: Operation is not supported on this platform.
            // .Net core의 문제인가? OS의 문제인가? 라이브러리의 문제인가?
            SendAsync(codec.Encode(packet), completed);
        }

        public void BroadcastAsync(IPacket packet, Action<bool> completed)
        {
            var data = codec.Encode(packet);
            var sessions = Sessions.Sessions.Where(s => s.ID != ID);
            foreach (var s in sessions)
            {
                Sessions.SendToAsync(data, s.ID, completed);
            }
        }

        public void BroadcastAllAsync(IPacket packet, Action completed)
        {
            var data = codec.Encode(packet);
            Sessions.BroadcastAsync(data, completed);
        }

        public void Disconnect()
        {
            Sessions.CloseSession(ID);
        }
    }

    public class Server
    {
        public void Run(string[] args)
        {
            var _ = GameService.Instance;

            var port = Config.ServerPort;
            var wssv = new WebSocketServer($"ws://127.0.0.1:{port}");
            wssv.AddWebSocketService<GameSession>("/game");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
