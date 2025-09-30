// Product.cs в SWM.Core.Models
using System;

namespace SWM.Core.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ArticleNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? CategoryID { get; set; }
        public int? SupplierID { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int StockBalance { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public string UnitOfMeasure { get; set; } = "шт.";
        public decimal? Weight { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public string Characteristics { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Навигационные свойства
        public virtual Category Category { get; set; }
        public virtual Supplier Supplier { get; set; }

        // Вычисляемое свойство
        public bool IsLowStock => StockBalance <= MinStockLevel;
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}