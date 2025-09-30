using System;

namespace SWM.Core.Models
{
    public class Warehouse
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string Email { get; set; }
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public int? ManagerID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Вычисляемые свойства
        public int FreeSpace => Capacity - CurrentOccupancy;
        public double OccupancyPercent => Capacity > 0 ? (CurrentOccupancy * 100.0) / Capacity : 0;
    }
}