using System;
using Mikazuki.DB;
using Xunit;

/*
[assembly: CollectionBehavior(MaxParallelThreads = 1)]
namespace MikazukiTest.DB
{
    public class TestDBConnection
    {
        IDBConnection CreateConnection()
        {
            var factory = new DBConnectionFactory();
            var conn = factory.ConnectSqlite(":memory:");
            conn.CreateTable();
            return conn;
        }

        [Fact]
        public async void TestGetOrCreateUser_new_user()
        {
            var conn = CreateConnection();
            var uuid = "foo";
            var user = await conn.GetOrCreateUser(uuid);
            Assert.NotNull(user);
            Assert.Equal(uuid, user.Uuid);
        }

        [Fact]
        public async void TestGetOrCreateUser_prev_user()
        {
            var conn = CreateConnection();
            // create new user. 이전 테스트에서 검증 되었다고 가정
            var uuid = "foo1";
            var u1 = await conn.GetOrCreateUser(uuid);

            var u2 = await conn.GetOrCreateUser(uuid);
            Assert.Equal(u2.ID, u1.ID);
        }

        [Fact]
        public async void TestGetUser_not_exist()
        {
            var conn = CreateConnection();
            var uuid = "foo2";
            var option = await conn.GetUser(uuid);
            Assert.False(option.HasValue);
        }

        [Fact]
        public async void TestGetUser_found()
        {
            var conn = CreateConnection();
            var uuid = "foo3";
            var u1 = await conn.GetOrCreateUser(uuid);

            var u2 = await conn.GetUser(uuid);
            Assert.True(u2.HasValue);
        }
    }

}
*/
