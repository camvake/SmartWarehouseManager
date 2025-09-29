using System;

namespace SWM.Core.Models
{
    public class SalesReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProductsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class InventoryReport
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; } // Меньше 10 шт
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }

    public class PopularProduct
    {
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}