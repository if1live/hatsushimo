using System.Threading.Tasks;
using NLog;
using Optional;
using SQLite;

namespace Mikazuki.DB
{
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
