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
        public void TestRoomJoinRequestPacket()
        {
            var a = new RoomJoinRequestPacket()
            {
                RoomID = "foo",
                Nickname = "test",
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestRoomJoinResponsePacket()
        {
            var a = new RoomJoinResponsePacket()
            {
                PlayerID = 123,
                RoomID = "foo",
                Nickname = "test",
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestRoomLeavePacket()
        {
            var a = new RoomLeavePacket()
            {
                PlayerID = 123,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestInputMovePacket()
        {
            var a = new InputMovePacket()
            {
                Dir = new Vec2(1, 2),
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestInputCommandPacket()
        {
            var a = new InputCommandPacket()
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
            var b = (LeaderboardPacket)SerializeAndDeserialize(a);

            Assert.Equal(a.Players, b.Players);
            for (var i = 0; i < a.Top.Length; i++)
            {
                Assert.Equal(a.Top[i], b.Top[i]);
            }
        }

        ReplicationActionPacket CreateFakeReplicationActionPacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Create,
                ID = 123,
                ActorType = ActorType.Player,
                Pos = new Vec2(1, 2),
                Dir = new Vec2(3, 4),
                Speed = 5,
                Extra = "todo",
            };
        }

        [Fact]
        public void TestReplicationActionPacket()
        {
            var a = CreateFakeReplicationActionPacket();
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestReplicationBulkActionPacket()
        {
            var a = new ReplicationBulkActionPacket()
            {
                Actions = new ReplicationActionPacket[]
                {
                    CreateFakeReplicationActionPacket(),
                    CreateFakeReplicationActionPacket(),
                },
            };
            var b = (ReplicationBulkActionPacket)SerializeAndDeserialize(a);

            Assert.Equal(a.Actions.Length, b.Actions.Length);
            for (var i = 0; i < a.Actions.Length; i++)
            {
                Assert.Equal(a.Actions[i], b.Actions[i]);
            }
        }

        IPacket SerializeAndDeserialize(IPacket a)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            a.Serialize(writer);

            var reader = new BinaryReader(new MemoryStream(stream.ToArray()));
            var b = a.CreateBlank();
            b.Deserialize(reader);

            return b;
        }
    }
}
