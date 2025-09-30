using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class InventoryRepository : BaseRepository
    {
        public InventoryRepository(string connectionString) : base(connectionString) { }

        public Inventory GetById(int inventoryId)
        {
            var sql = @"
                SELECT i.*, w.WarehouseName, u.FirstName, u.LastName
                FROM Inventories i
                LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON i.CreatedBy = u.UserID
                WHERE i.InventoryID = @InventoryID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@InventoryID", inventoryId)))
            {
                if (reader.Read())
                {
                    var inventory = MapInventory(reader);
                    inventory.InventoryItems = GetInventoryItems(inventoryId);
                    return inventory;
                }
            }
            return null;
        }

        public Inventory GetByNumber(string inventoryNumber)
        {
            var sql = @"
                SELECT i.*, w.WarehouseName, u.FirstName, u.LastName
                FROM Inventories i
                LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON i.CreatedBy = u.UserID
                WHERE i.InventoryNumber = @InventoryNumber";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@InventoryNumber", inventoryNumber)))
            {
                if (reader.Read())
                {
                    var inventory = MapInventory(reader);
                    inventory.InventoryItems = GetInventoryItems(Convert.ToInt32(reader["InventoryID"]));
                    return inventory;
                }
            }
            return null;
        }

        public List<Inventory> GetAll()
        {
            var inventories = new List<Inventory>();
            var sql = @"
                SELECT i.*, w.WarehouseName, u.FirstName, u.LastName
                FROM Inventories i
                LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON i.CreatedBy = u.UserID
                ORDER BY i.InventoryDate DESC";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    var inventory = MapInventory(reader);
                    inventories.Add(inventory);
                }
            }
            return inventories;
        }

        public List<Inventory> GetByStatus(string status)
        {
            var inventories = new List<Inventory>();
            var sql = @"
                SELECT i.*, w.WarehouseName, u.FirstName, u.LastName
                FROM Inventories i
                LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON i.CreatedBy = u.UserID
                WHERE i.Status = @Status
                ORDER BY i.InventoryDate DESC";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@Status", status)))
            {
                while (reader.Read())
                {
                    var inventory = MapInventory(reader);
                    inventories.Add(inventory);
                }
            }
            return inventories;
        }

        public List<Inventory> GetByWarehouse(int warehouseId)
        {
            var inventories = new List<Inventory>();
            var sql = @"
                SELECT i.*, w.WarehouseName, u.FirstName, u.LastName
                FROM Inventories i
                LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON i.CreatedBy = u.UserID
                WHERE i.WarehouseID = @WarehouseID
                ORDER BY i.InventoryDate DESC";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@WarehouseID", warehouseId)))
            {
                while (reader.Read())
                {
                    var inventory = MapInventory(reader);
                    inventories.Add(inventory);
                }
            }
            return inventories;
        }

        public int Create(Inventory inventory)
        {
            var sql = @"
                INSERT INTO Inventories (InventoryNumber, WarehouseID, CreatedBy, Notes)
                VALUES (@InventoryNumber, @WarehouseID, @CreatedBy, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@InventoryNumber", inventory.InventoryNumber),
                new SQLiteParameter("@WarehouseID", inventory.WarehouseID),
                new SQLiteParameter("@CreatedBy", inventory.CreatedBy),
                new SQLiteParameter("@Notes", inventory.Notes ?? (object)DBNull.Value)
            };

            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        public void CreateInventoryItem(InventoryItem item)
        {
            var sql = @"
                INSERT INTO InventoryItems (InventoryID, ProductID, ExpectedQuantity, ActualQuantity, Notes)
                VALUES (@InventoryID, @ProductID, @ExpectedQuantity, @ActualQuantity, @Notes)";

            var parameters = new[]
            {
                new SQLiteParameter("@InventoryID", item.InventoryID),
                new SQLiteParameter("@ProductID", item.ProductID),
                new SQLiteParameter("@ExpectedQuantity", item.ExpectedQuantity),
                new SQLiteParameter("@ActualQuantity", item.ActualQuantity),
                new SQLiteParameter("@Notes", item.Notes ?? (object)DBNull.Value)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void UpdateInventoryItem(InventoryItem item)
        {
            var sql = @"
                UPDATE InventoryItems 
                SET ActualQuantity = @ActualQuantity, Notes = @Notes
                WHERE InventoryItemID = @InventoryItemID";

            var parameters = new[]
            {
                new SQLiteParameter("@ActualQuantity", item.ActualQuantity),
                new SQLiteParameter("@Notes", item.Notes ?? (object)DBNull.Value),
                new SQLiteParameter("@InventoryItemID", item.InventoryItemID)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void CompleteInventory(int inventoryId, decimal totalDiscrepancy)
        {
            var sql = @"
                UPDATE Inventories 
                SET Status = 'Завершена', CompletedDate = CURRENT_TIMESTAMP, TotalDiscrepancy = @TotalDiscrepancy
                WHERE InventoryID = @InventoryID";

            ExecuteNonQuery(sql,
                new SQLiteParameter("@TotalDiscrepancy", totalDiscrepancy),
                new SQLiteParameter("@InventoryID", inventoryId));
        }

        public List<InventoryItemDisplayDto> GetInventoryItemsForDisplay(int inventoryId)
        {
            var items = new List<InventoryItemDisplayDto>();
            var sql = @"
                SELECT ii.*, p.ProductName, p.ArticleNumber, p.PurchasePrice
                FROM InventoryItems ii
                LEFT JOIN Products p ON ii.ProductID = p.ProductID
                WHERE ii.InventoryID = @InventoryID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@InventoryID", inventoryId)))
            {
                while (reader.Read())
                {
                    items.Add(new InventoryItemDisplayDto
                    {
                        InventoryItemID = Convert.ToInt32(reader["InventoryItemID"]),
                        ProductName = reader["ProductName"].ToString(),
                        ArticleNumber = reader["ArticleNumber"].ToString(),
                        ExpectedQuantity = Convert.ToInt32(reader["ExpectedQuantity"]),
                        ActualQuantity = Convert.ToInt32(reader["ActualQuantity"]),
                        QuantityDifference = Convert.ToInt32(reader["QuantityDifference"]),
                        CostDifference = Convert.ToDecimal(reader["CostDifference"]),
                        Notes = reader["Notes"]?.ToString(),
                        ProductPrice = Convert.ToDecimal(reader["PurchasePrice"])
                    });
                }
            }
            return items;
        }

        private List<InventoryItem> GetInventoryItems(int inventoryId)
        {
            var items = new List<InventoryItem>();
            var sql = @"
                SELECT ii.*, p.ProductName, p.ArticleNumber
                FROM InventoryItems ii
                LEFT JOIN Products p ON ii.ProductID = p.ProductID
                WHERE ii.InventoryID = @InventoryID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@InventoryID", inventoryId)))
            {
                while (reader.Read())
                {
                    items.Add(new InventoryItem
                    {
                        InventoryItemID = Convert.ToInt32(reader["InventoryItemID"]),
                        InventoryID = Convert.ToInt32(reader["InventoryID"]),
                        ProductID = Convert.ToInt32(reader["ProductID"]),
                        ExpectedQuantity = Convert.ToInt32(reader["ExpectedQuantity"]),
                        ActualQuantity = Convert.ToInt32(reader["ActualQuantity"]),
                        QuantityDifference = Convert.ToInt32(reader["QuantityDifference"]),
                        CostDifference = Convert.ToDecimal(reader["CostDifference"]),
                        Notes = reader["Notes"]?.ToString(),
                        Product = new Product
                        {
                            ProductName = reader["ProductName"].ToString(),
                            ArticleNumber = reader["ArticleNumber"].ToString()
                        }
                    });
                }
            }
            return items;
        }

        private Inventory MapInventory(SQLiteDataReader reader)
        {
            return new Inventory
            {
                InventoryID = Convert.ToInt32(reader["InventoryID"]),
                InventoryNumber = reader["InventoryNumber"].ToString(),
                WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                InventoryDate = Convert.ToDateTime(reader["InventoryDate"]),
                Status = reader["Status"].ToString(),
                CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                CompletedDate = reader["CompletedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CompletedDate"]) : null,
                TotalDiscrepancy = Convert.ToDecimal(reader["TotalDiscrepancy"]),
                Notes = reader["Notes"]?.ToString(),
                Warehouse = new Warehouse { WarehouseName = reader["WarehouseName"]?.ToString() },
                CreatedByUser = new User
                {
                    FirstName = reader["FirstName"]?.ToString(),
                    LastName = reader["LastName"]?.ToString()
                }
            };
        }
    }
}