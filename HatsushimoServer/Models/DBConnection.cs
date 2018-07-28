using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using Optional;
using Optional.Linq;
using NLog;

namespace HatsushimoServer.Models
{
    public interface IDBConnection
    {
        Task CreateTable();

        Task<User> GetOrCreateUser(string uuid);
        Task<Option<User>> GetUser(string uuid);
    }

    public class DBConnectionFactory {
        public IDBConnection Connect() {
            // sqlite
            var filename = "hatsushimo.db";
            var dirname = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dbpath = Path.Combine(dirname, filename);
            return ConnectSqlite(dbpath);
        }

        public IDBConnection ConnectSqlite(string dbpath) {
            var sqlite = new SQLiteAsyncConnection(dbpath);
            var conn = new SQLiteConnection(sqlite);
            return conn;
        }
    }

    class SQLiteConnection : IDBConnection
    {
        static readonly Logger log = LogManager.GetLogger("SQLiteConnection");
        SQLiteAsyncConnection conn;

        public SQLiteConnection(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public async Task CreateTable()
        {
            await conn.CreateTableAsync<User>();
            log.Info("create user table");
        }

        public async Task<User> GetOrCreateUser(string uuid)
        {
            var query = conn.Table<User>().Where(u => u.Uuid == uuid);
            var prevUser = await query.FirstOrDefaultAsync();
            if (prevUser == null)
            {
                var user = new User()
                {
                    Uuid = uuid,
                };
                await conn.InsertAsync(user);
                log.Info($"create user: uuid={uuid}");
                return user;
            }

            log.Info($"sign up: uuid={uuid}");
            return prevUser;
        }

        public async Task<Option<User>> GetUser(string uuid)
        {
            var query = conn.Table<User>().Where(u => u.Uuid == uuid);
            var user = await query.FirstOrDefaultAsync();
            if (user == null)
            {
                return Option.None<User>();
            }
            return Option.Some(user);
        }
    }
}
