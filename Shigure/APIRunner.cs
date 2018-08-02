using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;

namespace Shigure
{
    public class APIRunner
    {
        readonly Connection conn;
        public APIRunner(Connection conn)
        {
            this.conn = conn;
        }
        public async Task<int> Connect()
        {
            var send = new ConnectPacket();
            var recv = await conn.SendRecv<ConnectPacket, WelcomePacket>(send);
            Debug.Assert(recv.Version == Config.Version);
            return recv.UserID;
        }

        public async Task<int> SignUp(string uuid)
        {
            var send = new SignUpPacket()
            {
                Uuid = uuid,
            };
            var recv = await conn.SendRecv<SignUpPacket, SignUpResultPacket>(send);
            return recv.ResultCode;
        }

        public async Task<int> Authentication(string uuid)
        {
            var send = new AuthenticationPacket()
            {
                Uuid = uuid,
            };
            var recv = await conn.SendRecv<AuthenticationPacket, AuthenticationResultPacket>(send);
            return recv.ResultCode;
        }

        public async Task<bool> WorldJoin(string worldID, string nickname)
        {
            var send = new WorldJoinPacket()
            {
                WorldID = worldID,
                Nickname = nickname,
            };

            var recv = await conn.SendRecv<WorldJoinPacket, WorldJoinResultPacket>(send);
            return true;
        }

        public async Task<bool> WorldLeave()
        {
            var send = new WorldLeavePacket();
            var recv = await conn.SendRecv<WorldLeavePacket, WorldLeaveResultPacket>(send);
            return true;
        }

        public async Task<bool> PlayerReady()
        {
            var send = new PlayerReadyPacket();
            var recv = await conn.SendRecv<PlayerReadyPacket, PlayerReadyPacket>(send);
            return true;
        }

        public void Move(float x, float y)
        {
            var p = new MovePacket()
            {
                TargetPos = new Vector2(x, y)
            };
            conn.Send(p);
        }

        public void Disconnect()
        {
            conn.Send(new DisconnectPacket());
        }
    }
}
