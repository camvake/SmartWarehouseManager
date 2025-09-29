using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SWM.Core.Models;

namespace SWM.Data.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Users ORDER BY IsActive DESC, LastName, FirstName";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                UserID = GetInt(reader, "UserID"),
                                Login = GetString(reader, "Login"),
                                FirstName = GetString(reader, "FirstName"),
                                LastName = GetString(reader, "LastName"),
                                Email = GetString(reader, "Email"),
                                PhoneNumber = GetString(reader, "PhoneNumber"),
                                Role = (UserRole)GetInt(reader, "Role"),
                                IsActive = GetBool(reader, "IsActive"),
                                DateCreated = GetDateTime(reader, "DateCreated")
                            };

                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки пользователей: {ex.Message}");
            }

            return users;
        }

        public void CreateUser(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var query = @"INSERT INTO Users 
                            (Login, PasswordHash, FirstName, LastName, Email, PhoneNumber, Role, IsActive, DateCreated) 
                            VALUES 
                            (@Login, @PasswordHash, @FirstName, @LastName, @Email, @PhoneNumber, @Role, @IsActive, @DateCreated)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", user.Login);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? "");
                    command.Parameters.AddWithValue("@Role", (int)user.Role);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    command.Parameters.AddWithValue("@DateCreated", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUser(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var query = @"UPDATE Users SET 
                            FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                            PhoneNumber = @PhoneNumber, Role = @Role, IsActive = @IsActive 
                            WHERE UserID = @UserID";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", user.UserID);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? "");
                    command.Parameters.AddWithValue("@Role", (int)user.Role);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool LoginExists(string login)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM Users WHERE Login = @Login";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        // Вспомогательные методы для безопасного чтения данных
        private int GetInt(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        private string GetString(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        private bool GetBool(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            if (reader.IsDBNull(ordinal)) return false;

            var value = reader.GetValue(ordinal);
            return value.ToString() == "1" || value.ToString().ToLower() == "true";
        }

        private DateTime GetDateTime(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
        }
    }
}