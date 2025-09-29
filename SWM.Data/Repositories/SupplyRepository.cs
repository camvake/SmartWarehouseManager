using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using SWM.Core.Models;
using ProductModel = SWM.Core.Models.Product;

namespace SWM.Data.Repositories
{
    public class SupplyRepository : BaseRepository<Supply>
    {
        public SupplyRepository(string connectionString) : base(connectionString) { }

        public List<Supply> GetAllSupplies()
        {
            var supplies = new List<Supply>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT s.*, sup.Name as SupplierName, w.Name as WarehouseName
                    FROM Receipts s
                    LEFT JOIN Suppliers sup ON s.SupplierID = sup.SupplierID
                    LEFT JOIN Warehouses w ON s.WarehouseID = w.WarehouseID
                    ORDER BY s.ReceiptDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var supply = new Supply
                        {
                            SupplyID = reader.GetInt32(reader.GetOrdinal("ReceiptID")),
                            SupplyNumber = reader.GetString(reader.GetOrdinal("ReceiptNumber")),
                            SupplyDate = reader.GetDateTime(reader.GetOrdinal("ReceiptDate")),
                            SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                            SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
                            WarehouseID = reader.GetInt32(reader.GetOrdinal("WarehouseID")),
                            WarehouseName = reader.GetString(reader.GetOrdinal("WarehouseName")),
                            Status = SupplyStatus.Received // Упрощенно
                        };

                        // Загружаем товары поставки
                        supply.SupplyItems = GetSupplyItems(connection, supply.SupplyID);
                        supply.TotalAmount = supply.SupplyItems.Sum(x => x.TotalPrice);

                        supplies.Add(supply);
                    }
                }
            }

            return supplies;
        }

        private List<SupplyItem> GetSupplyItems(SQLiteConnection connection, int supplyId)
        {
            var items = new List<SupplyItem>();

            string query = @"
                SELECT si.*, p.Name as ProductName, p.ArticleNumber
                FROM ReceiptItems si
                JOIN Products p ON si.ProductID = p.ProductID
                WHERE si.ReceiptID = @SupplyID";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SupplyID", supplyId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new SupplyItem
                        {
                            SupplyItemID = reader.GetInt32(reader.GetOrdinal("ReceiptItemID")),
                            SupplyID = reader.GetInt32(reader.GetOrdinal("ReceiptID")),
                            ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            ArticleNumber = reader.GetString(reader.GetOrdinal("ArticleNumber")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
                        });
                    }
                }
            }

            return items;
        }

        public void CreateSupply(Supply supply)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Создаем поставку
                        string supplyQuery = @"
                            INSERT INTO Receipts (ReceiptNumber, ReceiptDate, SupplierID, WarehouseID)
                            VALUES (@SupplyNumber, @SupplyDate, @SupplierID, @WarehouseID);
                            SELECT last_insert_rowid();";

                        int supplyId;
                        using (var command = new SQLiteCommand(supplyQuery, connection))
                        {
                            command.Parameters.AddWithValue("@SupplyNumber", GenerateSupplyNumber());
                            command.Parameters.AddWithValue("@SupplyDate", supply.SupplyDate);
                            command.Parameters.AddWithValue("@SupplierID", supply.SupplierID);
                            command.Parameters.AddWithValue("@WarehouseID", supply.WarehouseID);

                            supplyId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Добавляем товары поставки
                        foreach (var item in supply.SupplyItems)
                        {
                            AddSupplyItem(connection, supplyId, item);

                            // Обновляем остатки на складе
                            UpdateProductStock(connection, item.ProductID, item.Quantity);
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

        private void AddSupplyItem(SQLiteConnection connection, int supplyId, SupplyItem item)
        {
            string query = @"
                INSERT INTO ReceiptItems (ReceiptID, ProductID, Quantity, UnitPrice)
                VALUES (@SupplyID, @ProductID, @Quantity, @UnitPrice)";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SupplyID", supplyId);
                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);

                command.ExecuteNonQuery();
            }
        }

        private void UpdateProductStock(SQLiteConnection connection, int productId, int quantity)
        {
            string query = "UPDATE Products SET StockBalance = StockBalance + @Quantity WHERE ProductID = @ProductID";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProductID", productId);
                command.Parameters.AddWithValue("@Quantity", quantity);

                command.ExecuteNonQuery();
            }
        }

        private string GenerateSupplyNumber()
        {
            return "SUP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        public List<Inventory> GetInventoryHistory()
        {
            // Заглушка - в реальном приложении нужно реализовать
            return new List<Inventory>();
        }

        public void CreateInventory(Inventory inventory)
        {
            // Заглушка - реализовать создание инвентаризации
        }
    }
}