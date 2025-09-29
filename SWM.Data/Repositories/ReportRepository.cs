using System.Data.SQLite;
using System.Collections.Generic;
using SWM.Core.Models;

namespace SWM.Data.Repositories
{
    public class ReportRepository : BaseRepository<object>
    {
        public ReportRepository(string connectionString) : base(connectionString) { }

        public SalesReport GetSalesReport(DateTime startDate, DateTime endDate)
        {
            var report = new SalesReport { PeriodStart = startDate, PeriodEnd = endDate };

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        COUNT(*) as TotalOrders,
                        COALESCE(SUM(TotalAmount), 0) as TotalRevenue,
                        COALESCE(SUM((SELECT SUM(Quantity) FROM OrderProducts op WHERE op.OrderID = o.OrderID)), 0) as TotalProductsSold
                    FROM Orders o
                    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            report.TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders"));
                            report.TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
                            report.TotalProductsSold = reader.GetInt32(reader.GetOrdinal("TotalProductsSold"));
                            report.AverageOrderValue = report.TotalOrders > 0 ? report.TotalRevenue / report.TotalOrders : 0;
                        }
                    }
                }
            }

            return report;
        }

        public InventoryReport GetInventoryReport()
        {
            var report = new InventoryReport();

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        COUNT(*) as TotalProducts,
                        SUM(CASE WHEN StockBalance = 0 THEN 1 ELSE 0 END) as OutOfStock,
                        SUM(CASE WHEN StockBalance > 0 AND StockBalance < 10 THEN 1 ELSE 0 END) as LowStock,
                        SUM(Price * StockBalance) as TotalValue
                    FROM Products 
                    WHERE IsActive = 1";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        report.TotalProducts = reader.GetInt32(reader.GetOrdinal("TotalProducts"));
                        report.OutOfStockProducts = reader.GetInt32(reader.GetOrdinal("OutOfStock"));
                        report.LowStockProducts = reader.GetInt32(reader.GetOrdinal("LowStock"));
                        report.TotalInventoryValue = reader.GetDecimal(reader.GetOrdinal("TotalValue"));
                    }
                }
            }

            return report;
        }

        public List<PopularProduct> GetPopularProducts(int topCount = 5)
        {
            var products = new List<PopularProduct>();

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        p.Name as ProductName,
                        p.ArticleNumber,
                        SUM(op.Quantity) as QuantitySold,
                        SUM(op.TotalPrice) as TotalRevenue
                    FROM OrderProducts op
                    JOIN Products p ON op.ProductID = p.ProductID
                    JOIN Orders o ON op.OrderID = o.OrderID
                    WHERE o.OrderDate >= date('now', '-30 days')
                    GROUP BY p.ProductID, p.Name, p.ArticleNumber
                    ORDER BY QuantitySold DESC
                    LIMIT @TopCount";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TopCount", topCount);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new PopularProduct
                            {
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                ArticleNumber = reader.GetString(reader.GetOrdinal("ArticleNumber")),
                                QuantitySold = reader.GetInt32(reader.GetOrdinal("QuantitySold")),
                                TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"))
                            });
                        }
                    }
                }
            }

            return products;
        }
    }
}