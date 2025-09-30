using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class SalesReport
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalProductsSold { get; set; }
        public List<SalesByCategory> SalesByCategories { get; set; } = new List<SalesByCategory>();
        public List<SalesByProduct> TopSellingProducts { get; set; } = new List<SalesByProduct>();
    }

    public class SalesByCategory
    {
        public string CategoryName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SalesByProduct
    {
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class StockReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int OverstockProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<StockStatusItem> StockStatus { get; set; } = new List<StockStatusItem>();
    }

    public class StockStatusItem
    {
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public string Status { get; set; }
        public decimal StockValue { get; set; }
    }

    public class WarehouseReport
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public double OccupancyPercent { get; set; }
        public List<WarehouseActivity> RecentActivity { get; set; } = new List<WarehouseActivity>();
    }

    public class WarehouseActivity
    {
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
    }
}