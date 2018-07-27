using SQLite;

namespace HatsushimoServer.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Uuid { get; set; }
    }
}
