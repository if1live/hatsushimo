using System;
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
            var b = a.CreateBlank().Deserialize(a.Serialize());
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
            var b = a.CreateBlank().Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestConnectPacket()
        {
            var a = new ConnectPacket() {};
            var b = a.CreateBlank().Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestDisconnectPacket()
        {
            var a = new DisconnectPacket() {};
            var b = a.CreateBlank().Deserialize(a.Serialize());
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
            var b = a.CreateBlank().Deserialize(a.Serialize());
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
            var b = a.CreateBlank().Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestRoomLeavePacket()
        {
            var a = new RoomLeavePacket()
            {
                PlayerID = 123,
            };
            var b = a.CreateBlank().Deserialize(a.Serialize());
            Assert.Equal(a, b);
        }
    }
}
