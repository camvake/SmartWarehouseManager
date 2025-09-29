using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using SWM.Core.Models;

namespace SWM.Data.Repositories
{
    public class AdvancedReportRepository : BaseRepository<object>
    {
        public AdvancedReportRepository(string connectionString) : base(connectionString) { }

        public List<SalesTrend> GetSalesTrends(ReportFilter filter)
        {
            var trends = new List<SalesTrend>();

            using (var connection = GetConnection())
            {
                connection.Open();

                string periodFormat = GetPeriodFormat(filter.PeriodType);
                string query = $@"
                    SELECT 
                        strftime('{periodFormat}', o.OrderDate) as Period,
                        COUNT(o.OrderID) as OrdersCount,
                        SUM(o.TotalAmount) as Revenue,
                        SUM((SELECT SUM(Quantity) FROM OrderProducts op WHERE op.OrderID = o.OrderID)) as ProductsSold
                    FROM Orders o
                    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                    GROUP BY strftime('{periodFormat}', o.OrderDate)
                    ORDER BY o.OrderDate";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", filter.StartDate);
                    command.Parameters.AddWithValue("@EndDate", filter.EndDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"));
                            var ordersCount = reader.GetInt32(reader.GetOrdinal("OrdersCount"));

                            trends.Add(new SalesTrend
                            {
                                PeriodDisplay = reader.GetString(reader.GetOrdinal("Period")),
                                Revenue = revenue,
                                OrdersCount = ordersCount,
                                ProductsSold = reader.GetInt32(reader.GetOrdinal("ProductsSold")),
                                AverageOrderValue = ordersCount > 0 ? revenue / ordersCount : 0
                            });
                        }
                    }
                }
            }

            return trends;
        }

        public List<ProductPerformance> GetProductPerformance(ReportFilter filter)
        {
            var performance = new List<ProductPerformance>();

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        p.ProductID,
                        p.Name as ProductName,
                        p.ArticleNumber,
                        p.Category,
                        p.StockBalance as CurrentStock,
                        (p.Price * p.StockBalance) as StockValue,
                        COALESCE(SUM(op.Quantity), 0) as QuantitySold,
                        COALESCE(SUM(op.TotalPrice), 0) as TotalRevenue
                    FROM Products p
                    LEFT JOIN OrderProducts op ON p.ProductID = op.ProductID
                    LEFT JOIN Orders o ON op.OrderID = o.OrderID 
                        AND o.OrderDate BETWEEN @StartDate AND @EndDate
                    WHERE p.IsActive = 1
                    GROUP BY p.ProductID, p.Name, p.ArticleNumber, p.Category, p.StockBalance, p.Price
                    ORDER BY TotalRevenue DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", filter.StartDate);
                    command.Parameters.AddWithValue("@EndDate", filter.EndDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var quantitySold = reader.GetInt32(reader.GetOrdinal("QuantitySold"));
                            var currentStock = reader.GetInt32(reader.GetOrdinal("CurrentStock"));
                            var totalSold = quantitySold + currentStock;

                            performance.Add(new ProductPerformance
                            {
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                ArticleNumber = reader.GetString(reader.GetOrdinal("ArticleNumber")),
                                Category = reader.GetString(reader.GetOrdinal("Category")),
                                QuantitySold = quantitySold,
                                TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue")),
                                CurrentStock = currentStock,
                                StockValue = reader.GetDecimal(reader.GetOrdinal("StockValue")),
                                TurnoverRate = totalSold > 0 ? (decimal)quantitySold / totalSold * 100 : 0,
                                ProfitMargin = 25.0m // Заглушка - в реальном приложении считать из себестоимости
                            });
                        }
                    }
                }
            }

            return performance;
        }

        public List<SupplierAnalysis> GetSupplierAnalysis(ReportFilter filter)
        {
            var analysis = new List<SupplierAnalysis>();

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = @"
            SELECT 
                s.SupplierID,
                s.Name as SupplierName,
                COUNT(DISTINCT r.ReceiptID) as DeliveriesCount,
                COALESCE(SUM(r.Quantity * p.Price), 0) as TotalDeliveredValue,
                COUNT(DISTINCT r.ProductID) as ProductsSupplied
            FROM Suppliers s
            LEFT JOIN Receipts r ON s.SupplierID = r.SupplierID 
                AND r.ReceiptDate BETWEEN @StartDate AND @EndDate
            LEFT JOIN Products p ON r.ProductID = p.ProductID
            GROUP BY s.SupplierID, s.Name
            ORDER BY TotalDeliveredValue DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", filter.StartDate);
                    command.Parameters.AddWithValue("@EndDate", filter.EndDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var deliveriesCount = reader.GetInt32(reader.GetOrdinal("DeliveriesCount"));

                            analysis.Add(new SupplierAnalysis
                            {
                                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                                SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
                                DeliveriesCount = deliveriesCount,
                                TotalDeliveredValue = reader.GetDecimal(reader.GetOrdinal("TotalDeliveredValue")),
                                ProductsSupplied = reader.GetInt32(reader.GetOrdinal("ProductsSupplied")),
                                AverageDeliveryTime = 7.0m, // Заглушка
                                ReliabilityScore = deliveriesCount > 0 ? 85.0m : 0 // Заглушка
                            });
                        }
                    }
                }
            }

            return analysis;
        }

        public FinancialReport GetFinancialReport(ReportFilter filter)
        {
            var report = new FinancialReport
            {
                PeriodStart = filter.StartDate,
                PeriodEnd = filter.EndDate
            };

            using (var connection = GetConnection())
            {
                connection.Open();

                // Выручка
                string revenueQuery = @"
                    SELECT COALESCE(SUM(TotalAmount), 0) as TotalRevenue
                    FROM Orders 
                    WHERE OrderDate BETWEEN @StartDate AND @EndDate";

                using (var command = new SQLiteCommand(revenueQuery, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", filter.StartDate);
                    command.Parameters.AddWithValue("@EndDate", filter.EndDate);
                    report.TotalRevenue = Convert.ToDecimal(command.ExecuteScalar());
                }

                // Себестоимость (упрощенно - 75% от выручки)
                report.TotalCost = report.TotalRevenue * 0.75m;

                // Операционные расходы (упрощенно - 15% от выручки)
                report.OperatingExpenses = report.TotalRevenue * 0.15m;
            }

            return report;
        }

        private string GetPeriodFormat(ReportPeriod periodType)
        {
            return periodType switch
            {
                ReportPeriod.Daily => "%Y-%m-%d",
                ReportPeriod.Weekly => "%Y-%W",
                ReportPeriod.Monthly => "%Y-%m",
                ReportPeriod.Quarterly => "%Y-%m",
                ReportPeriod.Yearly => "%Y",
                _ => "%Y-%m"
            };
        }
    }
}