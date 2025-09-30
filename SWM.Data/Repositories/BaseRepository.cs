using System;
using System.Data;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public abstract class BaseRepository : IDisposable
    {
        protected readonly string _connectionString;
        protected SQLiteConnection _connection;

        public BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(_connectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        protected void ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = GetConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        protected object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = GetConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        protected SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parameters)
        {
            var connection = GetConnection();
            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}