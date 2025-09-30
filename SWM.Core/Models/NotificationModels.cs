using System;

namespace SWM.Core.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public int? RelatedID { get; set; } // ID связанной сущности (заказ, поставка и т.д.)
        public string RelatedType { get; set; } // Тип сущности
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public int? UserID { get; set; } // Если уведомление персональное

        // Вычисляемые свойства
        public string PriorityIcon => Priority switch
        {
            NotificationPriority.Low => "ℹ️",
            NotificationPriority.Medium => "⚠️",
            NotificationPriority.High => "🚨",
            _ => "📢"
        };

        public string TypeIcon => Type switch
        {
            NotificationType.Stock => "📦",
            NotificationType.Order => "📋",
            NotificationType.Supply => "🚚",
            NotificationType.Inventory => "📊",
            NotificationType.System => "⚙️",
            _ => "📢"
        };
    }

    public enum NotificationType
    {
        Stock = 1,
        Order = 2,
        Supply = 3,
        Inventory = 4,
        System = 5
    }

    public enum NotificationPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public class StockAlert
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public string AlertType { get; set; } // "LowStock", "OutOfStock", "Overstock"
        public string Message { get; set; }
        public DateTime AlertDate { get; set; }
    }
}