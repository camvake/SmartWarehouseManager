using System.Data.SQLite;
using System.Collections.Generic;

namespace SWM.Data.Repositories
{
    public abstract class BaseRepository<T>
    {
        protected string _connectionString;

        public BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}