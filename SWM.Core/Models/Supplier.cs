namespace SWM.Core.Models
{
    public class Supplier
    {
        public int SupplierID { get; set; }
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public bool IsActive { get; set; } = true;
    }
}