using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class SalesTrend
    {
        public DateTime Period { get; set; }
        public string PeriodDisplay { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
        public int ProductsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class ProductPerformance
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public string Category { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal StockValue { get; set; }
        public int CurrentStock { get; set; }
        public decimal TurnoverRate { get; set; } // Оборачиваемость
    }

    public class SupplierAnalysis
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public int DeliveriesCount { get; set; }
        public decimal TotalDeliveredValue { get; set; }
        public decimal AverageDeliveryTime { get; set; } // В днях
        public int ProductsSupplied { get; set; }
        public decimal ReliabilityScore { get; set; } // Оценка надежности 0-100
    }

    public class WarehouseEfficiency
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public int OrdersProcessed { get; set; }
        public decimal SpaceUtilization { get; set; } // % использования
        public decimal EfficiencyScore { get; set; } // Общая эффективность
    }

    public class FinancialReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit => TotalRevenue - TotalCost;
        public decimal GrossMargin => TotalRevenue > 0 ? (GrossProfit / TotalRevenue) * 100 : 0;
        public decimal OperatingExpenses { get; set; }
        public decimal NetProfit => GrossProfit - OperatingExpenses;
        public decimal NetMargin => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
    }

    public class ReportFilter
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public int? CategoryID { get; set; }
        public int? SupplierID { get; set; }
        public int? WarehouseID { get; set; }
        public ReportPeriod PeriodType { get; set; } = ReportPeriod.Monthly;
    }

    public enum ReportPeriod
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Quarterly = 4,
        Yearly = 5
    }
}