namespace SWM.Core.Models
{
    public class InventoryItemDisplayDto
    {
        public int InventoryItemID { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int ExpectedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int QuantityDifference { get; set; }
        public decimal CostDifference { get; set; }
        public string DiscrepancyType { get; set; }
        public bool HasDiscrepancy { get; set; }
        public string Notes { get; set; }
        public decimal ProductPrice { get; set; }

        // Вычисляемые свойства для отображения
        public string DifferenceDisplay => QuantityDifference > 0 ? $"+{QuantityDifference}" : QuantityDifference.ToString();
        public string CostDifferenceDisplay => CostDifference.ToString("C2");
        public string StatusColor => QuantityDifference == 0 ? "Green" : QuantityDifference > 0 ? "Orange" : "Red";
    }
}