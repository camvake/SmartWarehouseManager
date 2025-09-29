using System;
using System.Collections.Generic;
using ProductModel = SWM.Core.Models.Product;

namespace SWM.Core.Models
{
    public class Supply
    {
        public int SupplyID { get; set; }
        public string SupplyNumber { get; set; }
        public DateTime SupplyDate { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
        public SupplyStatus Status { get; set; }
        public List<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();

        // Вычисляемые свойства
        public string StatusDisplay => GetStatusDisplay();
        public int TotalItems => SupplyItems.Sum(x => x.Quantity);

        private string GetStatusDisplay()
        {
            return Status switch
            {
                SupplyStatus.Pending => "⏳ Ожидает",
                SupplyStatus.Received => "✅ Принята",
                SupplyStatus.PartiallyReceived => "⚠️ Частично",
                SupplyStatus.Cancelled => "❌ Отменена",
                _ => "Неизвестно"
            };
        }
    }

    public class SupplyItem
    {
        public int SupplyItemID { get; set; }
        public int SupplyID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public enum SupplyStatus
    {
        Pending = 1,           // Ожидает поставки
        Received = 2,          // Полностью получена
        PartiallyReceived = 3, // Частично получена
        Cancelled = 4          // Отменена
    }

}