using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class UserRepository : BaseRepository
    {
        public UserRepository(string connectionString) : base(connectionString) { }

        public User GetById(int userId)
        {
            var sql = @"
                SELECT u.*, w.WarehouseName 
                FROM Users u 
                LEFT JOIN Warehouses w ON u.WarehouseID = w.WarehouseID 
                WHERE u.UserID = @UserID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@UserID", userId)))
            {
                if (reader.Read())
                {
                    return MapUser(reader);
                }
            }
            return null;
        }

        public User GetByLogin(string login)
        {
            var sql = @"
                SELECT u.*, w.WarehouseName 
                FROM Users u 
                LEFT JOIN Warehouses w ON u.WarehouseID = w.WarehouseID 
                WHERE u.Login = @Login AND u.IsActive = 1";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@Login", login)))
            {
                if (reader.Read())
                {
                    return MapUser(reader);
                }
            }
            return null;
        }

        public List<User> GetAll()
        {
            var users = new List<User>();
            var sql = @"
                SELECT u.*, w.WarehouseName 
                FROM Users u 
                LEFT JOIN Warehouses w ON u.WarehouseID = w.WarehouseID 
                WHERE u.IsActive = 1 
                ORDER BY u.LastName, u.FirstName";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    users.Add(MapUser(reader));
                }
            }
            return users;
        }

        public int Create(User user)
        {
            var sql = @"
                INSERT INTO Users (Login, Email, PasswordHash, PasswordSalt, FirstName, LastName, 
                                 PhoneNumber, RoleID, WarehouseID, IsActive)
                VALUES (@Login, @Email, @PasswordHash, @PasswordSalt, @FirstName, @LastName,
                       @PhoneNumber, @RoleID, @WarehouseID, 1);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@Login", user.Login),
                new SQLiteParameter("@Email", user.Email),
                new SQLiteParameter("@PasswordHash", user.PasswordHash),
                new SQLiteParameter("@PasswordSalt", user.PasswordSalt),
                new SQLiteParameter("@FirstName", user.FirstName),
                new SQLiteParameter("@LastName", user.LastName),
                new SQLiteParameter("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value),
                new SQLiteParameter("@RoleID", (int)user.Role),
                new SQLiteParameter("@WarehouseID", user.WarehouseID ?? (object)DBNull.Value)
            };

            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        public void Update(User user)
        {
            var sql = @"
                UPDATE Users 
                SET Login = @Login, Email = @Email, FirstName = @FirstName, LastName = @LastName,
                    PhoneNumber = @PhoneNumber, RoleID = @RoleID, WarehouseID = @WarehouseID,
                    UpdatedDate = CURRENT_TIMESTAMP
                WHERE UserID = @UserID";

            var parameters = new[]
            {
                new SQLiteParameter("@Login", user.Login),
                new SQLiteParameter("@Email", user.Email),
                new SQLiteParameter("@FirstName", user.FirstName),
                new SQLiteParameter("@LastName", user.LastName),
                new SQLiteParameter("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value),
                new SQLiteParameter("@RoleID", (int)user.Role),
                new SQLiteParameter("@WarehouseID", user.WarehouseID ?? (object)DBNull.Value),
                new SQLiteParameter("@UserID", user.UserID)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void Delete(int userId)
        {
            var sql = "UPDATE Users SET IsActive = 0, UpdatedDate = CURRENT_TIMESTAMP WHERE UserID = @UserID";
            ExecuteNonQuery(sql, new SQLiteParameter("@UserID", userId));
        }

        public void UpdateLastLogin(int userId)
        {
            var sql = "UPDATE Users SET LastLoginDate = CURRENT_TIMESTAMP WHERE UserID = @UserID";
            ExecuteNonQuery(sql, new SQLiteParameter("@UserID", userId));
        }

        private User MapUser(SQLiteDataReader reader)
        {
            return new User
            {
                UserID = Convert.ToInt32(reader["UserID"]),
                Login = reader["Login"].ToString(),
                Email = reader["Email"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                PasswordSalt = reader["PasswordSalt"].ToString(),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                PhoneNumber = reader["PhoneNumber"]?.ToString(),
                Role = (UserRole)Convert.ToInt32(reader["RoleID"]),
                WarehouseID = reader["WarehouseID"] != DBNull.Value ? Convert.ToInt32(reader["WarehouseID"]) : null,
                LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginDate"]) : null,
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                Warehouse = reader["WarehouseName"] != DBNull.Value ? new Warehouse { WarehouseName = reader["WarehouseName"].ToString() } : null
            };
        }
    }
}