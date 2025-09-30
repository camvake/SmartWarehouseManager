using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class Inventory
    {
        public int InventoryID { get; set; }
        public string InventoryNumber { get; set; }
        public int WarehouseID { get; set; }
        public DateTime InventoryDate { get; set; }
        public string Status { get; set; } = "В процессе";
        public int CreatedBy { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal TotalDiscrepancy { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Warehouse Warehouse { get; set; }
        public User CreatedByUser { get; set; }
        public List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

        // Вычисляемые свойства
        public bool IsCompleted => Status == "Завершена";
        public bool IsInProgress => Status == "В процессе";
        public int ItemsCount => InventoryItems?.Count ?? 0;
        public int DiscrepanciesCount => InventoryItems?.Count(i => i.QuantityDifference != 0) ?? 0;
    }

    public class InventoryItem
    {
        public int InventoryItemID { get; set; }
        public int InventoryID { get; set; }
        public int ProductID { get; set; }
        public int ExpectedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int QuantityDifference { get; set; }
        public decimal CostDifference { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Inventory Inventory { get; set; }
        public Product Product { get; set; }

        // Вычисляемые свойства
        public bool HasDiscrepancy => QuantityDifference != 0;
        public string DiscrepancyType => QuantityDifference > 0 ? "Излишек" : QuantityDifference < 0 ? "Недостача" : "Нет расхождений";
    }
}   