using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class Inventory
    {
        public int InventoryID { get; set; }
        public string InventoryNumber { get; set; }
        public DateTime InventoryDate { get; set; }
        public int WarehouseID { get; set; }
        public InventoryStatus Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public virtual List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

    public class InventoryItem
    {
        public int InventoryItemID { get; set; }
        public int InventoryID { get; set; }
        public int ProductID { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Difference => ActualQuantity - ExpectedQuantity;
        public string Notes { get; set; }

        // Навигационные свойства
        public virtual Inventory Inventory { get; set; }
        public virtual Product Product { get; set; }

        // Вычисляемые свойства для отображения в форме
        public string ProductName => Product?.Name;
        public string ArticleNumber => Product?.ArticleNumber;
    }

    public enum InventoryStatus
    {
        Draft = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}