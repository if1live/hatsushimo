using System;
using System.IO;
using Xunit;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.NetChan;

namespace HatsushimoServerTest.Packets
{
    public class TestPacket
    {
        [Fact]
        public void TestPingPacket()
        {
            var a = new PingPacket()
            {
                millis = 1234,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestWelcomePacket()
        {
            var a = new WelcomePacket()
            {
                UserID = 12,
                Version = 34,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestConnectPacket()
        {
            var a = new ConnectPacket() { };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestDisconnectPacket()
        {
            var a = new DisconnectPacket() { };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestWorldJoinPacket()
        {
            var a = new WorldJoinPacket()
            {
                WorldID = "foo",
                Nickname = "test",
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestWorldJoinResultPacket()
        {
            var a = new WorldJoinResultPacket()
            {
                PlayerID = 123,
                WorldID = "foo",
                Nickname = "test",
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestWorldLeaveResultPacket()
        {
            var a = new WorldLeaveResultPacket()
            {
                PlayerID = 123,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestWorldLeavePacket()
        {
            var a = new WorldLeavePacket() { };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestMovePacket()
        {
            var a = new MovePacket()
            {
                TargetPos = new Vec2(1, 2),
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestAttaclPacket()
        {
            var a = new AttackPacket()
            {
                Mode = 123,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestLeaderboardPacket()
        {
            var a = new LeaderboardPacket()
            {
                Players = 10,
                Top = new Rank[]
                {
                    new Rank() { ID=1, Score=2, Ranking=3 },
                    new Rank() { ID=4, Score=5, Ranking=6 },
                },
            };
            var b = SerializeAndDeserialize(a);

            Assert.Equal(a, b);
        }

        [Fact]
        public void TestSignUpPacket()
        {
            var a = new SignUpPacket() { Uuid = "hello", };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestSignUpResultPacket()
        {
            var a = new SignUpResultPacket() { ResultCode = 12 };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestAuthenticationPacket()
        {
            var a = new AuthenticationPacket() { Uuid = "hello" };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestAuthenticationResultPacket()
        {
            var a = new AuthenticationResultPacket() { ResultCode = 12 };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        T SerializeAndDeserialize<T>(T a) where T : IPacket, new()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            a.Serialize(writer);

            var reader = new BinaryReader(new MemoryStream(stream.ToArray()));
            var b = new T();
            b.Deserialize(reader);

            return b;
        }
    }
}
