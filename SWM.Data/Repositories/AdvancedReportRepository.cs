using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class AdvancedReportRepository : BaseRepository
    {
        public AdvancedReportRepository(string connectionString) : base(connectionString) { }

        public FinancialReport GetFinancialReport(DateTime fromDate, DateTime toDate)
        {
            var report = new FinancialReport
            {
                PeriodFrom = fromDate,
                PeriodTo = toDate
            };

            // Основные финансовые показатели
            var sql = @"
                SELECT 
                    -- Выручка
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN o.FinalAmount ELSE 0 END) as TotalRevenue,
                    -- Себестоимость
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN oi.Quantity * p.PurchasePrice ELSE 0 END) as TotalCost,
                    -- Операционные расходы (оценочно 15% от выручки)
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN o.FinalAmount * 0.15 ELSE 0 END) as OperatingExpenses
                FROM Orders o
                LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                LEFT JOIN Products p ON oi.ProductID = p.ProductID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                if (reader.Read())
                {
                    report.TotalRevenue = reader["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRevenue"]) : 0;
                    report.TotalCost = reader["TotalCost"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCost"]) : 0;
                    report.OperatingExpenses = reader["OperatingExpenses"] != DBNull.Value ? Convert.ToDecimal(reader["OperatingExpenses"]) : 0;

                    report.GrossProfit = report.TotalRevenue - report.TotalCost;
                    report.NetProfit = report.GrossProfit - report.OperatingExpenses;
                    report.ProfitMargin = report.TotalRevenue > 0 ? (report.NetProfit / report.TotalRevenue) * 100 : 0;
                }
            }

            // Помесячная разбивка
            report.MonthlyBreakdown = GetMonthlyFinancialBreakdown(fromDate, toDate);

            return report;
        }

        public SupplierPerformanceReport GetSupplierPerformanceReport(DateTime fromDate, DateTime toDate)
        {
            var report = new SupplierPerformanceReport
            {
                PeriodFrom = fromDate,
                PeriodTo = toDate
            };

            var sql = @"
                SELECT 
                    s.SupplierName,
                    COUNT(DISTINCT r.ReceiptID) as TotalDeliveries,
                    COUNT(DISTINCT CASE WHEN r.Status = 'Доставлен' AND r.ReceiptDate <= r.ExpectedDate THEN r.ReceiptID END) as OnTimeDeliveries,
                    SUM(r.TotalAmount) as TotalSpent,
                    AVG(s.Rating) as AverageRating,
                    COUNT(DISTINCT CASE WHEN ri.ExpiryDate < CURRENT_DATE THEN ri.ReceiptItemID END) as QualityIssues
                FROM Suppliers s
                LEFT JOIN Receipts r ON s.SupplierID = r.SupplierID
                LEFT JOIN ReceiptItems ri ON r.ReceiptID = ri.ReceiptID
                WHERE r.ReceiptDate BETWEEN @FromDate AND @ToDate
                GROUP BY s.SupplierID, s.SupplierName
                ORDER BY TotalSpent DESC";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                while (reader.Read())
                {
                    var totalDeliveries = Convert.ToInt32(reader["TotalDeliveries"]);
                    var onTimeDeliveries = Convert.ToInt32(reader["OnTimeDeliveries"]);
                    var onTimeRate = totalDeliveries > 0 ? (onTimeDeliveries * 100.0m) / totalDeliveries : 0;

                    report.SupplierPerformances.Add(new SupplierPerformance
                    {
                        SupplierName = reader["SupplierName"].ToString(),
                        TotalDeliveries = totalDeliveries,
                        OnTimeDeliveries = onTimeDeliveries,
                        OnTimeDeliveryRate = onTimeRate,
                        TotalSpent = reader["TotalSpent"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSpent"]) : 0,
                        AverageRating = reader["AverageRating"] != DBNull.Value ? Convert.ToDecimal(reader["AverageRating"]) : 0,
                        QualityIssues = Convert.ToInt32(reader["QualityIssues"]),
                        PerformanceStatus = GetPerformanceStatus(onTimeRate, Convert.ToDecimal(reader["AverageRating"]))
                    });
                }
            }

            return report;
        }

        public InventoryTurnoverReport GetInventoryTurnoverReport(DateTime fromDate, DateTime toDate)
        {
            var report = new InventoryTurnoverReport
            {
                PeriodFrom = fromDate,
                PeriodTo = toDate
            };

            // Средний запас и себестоимость проданных товаров
            var sql = @"
                SELECT 
                    -- Средний запас
                    AVG(p.StockBalance * p.PurchasePrice) as AverageInventory,
                    -- Себестоимость проданных товаров
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN oi.Quantity * p.PurchasePrice ELSE 0 END) as CostOfGoodsSold
                FROM Products p
                LEFT JOIN OrderItems oi ON p.ProductID = oi.ProductID
                LEFT JOIN Orders o ON oi.OrderID = o.OrderID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate OR o.OrderID IS NULL";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                if (reader.Read())
                {
                    report.AverageInventory = reader["AverageInventory"] != DBNull.Value ? Convert.ToDecimal(reader["AverageInventory"]) : 0;
                    report.CostOfGoodsSold = reader["CostOfGoodsSold"] != DBNull.Value ? Convert.ToDecimal(reader["CostOfGoodsSold"]) : 0;
                    report.TurnoverRatio = report.AverageInventory > 0 ? report.CostOfGoodsSold / report.AverageInventory : 0;
                }
            }

            // Оборачиваемость по товарам
            report.ProductTurnovers = GetProductTurnovers(fromDate, toDate);

            return report;
        }

        public CustomerAnalysisReport GetCustomerAnalysisReport(DateTime fromDate, DateTime toDate)
        {
            var report = new CustomerAnalysisReport
            {
                PeriodFrom = fromDate,
                PeriodTo = toDate
            };

            var sql = @"
                WITH CustomerStats AS (
                    SELECT 
                        CustomerPhone,
                        COUNT(*) as OrderCount,
                        SUM(FinalAmount) as TotalSpent,
                        AVG(FinalAmount) as AvgOrderValue
                    FROM Orders 
                    WHERE OrderDate BETWEEN @FromDate AND @ToDate
                    AND StatusID IN (5, 6)
                    GROUP BY CustomerPhone
                )
                SELECT 
                    COUNT(*) as TotalCustomers,
                    COUNT(CASE WHEN OrderCount > 1 THEN 1 END) as RepeatCustomers,
                    AVG(AvgOrderValue) as AverageOrderValue
                FROM CustomerStats";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                if (reader.Read())
                {
                    report.TotalCustomers = Convert.ToInt32(reader["TotalCustomers"]);
                    report.RepeatCustomers = Convert.ToInt32(reader["RepeatCustomers"]);
                    report.AverageOrderValue = reader["AverageOrderValue"] != DBNull.Value ? Convert.ToDecimal(reader["AverageOrderValue"]) : 0;
                    report.RepeatCustomerRate = report.TotalCustomers > 0 ? (report.RepeatCustomers * 100.0m) / report.TotalCustomers : 0;
                }
            }

            // Сегментация клиентов
            report.CustomerSegments = GetCustomerSegments(fromDate, toDate);

            return report;
        }

        private List<MonthlyFinancial> GetMonthlyFinancialBreakdown(DateTime fromDate, DateTime toDate)
        {
            var breakdown = new List<MonthlyFinancial>();

            var sql = @"
                SELECT 
                    strftime('%Y-%m', o.OrderDate) as Month,
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN o.FinalAmount ELSE 0 END) as Revenue,
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN oi.Quantity * p.PurchasePrice ELSE 0 END) as Cost
                FROM Orders o
                LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                LEFT JOIN Products p ON oi.ProductID = p.ProductID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
                GROUP BY strftime('%Y-%m', o.OrderDate)
                ORDER BY Month";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                while (reader.Read())
                {
                    var revenue = reader["Revenue"] != DBNull.Value ? Convert.ToDecimal(reader["Revenue"]) : 0;
                    var cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0;
                    var profit = revenue - cost;
                    var margin = revenue > 0 ? (profit / revenue) * 100 : 0;

                    breakdown.Add(new MonthlyFinancial
                    {
                        Month = reader["Month"].ToString(),
                        Revenue = revenue,
                        Cost = cost,
                        Profit = profit,
                        Margin = margin
                    });
                }
            }

            return breakdown;
        }

        private List<ProductTurnover> GetProductTurnovers(DateTime fromDate, DateTime toDate)
        {
            var turnovers = new List<ProductTurnover>();

            var sql = @"
                SELECT 
                    p.ProductName,
                    p.ArticleNumber,
                    AVG(p.StockBalance) as AverageStock,
                    SUM(CASE WHEN o.StatusID IN (5, 6) THEN oi.Quantity ELSE 0 END) as UnitsSold
                FROM Products p
                LEFT JOIN OrderItems oi ON p.ProductID = oi.ProductID
                LEFT JOIN Orders o ON oi.OrderID = o.OrderID
                WHERE o.OrderDate BETWEEN @FromDate AND @ToDate OR o.OrderID IS NULL
                GROUP BY p.ProductID, p.ProductName, p.ArticleNumber
                HAVING UnitsSold > 0
                ORDER BY UnitsSold DESC";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                while (reader.Read())
                {
                    var averageStock = Convert.ToDecimal(reader["AverageStock"]);
                    var unitsSold = Convert.ToInt32(reader["UnitsSold"]);
                    var turnoverRatio = averageStock > 0 ? unitsSold / averageStock : 0;

                    turnovers.Add(new ProductTurnover
                    {
                        ProductName = reader["ProductName"].ToString(),
                        ArticleNumber = reader["ArticleNumber"].ToString(),
                        AverageStock = averageStock,
                        UnitsSold = unitsSold,
                        TurnoverRatio = turnoverRatio,
                        TurnoverCategory = GetTurnoverCategory(turnoverRatio)
                    });
                }
            }

            return turnovers;
        }

        private List<CustomerSegment> GetCustomerSegments(DateTime fromDate, DateTime toDate)
        {
            var segments = new List<CustomerSegment>();

            var sql = @"
                WITH CustomerOrders AS (
                    SELECT 
                        CustomerPhone,
                        COUNT(*) as OrderCount,
                        SUM(FinalAmount) as TotalSpent,
                        AVG(FinalAmount) as AvgOrderValue
                    FROM Orders 
                    WHERE OrderDate BETWEEN @FromDate AND @ToDate
                    AND StatusID IN (5, 6)
                    GROUP BY CustomerPhone
                )
                SELECT 
                    CASE 
                        WHEN TotalSpent > 100000 THEN 'VIP'
                        WHEN OrderCount > 1 THEN 'Постоянные'
                        ELSE 'Новые'
                    END as Segment,
                    COUNT(*) as CustomerCount,
                    SUM(TotalSpent) as TotalRevenue,
                    AVG(AvgOrderValue) as AverageOrderValue
                FROM CustomerOrders
                GROUP BY Segment
                ORDER BY TotalRevenue DESC";

            using (var reader = ExecuteReader(sql,
                new SQLiteParameter("@FromDate", fromDate),
                new SQLiteParameter("@ToDate", toDate)))
            {
                while (reader.Read())
                {
                    segments.Add(new CustomerSegment
                    {
                        Segment = reader["Segment"].ToString(),
                        CustomerCount = Convert.ToInt32(reader["CustomerCount"]),
                        TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"]),
                        AverageOrderValue = Convert.ToDecimal(reader["AverageOrderValue"])
                    });
                }
            }

            return segments;
        }

        private string GetPerformanceStatus(decimal onTimeRate, decimal rating)
        {
            if (onTimeRate >= 90 && rating >= 4.5m) return "Отлично";
            if (onTimeRate >= 80 && rating >= 4.0m) return "Хорошо";
            if (onTimeRate >= 70 && rating >= 3.5m) return "Удовлетворительно";
            return "Требует внимания";
        }

        private string GetTurnoverCategory(decimal turnoverRatio)
        {
            if (turnoverRatio > 2.0m) return "Высокий";
            if (turnoverRatio > 1.0m) return "Средний";
            return "Низкий";
        }
    }
}