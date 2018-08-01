using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using Optional;
using Optional.Linq;
using NLog;

namespace Mikazuki.DB
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
            var filename = "mikazuki.db";
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
}
