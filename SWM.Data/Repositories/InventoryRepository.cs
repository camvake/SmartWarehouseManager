using System.Data.SQLite;
using System.Collections.Generic;
using SWM.Core.Models;
using System.Data;
using System.Linq;

namespace SWM.Data.Repositories
{
    public class InventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public List<Inventory> GetAllInventories()
        {
            var inventories = new List<Inventory>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT i.*, w.Name as WarehouseName 
                    FROM Inventories i 
                    LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID 
                    ORDER BY i.InventoryDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var inventory = new Inventory
                        {
                            InventoryID = GetInt(reader, "InventoryID"),
                            InventoryNumber = GetString(reader, "InventoryNumber"),
                            InventoryDate = GetDateTime(reader, "InventoryDate"),
                            WarehouseID = GetInt(reader, "WarehouseID"),
                            Status = (InventoryStatus)GetInt(reader, "Status"),
                            Notes = GetString(reader, "Notes"),
                            CreatedDate = GetDateTime(reader, "CreatedDate"),
                            CompletedDate = GetNullableDateTime(reader, "CompletedDate")
                        };

                        // Загружаем элементы инвентаризации
                        inventory.InventoryItems = GetInventoryItems(inventory.InventoryID);
                        inventories.Add(inventory);
                    }
                }
            }

            return inventories;
        }

        public List<InventoryItem> GetInventoryItems(int inventoryId)
        {
            var items = new List<InventoryItem>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT ii.*, p.Name as ProductName, p.ArticleNumber 
                    FROM InventoryItems ii 
                    LEFT JOIN Products p ON ii.ProductID = p.ProductID 
                    WHERE ii.InventoryID = @InventoryID";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InventoryID", inventoryId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new InventoryItem
                            {
                                InventoryItemID = GetInt(reader, "InventoryItemID"),
                                InventoryID = GetInt(reader, "InventoryID"),
                                ProductID = GetInt(reader, "ProductID"),
                                ExpectedQuantity = GetDecimal(reader, "ExpectedQuantity"),
                                ActualQuantity = GetDecimal(reader, "ActualQuantity"),
                                Notes = GetString(reader, "Notes"),
                                Product = new Product
                                {
                                    Name = GetString(reader, "ProductName"),
                                    ArticleNumber = GetString(reader, "ArticleNumber")
                                }
                            });
                        }
                    }
                }
            }

            return items;
        }

        public int CreateInventory(Inventory inventory)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Inventories 
                    (InventoryNumber, InventoryDate, WarehouseID, Status, Notes, CreatedDate) 
                    VALUES 
                    (@InventoryNumber, @InventoryDate, @WarehouseID, @Status, @Notes, @CreatedDate);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    inventory.InventoryNumber = GenerateInventoryNumber();
                    inventory.CreatedDate = DateTime.Now;

                    command.Parameters.AddWithValue("@InventoryNumber", inventory.InventoryNumber);
                    command.Parameters.AddWithValue("@InventoryDate", inventory.InventoryDate);
                    command.Parameters.AddWithValue("@WarehouseID", inventory.WarehouseID);
                    command.Parameters.AddWithValue("@Status", (int)inventory.Status);
                    command.Parameters.AddWithValue("@Notes", inventory.Notes ?? "");
                    command.Parameters.AddWithValue("@CreatedDate", inventory.CreatedDate);

                    var inventoryId = Convert.ToInt32(command.ExecuteScalar());
                    return inventoryId;
                }
            }
        }

        public void AddInventoryItem(InventoryItem item)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO InventoryItems 
                    (InventoryID, ProductID, ExpectedQuantity, ActualQuantity, Notes) 
                    VALUES 
                    (@InventoryID, @ProductID, @ExpectedQuantity, @ActualQuantity, @Notes)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InventoryID", item.InventoryID);
                    command.Parameters.AddWithValue("@ProductID", item.ProductID);
                    command.Parameters.AddWithValue("@ExpectedQuantity", item.ExpectedQuantity);
                    command.Parameters.AddWithValue("@ActualQuantity", item.ActualQuantity);
                    command.Parameters.AddWithValue("@Notes", item.Notes ?? "");

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CompleteInventory(int inventoryId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Обновляем остатки товаров на основе инвентаризации
                        var updateQuery = @"
                            UPDATE Products 
                            SET StockBalance = (
                                SELECT ii.ActualQuantity 
                                FROM InventoryItems ii 
                                WHERE ii.ProductID = Products.ProductID AND ii.InventoryID = @InventoryID
                            )
                            WHERE ProductID IN (
                                SELECT ProductID FROM InventoryItems WHERE InventoryID = @InventoryID
                            )";

                        using (var command = new SQLiteCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@InventoryID", inventoryId);
                            command.ExecuteNonQuery();
                        }

                        // 2. Обновляем статус инвентаризации
                        var statusQuery = @"
                            UPDATE Inventories 
                            SET Status = @Status, CompletedDate = @CompletedDate 
                            WHERE InventoryID = @InventoryID";

                        using (var command = new SQLiteCommand(statusQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Status", (int)InventoryStatus.Completed);
                            command.Parameters.AddWithValue("@CompletedDate", DateTime.Now);
                            command.Parameters.AddWithValue("@InventoryID", inventoryId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
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

        private decimal GetDecimal(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetDecimal(ordinal);
        }

        private DateTime GetDateTime(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
        }

        private DateTime? GetNullableDateTime(SQLiteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }

        private string GenerateInventoryNumber()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }
    }
}