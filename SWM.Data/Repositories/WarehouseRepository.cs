using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class WarehouseRepository : BaseRepository
    {
        public WarehouseRepository(string connectionString) : base(connectionString) { }

        public Warehouse GetById(int warehouseId)
        {
            var sql = @"
                SELECT w.*, u.FirstName, u.LastName
                FROM Warehouses w
                LEFT JOIN Users u ON w.ManagerID = u.UserID
                WHERE w.WarehouseID = @WarehouseID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@WarehouseID", warehouseId)))
            {
                if (reader.Read())
                {
                    return MapWarehouse(reader);
                }
            }
            return null;
        }

        public List<Warehouse> GetAll()
        {
            var warehouses = new List<Warehouse>();
            var sql = @"
                SELECT w.*, u.FirstName, u.LastName
                FROM Warehouses w
                LEFT JOIN Users u ON w.ManagerID = u.UserID
                WHERE w.IsActive = 1
                ORDER BY w.WarehouseName";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    warehouses.Add(MapWarehouse(reader));
                }
            }
            return warehouses;
        }

        public List<Warehouse> GetActiveWarehouses()
        {
            var warehouses = new List<Warehouse>();
            var sql = "SELECT * FROM Warehouses WHERE IsActive = 1 ORDER BY WarehouseName";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    warehouses.Add(MapWarehouse(reader));
                }
            }
            return warehouses;
        }

        public int Create(Warehouse warehouse)
        {
            var sql = @"
                INSERT INTO Warehouses (WarehouseName, Address, ContactPhone, Email, Capacity, ManagerID)
                VALUES (@WarehouseName, @Address, @ContactPhone, @Email, @Capacity, @ManagerID);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@WarehouseName", warehouse.WarehouseName),
                new SQLiteParameter("@Address", warehouse.Address),
                new SQLiteParameter("@ContactPhone", warehouse.ContactPhone ?? (object)DBNull.Value),
                new SQLiteParameter("@Email", warehouse.Email ?? (object)DBNull.Value),
                new SQLiteParameter("@Capacity", warehouse.Capacity),
                new SQLiteParameter("@ManagerID", warehouse.ManagerID ?? (object)DBNull.Value)
            };

            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        public void Update(Warehouse warehouse)
        {
            var sql = @"
                UPDATE Warehouses 
                SET WarehouseName = @WarehouseName, Address = @Address, ContactPhone = @ContactPhone,
                    Email = @Email, Capacity = @Capacity, ManagerID = @ManagerID,
                    UpdatedDate = CURRENT_TIMESTAMP
                WHERE WarehouseID = @WarehouseID";

            var parameters = new[]
            {
                new SQLiteParameter("@WarehouseName", warehouse.WarehouseName),
                new SQLiteParameter("@Address", warehouse.Address),
                new SQLiteParameter("@ContactPhone", warehouse.ContactPhone ?? (object)DBNull.Value),
                new SQLiteParameter("@Email", warehouse.Email ?? (object)DBNull.Value),
                new SQLiteParameter("@Capacity", warehouse.Capacity),
                new SQLiteParameter("@ManagerID", warehouse.ManagerID ?? (object)DBNull.Value),
                new SQLiteParameter("@WarehouseID", warehouse.WarehouseID)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void UpdateOccupancy(int warehouseId, int newOccupancy)
        {
            var sql = "UPDATE Warehouses SET CurrentOccupancy = @Occupancy, UpdatedDate = CURRENT_TIMESTAMP WHERE WarehouseID = @WarehouseID";
            ExecuteNonQuery(sql,
                new SQLiteParameter("@Occupancy", newOccupancy),
                new SQLiteParameter("@WarehouseID", warehouseId));
        }

        public void Delete(int warehouseId)
        {
            var sql = "UPDATE Warehouses SET IsActive = 0, UpdatedDate = CURRENT_TIMESTAMP WHERE WarehouseID = @WarehouseID";
            ExecuteNonQuery(sql, new SQLiteParameter("@WarehouseID", warehouseId));
        }

        private Warehouse MapWarehouse(SQLiteDataReader reader)
        {
            return new Warehouse
            {
                WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                WarehouseName = reader["WarehouseName"].ToString(),
                Address = reader["Address"].ToString(),
                ContactPhone = reader["ContactPhone"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                Capacity = Convert.ToInt32(reader["Capacity"]),
                CurrentOccupancy = Convert.ToInt32(reader["CurrentOccupancy"]),
                ManagerID = reader["ManagerID"] != DBNull.Value ? Convert.ToInt32(reader["ManagerID"]) : null,
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"]),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}