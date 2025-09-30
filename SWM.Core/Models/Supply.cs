using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class Receipt
    {
        public int ReceiptID { get; set; }
        public string ReceiptNumber { get; set; }
        public int SupplierID { get; set; }
        public int WarehouseID { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Ожидает";
        public int UserID { get; set; }
        public string InvoiceNumber { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Supplier Supplier { get; set; }
        public Warehouse Warehouse { get; set; }
        public User User { get; set; }
        public List<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();

        // Вычисляемые свойства
        public bool IsReceived => Status == "Доставлен";
        public bool IsOverdue => ExpectedDate.HasValue && ExpectedDate.Value < DateTime.Now && !IsReceived;
    }

    public class ReceiptItem
    {
        public int ReceiptItemID { get; set; }
        public int ReceiptID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Навигационные свойства
        public Receipt Receipt { get; set; }
        public Product Product { get; set; }

        // Вычисляемые свойства
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    }
}