using System.Data.SQLite;
using System.Collections.Generic;
using SWM.Core.Models;
using System.Data;

namespace SWM.Data.Repositories
{
    public class ProductRepository : BaseRepository<Product>
    {
        public ProductRepository(string connectionString) : base(connectionString) { }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE IsActive = 1";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductID = reader.GetInt32("ProductID"),
                            ArticleNumber = reader.GetString("ArticleNumber"),
                            Name = reader.GetString("Name"),
                            Price = reader.GetDecimal("Price"),
                            Description = reader.GetString("Description"),
                            StockBalance = reader.GetInt32("StockBalance"),
                            Category = reader.GetString("Category"),
                            Characteristics = reader.GetString("Characteristics"),
                            WarehouseID = reader.GetInt32("WarehouseID")
                        });
                    }
                }
            }

            return products;
        }

        public void AddProduct(Product product)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Products 
                                (ArticleNumber, Name, Price, Description, StockBalance, Category, Characteristics, WarehouseID) 
                                VALUES (@ArticleNumber, @Name, @Price, @Description, @StockBalance, @Category, @Characteristics, @WarehouseID)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ArticleNumber", product.ArticleNumber);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@StockBalance", product.StockBalance);
                    command.Parameters.AddWithValue("@Category", product.Category);
                    command.Parameters.AddWithValue("@Characteristics", product.Characteristics);
                    command.Parameters.AddWithValue("@WarehouseID", product.WarehouseID);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}