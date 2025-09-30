using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class ReportRepository : BaseRepository
    {
        public ReportRepository(string connectionString) : base(connectionString) { }

        public SalesReport GetSalesReport(DateTime fromDate, DateTime toDate)
        {
            var report = new SalesReport
            {
                PeriodFrom = fromDate,
                PeriodTo = toDate
            };

            // Общая статистика
            var statsSql = @"
                SELECT 
                    COUNT(DISTINCT o.OrderID) as TotalOrders,
                    SUM(o.FinalAmount) as TotalRevenue,
                    SUM(oi.Quantity * (oi.UnitPrice - p.PurchasePrice)) as TotalProfit,
                    SUM(oi.Quantity) as TotalProductsSold
                FROM Orders o
                LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                LEFT JOIN Products p ON oi.ProductID = p.ProductID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
                AND o.StatusID IN (5, 6)"; // Доставленные заказы

            using (var reader = ExecuteReader(statsSql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                if (reader.Read())
                {
                    report.TotalOrders = Convert.ToInt32(reader["TotalOrders"]);
                    report.TotalRevenue = reader["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRevenue"]) : 0;
                    report.TotalProfit = reader["TotalProfit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalProfit"]) : 0;
                    report.TotalProductsSold = reader["TotalProductsSold"] != DBNull.Value ? Convert.ToInt32(reader["TotalProductsSold"]) : 0;
                }
            }

            // Продажи по категориям
            var categoriesSql = @"
                SELECT 
                    c.CategoryName,
                    SUM(oi.Quantity) as QuantitySold,
                    SUM(oi.TotalPrice) as TotalRevenue
                FROM OrderItems oi
                LEFT JOIN Orders o ON oi.OrderID = o.OrderID
                LEFT JOIN Products p ON oi.ProductID = p.ProductID
                LEFT JOIN ProductCategories c ON p.CategoryID = c.CategoryID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
                AND o.StatusID IN (5, 6)
                GROUP BY c.CategoryID, c.CategoryName
                ORDER BY TotalRevenue DESC";

            using (var reader = ExecuteReader(categoriesSql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                while (reader.Read())
                {
                    report.SalesByCategories.Add(new SalesByCategory
                    {
                        CategoryName = reader["CategoryName"].ToString(),
                        QuantitySold = Convert.ToInt32(reader["QuantitySold"]),
                        TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"])
                    });
                }
            }

            // Топ товаров
            var topProductsSql = @"
                SELECT 
                    p.ProductName,
                    p.ArticleNumber,
                    SUM(oi.Quantity) as QuantitySold,
                    SUM(oi.TotalPrice) as TotalRevenue,
                    SUM(oi.Quantity * (oi.UnitPrice - p.PurchasePrice)) as Profit
                FROM OrderItems oi
                LEFT JOIN Orders o ON oi.OrderID = o.OrderID
                LEFT JOIN Products p ON oi.ProductID = p.ProductID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
                AND o.StatusID IN (5, 6)
                GROUP BY p.ProductID, p.ProductName, p.ArticleNumber
                ORDER BY TotalRevenue DESC
                LIMIT 10";

            using (var reader = ExecuteReader(topProductsSql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                while (reader.Read())
                {
                    report.TopSellingProducts.Add(new SalesByProduct
                    {
                        ProductName = reader["ProductName"].ToString(),
                        ArticleNumber = reader["ArticleNumber"].ToString(),
                        QuantitySold = Convert.ToInt32(reader["QuantitySold"]),
                        TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"]),
                        Profit = Convert.ToDecimal(reader["Profit"])
                    });
                }
            }

            return report;
        }

        public StockReport GetStockReport()
        {
            var report = new StockReport
            {
                ReportDate = DateTime.Now
            };

            var sql = @"
                SELECT 
                    COUNT(*) as TotalProducts,
                    SUM(CASE WHEN StockBalance <= MinStockLevel THEN 1 ELSE 0 END) as LowStockProducts,
                    SUM(CASE WHEN StockBalance = 0 THEN 1 ELSE 0 END) as OutOfStockProducts,
                    SUM(CASE WHEN StockBalance >= MaxStockLevel THEN 1 ELSE 0 END) as OverstockProducts,
                    SUM(StockBalance * PurchasePrice) as TotalStockValue
                FROM Products 
                WHERE IsActive = 1";

            using (var reader = ExecuteReader(sql))
            {
                if (reader.Read())
                {
                    report.TotalProducts = Convert.ToInt32(reader["TotalProducts"]);
                    report.LowStockProducts = Convert.ToInt32(reader["LowStockProducts"]);
                    report.OutOfStockProducts = Convert.ToInt32(reader["OutOfStockProducts"]);
                    report.OverstockProducts = Convert.ToInt32(reader["OverstockProducts"]);
                    report.TotalStockValue = reader["TotalStockValue"] != DBNull.Value ? Convert.ToDecimal(reader["TotalStockValue"]) : 0;
                }
            }

            // Детали по товарам
            var detailsSql = @"
                SELECT 
                    ProductName,
                    ArticleNumber,
                    StockBalance,
                    MinStockLevel,
                    MaxStockLevel,
                    (StockBalance * PurchasePrice) as StockValue,
                    CASE 
                        WHEN StockBalance = 0 THEN 'Нет в наличии'
                        WHEN StockBalance <= MinStockLevel THEN 'Низкий запас'
                        WHEN StockBalance >= MaxStockLevel THEN 'Избыток'
                        ELSE 'Норма'
                    END as Status
                FROM Products 
                WHERE IsActive = 1
                ORDER BY StockBalance ASC";

            using (var reader = ExecuteReader(detailsSql))
            {
                while (reader.Read())
                {
                    report.StockStatus.Add(new StockStatusItem
                    {
                        ProductName = reader["ProductName"].ToString(),
                        ArticleNumber = reader["ArticleNumber"].ToString(),
                        CurrentStock = Convert.ToInt32(reader["StockBalance"]),
                        MinStockLevel = Convert.ToInt32(reader["MinStockLevel"]),
                        MaxStockLevel = Convert.ToInt32(reader["MaxStockLevel"]),
                        Status = reader["Status"].ToString(),
                        StockValue = Convert.ToDecimal(reader["StockValue"])
                    });
                }
            }

            return report;
        }
    }
}