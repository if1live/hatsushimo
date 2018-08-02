using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Optional.Unsafe;

namespace Shigure
{
    public class AssertThinker : IThinker
    {
        int ping = 1234;
        string botUuid = "bot-uuid";
        string worldID = "default";
        string nickname = "bot-nick";

        int playerID = 0;

        readonly Connection conn;

        public AssertThinker(Connection conn)
        {
            this.conn = conn;
        }

        void HandleWelcome(WelcomePacket p)
        {
            Debug.Assert(p.Version == Config.Version);

            playerID = p.UserID;
            Debug.Assert(playerID > 0);
        }

        void HandlePing(PingPacket p)
        {
            Debug.Assert(p.millis == ping);
        }

        void HandleSignUp(SignUpResultPacket p)
        {
            Debug.Assert(0 == p.ResultCode);
        }

        void HandleAuthentication(AuthenticationResultPacket p)
        {
            Debug.Assert(0 == p.ResultCode);
        }

        void HandleWorldJoin(WorldJoinResultPacket p)
        {
            Debug.Assert(nickname == p.Nickname);
            Debug.Assert(worldID == p.WorldID);
        }

        void HandleWorldLeave(WorldLeaveResultPacket p)
        {
            Debug.Assert(p.PlayerID == playerID);
        }

        void HandlePlayerReady(PlayerReadyPacket p)
        {
        }

        void HandleDisconnect(DisconnectPacket p)
        {
        }

        void HandleAttackNotify(AttackNotifyPacket p)
        {
        }

        void HandleMoveNotify(MoveNotifyPacket p)
        {
            var my = p.list.Where(el => el.ID == playerID).ToArray();
            Debug.Assert(my.Length == 1);
        }

        void HandleLeaderboard(LeaderboardPacket p)
        {
            Debug.Assert(p.Top.Length >= 0);
            Debug.Assert(p.Players >= 0);
        }

        public async Task<bool> Run()
        {
            conn.Send(new ConnectPacket());

            var welcome = await conn.Recv<WelcomePacket>();
            HandleWelcome(welcome.ValueOrFailure());

            conn.Send(new PingPacket() { millis = this.ping });
            var ping = await conn.Recv<PingPacket>();
            HandlePing(ping.ValueOrFailure());

            conn.Send(new SignUpPacket() { Uuid = botUuid });
            var signup = await conn.Recv<SignUpResultPacket>();
            HandleSignUp(signup.ValueOrFailure());

            conn.Send(new AuthenticationPacket() { Uuid = botUuid });
            var auth = await conn.Recv<AuthenticationResultPacket>();
            HandleAuthentication(auth.ValueOrFailure());

            conn.Send(new WorldJoinPacket()
            {
                WorldID = worldID,
                Nickname = nickname,
            });
            var join = await conn.Recv<WorldJoinResultPacket>();
            HandleWorldJoin(join.ValueOrFailure());

            conn.Send(new PlayerReadyPacket());
            var ready = await conn.Recv<PlayerReadyPacket>();
            HandlePlayerReady(ready.ValueOrFailure());

            // TODO game loop 테스트 구현?
            await RunLeaderboardTest();
            await RunMoveTest();

            conn.Send(new WorldLeavePacket());
            var leave = await conn.Recv<WorldLeaveResultPacket>();
            HandleWorldLeave(leave.ValueOrFailure());

            conn.Send(new DisconnectPacket());
            var disconnect = await conn.Recv<DisconnectPacket>();
            HandleDisconnect(disconnect.ValueOrFailure());


            conn.Send(new DisconnectPacket());
            conn.Shutdown();

            Console.WriteLine("test completed");
            return true;
        }

        async Task<int> RunLeaderboardTest()
        {
            // 아무것도 안해도 리더보드 정보를 받는다
            for (var i = 0; i < 2; i++)
            {
                var leaderboard = await conn.RecvLeaderboard();
                HandleLeaderboard(leaderboard.ValueOrFailure());
            }
            return 0;
        }

        async Task<int> RunMoveTest()
        {
            var targetPos = new System.Numerics.Vector2(1, 2);

            {
                // 아무것도 안해도 이동패킷이 온다
                var moveNotify = await conn.RecvMove();
                var packet = moveNotify.ValueOrFailure();
                var my = packet.list.Where(el => el.ID == playerID).ToArray();
                Debug.Assert(my.Length == 1);
            }

            // 이동 요청 보내기전에 받은 패킷은 테스트에 필요없다
            conn.FlushMove();

            // 이동 요청
            conn.Send(new MovePacket()
            {
                TargetPos = targetPos,
            });

            // 요청한 좌표를 받았는지 확인하기
            // send 요청이 들어가기전에 이동 패킷을 받았을지모른다
            // 받은 패킷 몇개에 대해서 검증하기
            var tryCount = 5;
            for (var i = 0; i < tryCount; i++)
            {
                // 요청된 좌표 정보를 돌려받는다
                var moveNotify = await conn.RecvMove();
                var packet = moveNotify.ValueOrFailure();
                var my = packet.list.Where(el => el.ID == playerID).ToArray();
                Debug.Assert(my.Length == 1);
                var myNotify = my[0];
                // TODO 최초의 targetPos가 0,0이어도 상관없나?
                if (myNotify.TargetPos == System.Numerics.Vector2.Zero)
                {
                    continue;
                }
                Debug.Assert(myNotify.TargetPos == targetPos);
            }
            return 0;
        }
    }
}
