using System;

namespace SWM.Core.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ArticleNumber { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int StockBalance { get; set; }
        public string Category { get; set; }
        public string Characteristics { get; set; }
        public int WarehouseID { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}