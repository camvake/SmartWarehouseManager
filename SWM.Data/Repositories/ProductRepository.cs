using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class ProductRepository : BaseRepository
    {
        public ProductRepository(string connectionString) : base(connectionString) { }

        public Product GetById(int productId)
        {
            var sql = @"
                SELECT p.*, c.CategoryName, s.SupplierName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.ProductID = @ProductID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@ProductID", productId)))
            {
                if (reader.Read())
                {
                    return MapProduct(reader);
                }
            }
            return null;
        }

        public Product GetByArticle(string articleNumber)
        {
            var sql = @"
                SELECT p.*, c.CategoryName, s.SupplierName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.ArticleNumber = @ArticleNumber";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@ArticleNumber", articleNumber)))
            {
                if (reader.Read())
                {
                    return MapProduct(reader);
                }
            }
            return null;
        }

        public List<Product> GetAll()
        {
            var products = new List<Product>();
            var sql = @"
                SELECT p.*, c.CategoryName, s.SupplierName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.IsActive = 1
                ORDER BY p.ProductName";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    products.Add(MapProduct(reader));
                }
            }
            return products;
        }

        public List<Product> GetByCategory(int categoryId)
        {
            var products = new List<Product>();
            var sql = @"
                SELECT p.*, c.CategoryName, s.SupplierName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.CategoryID = @CategoryID AND p.IsActive = 1";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@CategoryID", categoryId)))
            {
                while (reader.Read())
                {
                    products.Add(MapProduct(reader));
                }
            }
            return products;
        }

        public List<Product> GetLowStock()
        {
            var products = new List<Product>();
            var sql = @"
                SELECT p.*, c.CategoryName, s.SupplierName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.StockBalance <= p.MinStockLevel AND p.IsActive = 1";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    products.Add(MapProduct(reader));
                }
            }
            return products;
        }

        public int Create(Product product)
        {
            var sql = @"
                INSERT INTO Products (
                    ArticleNumber, ProductName, Description, CategoryID, SupplierID,
                    PurchasePrice, SalePrice, StockBalance, MinStockLevel, MaxStockLevel, 
                    UnitOfMeasure, Weight, Dimensions, Barcode, ImageURL, Characteristics
                ) VALUES (
                    @ArticleNumber, @ProductName, @Description, @CategoryID, @SupplierID,
                    @PurchasePrice, @SalePrice, @StockBalance, @MinStockLevel, @MaxStockLevel,
                    @UnitOfMeasure, @Weight, @Dimensions, @Barcode, @ImageURL, @Characteristics
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@ArticleNumber", product.ArticleNumber),
                new SQLiteParameter("@ProductName", product.ProductName),
                new SQLiteParameter("@Description", (object)product.Description ?? DBNull.Value),
                new SQLiteParameter("@CategoryID", (object)product.CategoryID ?? DBNull.Value),
                new SQLiteParameter("@SupplierID", (object)product.SupplierID ?? DBNull.Value),
                new SQLiteParameter("@PurchasePrice", product.PurchasePrice),
                new SQLiteParameter("@SalePrice", product.SalePrice),
                new SQLiteParameter("@StockBalance", product.StockBalance),
                new SQLiteParameter("@MinStockLevel", product.MinStockLevel),
                new SQLiteParameter("@MaxStockLevel", product.MaxStockLevel),
                new SQLiteParameter("@UnitOfMeasure", product.UnitOfMeasure),
                new SQLiteParameter("@Weight", (object)product.Weight ?? DBNull.Value),
                new SQLiteParameter("@Dimensions", (object)product.Dimensions ?? DBNull.Value),
                new SQLiteParameter("@Barcode", (object)product.Barcode ?? DBNull.Value),
                new SQLiteParameter("@ImageURL", (object)product.ImageURL ?? DBNull.Value),
                new SQLiteParameter("@Characteristics", (object)product.Characteristics ?? DBNull.Value)
            };

            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        public void Update(Product product)
        {
            var sql = @"
                UPDATE Products 
                SET ArticleNumber = @ArticleNumber, 
                    ProductName = @ProductName, 
                    Description = @Description,
                    CategoryID = @CategoryID, 
                    SupplierID = @SupplierID, 
                    PurchasePrice = @PurchasePrice,
                    SalePrice = @SalePrice, 
                    StockBalance = @StockBalance,
                    MinStockLevel = @MinStockLevel, 
                    MaxStockLevel = @MaxStockLevel,
                    UnitOfMeasure = @UnitOfMeasure, 
                    Weight = @Weight, 
                    Dimensions = @Dimensions,
                    Barcode = @Barcode, 
                    ImageURL = @ImageURL, 
                    Characteristics = @Characteristics,
                    UpdatedDate = CURRENT_TIMESTAMP
                WHERE ProductID = @ProductID";

            var parameters = new[]
            {
                new SQLiteParameter("@ArticleNumber", product.ArticleNumber),
                new SQLiteParameter("@ProductName", product.ProductName),
                new SQLiteParameter("@Description", (object)product.Description ?? DBNull.Value),
                new SQLiteParameter("@CategoryID", (object)product.CategoryID ?? DBNull.Value),
                new SQLiteParameter("@SupplierID", (object)product.SupplierID ?? DBNull.Value),
                new SQLiteParameter("@PurchasePrice", product.PurchasePrice),
                new SQLiteParameter("@SalePrice", product.SalePrice),
                new SQLiteParameter("@StockBalance", product.StockBalance),
                new SQLiteParameter("@MinStockLevel", product.MinStockLevel),
                new SQLiteParameter("@MaxStockLevel", product.MaxStockLevel),
                new SQLiteParameter("@UnitOfMeasure", product.UnitOfMeasure),
                new SQLiteParameter("@Weight", (object)product.Weight ?? DBNull.Value),
                new SQLiteParameter("@Dimensions", (object)product.Dimensions ?? DBNull.Value),
                new SQLiteParameter("@Barcode", (object)product.Barcode ?? DBNull.Value),
                new SQLiteParameter("@ImageURL", (object)product.ImageURL ?? DBNull.Value),
                new SQLiteParameter("@Characteristics", (object)product.Characteristics ?? DBNull.Value),
                new SQLiteParameter("@ProductID", product.ProductID)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void UpdateStock(int productId, int newStock)
        {
            var sql = @"
                UPDATE Products 
                SET StockBalance = @StockBalance, 
                    UpdatedDate = CURRENT_TIMESTAMP 
                WHERE ProductID = @ProductID";

            ExecuteNonQuery(sql,
                new SQLiteParameter("@StockBalance", newStock),
                new SQLiteParameter("@ProductID", productId));
        }

        public void Delete(int productId)
        {
            var sql = @"
                UPDATE Products 
                SET IsActive = 0, 
                    UpdatedDate = CURRENT_TIMESTAMP 
                WHERE ProductID = @ProductID";

            ExecuteNonQuery(sql, new SQLiteParameter("@ProductID", productId));
        }

        private Product MapProduct(SQLiteDataReader reader)
        {
            return new Product
            {
                ProductID = GetInt32(reader, "ProductID"),
                ArticleNumber = GetString(reader, "ArticleNumber"),
                ProductName = GetString(reader, "ProductName"),
                Description = GetString(reader, "Description"),
                CategoryID = GetNullableInt32(reader, "CategoryID"),
                SupplierID = GetNullableInt32(reader, "SupplierID"),
                PurchasePrice = GetDecimal(reader, "PurchasePrice"),
                SalePrice = GetDecimal(reader, "SalePrice"),
                StockBalance = GetInt32(reader, "StockBalance"),
                MinStockLevel = GetInt32(reader, "MinStockLevel"),
                MaxStockLevel = GetInt32(reader, "MaxStockLevel"),
                UnitOfMeasure = GetString(reader, "UnitOfMeasure"),
                Weight = GetNullableDecimal(reader, "Weight"),
                Dimensions = GetString(reader, "Dimensions"),
                Barcode = GetString(reader, "Barcode"),
                ImageURL = GetString(reader, "ImageURL"),
                Characteristics = GetString(reader, "Characteristics"),
                IsActive = GetBoolean(reader, "IsActive"),
                CreatedDate = GetDateTime(reader, "CreatedDate"),
                UpdatedDate = GetNullableDateTime(reader, "UpdatedDate"),
                Category = new Category
                {
                    CategoryName = GetString(reader, "CategoryName")
                },
                Supplier = new Supplier
                {
                    SupplierName = GetString(reader, "SupplierName")
                }
            };
        }

        // Вспомогательные методы для безопасного чтения данных
        private string GetString(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        private int GetInt32(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        private int? GetNullableInt32(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        }

        private decimal GetDecimal(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetDecimal(ordinal);
        }

        private decimal? GetNullableDecimal(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
        }

        private bool GetBoolean(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }

        private DateTime GetDateTime(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
        }

        private DateTime? GetNullableDateTime(SQLiteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }
    }
}