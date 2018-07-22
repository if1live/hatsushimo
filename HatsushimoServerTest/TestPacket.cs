using System;
using System.IO;
using Xunit;
using HatsushimoShared;

namespace HatsushimoServerTest
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
            var a = new ConnectPacket() {};
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestDisconnectPacket()
        {
            var a = new DisconnectPacket() {};
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

        ReplicationActionPacket CreateFakeReplicationActionPacket()
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Create,
                ID = 123,
                ActorType = ActorType.Player,
                PosX = 1,
                PosY = 2,
                DirX = 3,
                DirY = 4,
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
            for(var i = 0 ; i < a.Actions.Length ; i++) {
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
