using Hatsushimo.Packets;
using Xunit;

namespace HatsushimoTest.Packets
{
    public class TestWelcomePacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new WelcomePacket()
            {
                UserID = 12,
                Version = 34,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestPingPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new PingPacket()
            {
                millis = 1234,
            };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestConnectPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new ConnectPacket() { };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestDisconnectPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new DisconnectPacket() { };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestSignUpPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new SignUpPacket() { Uuid = "hello", };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestSignUpResultPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new SignUpResultPacket() { ResultCode = 12 };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);

        }
    }

    public class TestAuthenticationPacket : PacketTestCase
    {

        [Fact]
        public void TestSerde()
        {
            var a = new AuthenticationPacket() { Uuid = "hello" };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }

    public class TestAuthenticationResultPacket : PacketTestCase
    {
        [Fact]
        public void TestSerde()
        {
            var a = new AuthenticationResultPacket() { ResultCode = 12 };
            var b = SerializeAndDeserialize(a);
            Assert.Equal(a, b);
        }
    }
}
