using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class ReceiptRepository : BaseRepository
    {
        public ReceiptRepository(string connectionString) : base(connectionString) { }

        public Receipt GetById(int receiptId)
        {
            var sql = @"
                SELECT r.*, s.SupplierName, w.WarehouseName, u.FirstName, u.LastName
                FROM Receipts r
                LEFT JOIN Suppliers s ON r.SupplierID = s.SupplierID
                LEFT JOIN Warehouses w ON r.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON r.UserID = u.UserID
                WHERE r.ReceiptID = @ReceiptID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@ReceiptID", receiptId)))
            {
                if (reader.Read())
                {
                    var receipt = MapReceipt(reader);
                    receipt.ReceiptItems = GetReceiptItems(receiptId);
                    return receipt;
                }
            }
            return null;
        }

        public List<Receipt> GetAll()
        {
            var receipts = new List<Receipt>();
            var sql = @"
                SELECT r.*, s.SupplierName, w.WarehouseName, u.FirstName, u.LastName
                FROM Receipts r
                LEFT JOIN Suppliers s ON r.SupplierID = s.SupplierID
                LEFT JOIN Warehouses w ON r.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON r.UserID = u.UserID
                ORDER BY r.ReceiptDate DESC";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    var receipt = MapReceipt(reader);
                    receipts.Add(receipt);
                }
            }
            return receipts;
        }

        public List<Receipt> GetBySupplier(int supplierId)
        {
            var receipts = new List<Receipt>();
            var sql = @"
                SELECT r.*, s.SupplierName, w.WarehouseName, u.FirstName, u.LastName
                FROM Receipts r
                LEFT JOIN Suppliers s ON r.SupplierID = s.SupplierID
                LEFT JOIN Warehouses w ON r.WarehouseID = w.WarehouseID
                LEFT JOIN Users u ON r.UserID = u.UserID
                WHERE r.SupplierID = @SupplierID
                ORDER BY r.ReceiptDate DESC";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@SupplierID", supplierId)))
            {
                while (reader.Read())
                {
                    var receipt = MapReceipt(reader);
                    receipts.Add(receipt);
                }
            }
            return receipts;
        }

        public int Create(Receipt receipt)
        {
            var sql = @"
                INSERT INTO Receipts (ReceiptNumber, SupplierID, WarehouseID, ExpectedDate, 
                                    TotalQuantity, TotalAmount, Status, UserID, InvoiceNumber, Notes)
                VALUES (@ReceiptNumber, @SupplierID, @WarehouseID, @ExpectedDate,
                       @TotalQuantity, @TotalAmount, @Status, @UserID, @InvoiceNumber, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@ReceiptNumber", receipt.ReceiptNumber),
                new SQLiteParameter("@SupplierID", receipt.SupplierID),
                new SQLiteParameter("@WarehouseID", receipt.WarehouseID),
                new SQLiteParameter("@ExpectedDate", receipt.ExpectedDate ?? (object)DBNull.Value),
                new SQLiteParameter("@TotalQuantity", receipt.TotalQuantity),
                new SQLiteParameter("@TotalAmount", receipt.TotalAmount),
                new SQLiteParameter("@Status", receipt.Status),
                new SQLiteParameter("@UserID", receipt.UserID),
                new SQLiteParameter("@InvoiceNumber", receipt.InvoiceNumber ?? (object)DBNull.Value),
                new SQLiteParameter("@Notes", receipt.Notes ?? (object)DBNull.Value)
            };

            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        public void CreateReceiptItem(ReceiptItem item)
        {
            var sql = @"
                INSERT INTO ReceiptItems (ReceiptID, ProductID, Quantity, UnitCost, TotalCost, BatchNumber, ExpiryDate)
                VALUES (@ReceiptID, @ProductID, @Quantity, @UnitCost, @TotalCost, @BatchNumber, @ExpiryDate)";

            var parameters = new[]
            {
                new SQLiteParameter("@ReceiptID", item.ReceiptID),
                new SQLiteParameter("@ProductID", item.ProductID),
                new SQLiteParameter("@Quantity", item.Quantity),
                new SQLiteParameter("@UnitCost", item.UnitCost),
                new SQLiteParameter("@TotalCost", item.TotalCost),
                new SQLiteParameter("@BatchNumber", item.BatchNumber ?? (object)DBNull.Value),
                new SQLiteParameter("@ExpiryDate", item.ExpiryDate ?? (object)DBNull.Value)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void UpdateStatus(int receiptId, string status)
        {
            var sql = "UPDATE Receipts SET Status = @Status WHERE ReceiptID = @ReceiptID";
            ExecuteNonQuery(sql,
                new SQLiteParameter("@Status", status),
                new SQLiteParameter("@ReceiptID", receiptId));
        }

        private List<ReceiptItem> GetReceiptItems(int receiptId)
        {
            var items = new List<ReceiptItem>();
            var sql = @"
                SELECT ri.*, p.ProductName, p.ArticleNumber
                FROM ReceiptItems ri
                LEFT JOIN Products p ON ri.ProductID = p.ProductID
                WHERE ri.ReceiptID = @ReceiptID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@ReceiptID", receiptId)))
            {
                while (reader.Read())
                {
                    items.Add(new ReceiptItem
                    {
                        ReceiptItemID = Convert.ToInt32(reader["ReceiptItemID"]),
                        ReceiptID = Convert.ToInt32(reader["ReceiptID"]),
                        ProductID = Convert.ToInt32(reader["ProductID"]),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                        UnitCost = Convert.ToDecimal(reader["UnitCost"]),
                        TotalCost = Convert.ToDecimal(reader["TotalCost"]),
                        BatchNumber = reader["BatchNumber"]?.ToString(),
                        ExpiryDate = reader["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(reader["ExpiryDate"]) : null,
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

        private Receipt MapReceipt(SQLiteDataReader reader)
        {
            return new Receipt
            {
                ReceiptID = Convert.ToInt32(reader["ReceiptID"]),
                ReceiptNumber = reader["ReceiptNumber"].ToString(),
                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                ReceiptDate = Convert.ToDateTime(reader["ReceiptDate"]),
                ExpectedDate = reader["ExpectedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ExpectedDate"]) : null,
                TotalQuantity = Convert.ToInt32(reader["TotalQuantity"]),
                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                Status = reader["Status"].ToString(),
                UserID = Convert.ToInt32(reader["UserID"]),
                InvoiceNumber = reader["InvoiceNumber"]?.ToString(),
                Notes = reader["Notes"]?.ToString(),
                Supplier = new Supplier { SupplierName = reader["SupplierName"]?.ToString() },
                Warehouse = new Warehouse { WarehouseName = reader["WarehouseName"]?.ToString() },
                User = new User
                {
                    FirstName = reader["FirstName"]?.ToString(),
                    LastName = reader["LastName"]?.ToString()
                }
            };
        }
    }
}