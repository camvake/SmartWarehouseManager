using System;

namespace SWM.Core.Models
{
    public class StockMovement
    {
        public int MovementID { get; set; }
        public int ProductID { get; set; }
        public int WarehouseID { get; set; }
        public MovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public int? ReferenceID { get; set; }
        public string ReferenceType { get; set; }
        public DateTime MovementDate { get; set; }
        public int UserID { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
        public User User { get; set; }

        // Вычисляемые свойства
        public string MovementTypeDisplay => MovementType switch
        {
            MovementType.In => "Приход",
            MovementType.Out => "Расход",
            MovementType.Transfer => "Перемещение",
            MovementType.Adjustment => "Корректировка",
            _ => "Неизвестно"
        };

        public string Direction => MovementType == MovementType.In ? "➕" :
                                 MovementType == MovementType.Out ? "➖" : "🔄";
    }

    public enum MovementType
    {
        In = 1,      // Приход
        Out = 2,     // Расход
        Transfer = 3, // Перемещение
        Adjustment = 4 // Корректировка
    }
}