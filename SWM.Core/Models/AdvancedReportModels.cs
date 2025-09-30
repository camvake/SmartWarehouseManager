using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class FinancialReport
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public List<MonthlyFinancial> MonthlyBreakdown { get; set; } = new List<MonthlyFinancial>();
    }

    public class MonthlyFinancial
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal Margin { get; set; }
    }

    public class SupplierPerformanceReport
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public List<SupplierPerformance> SupplierPerformances { get; set; } = new List<SupplierPerformance>();
    }

    public class SupplierPerformance
    {
        public string SupplierName { get; set; }
        public int TotalDeliveries { get; set; }
        public int OnTimeDeliveries { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageRating { get; set; }
        public int QualityIssues { get; set; }
        public string PerformanceStatus { get; set; }
    }

    public class InventoryTurnoverReport
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal AverageInventory { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal TurnoverRatio { get; set; }
        public List<ProductTurnover> ProductTurnovers { get; set; } = new List<ProductTurnover>();
    }

    public class ProductTurnover
    {
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public decimal AverageStock { get; set; }
        public int UnitsSold { get; set; }
        public decimal TurnoverRatio { get; set; }
        public string TurnoverCategory { get; set; } // Высокий, Средний, Низкий
    }

    public class CustomerAnalysisReport
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int TotalCustomers { get; set; }
        public int RepeatCustomers { get; set; }
        public decimal RepeatCustomerRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<CustomerSegment> CustomerSegments { get; set; } = new List<CustomerSegment>();
    }

    public class CustomerSegment
    {
        public string Segment { get; set; } // VIP, Постоянные, Новые
        public int CustomerCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}