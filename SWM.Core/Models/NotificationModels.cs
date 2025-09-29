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
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public int? RelatedEntityID { get; set; } // ID связанной сущности (заказ, поставка и т.д.)
        public string RelatedEntityType { get; set; } // Тип сущности

        // Вычисляемые свойства
        public string PriorityIcon => GetPriorityIcon();
        public string TypeIcon => GetTypeIcon();

        private string GetPriorityIcon()
        {
            return Priority switch
            {
                NotificationPriority.Low => "💡",
                NotificationPriority.Medium => "⚠️",
                NotificationPriority.High => "🚨",
                NotificationPriority.Critical => "🔥",
                _ => "📌"
            };
        }

        private string GetTypeIcon()
        {
            return Type switch
            {
                NotificationType.Stock => "📦",
                NotificationType.Order => "📋",
                NotificationType.Supply => "🚚",
                NotificationType.System => "⚙️",
                NotificationType.Report => "📊",
                _ => "📢"
            };
        }
    }

    public enum NotificationType
    {
        Stock = 1,      // Уведомления о запасах
        Order = 2,      // Уведомления о заказах
        Supply = 3,     // Уведомления о поставках
        System = 4,     // Системные уведомления
        Report = 5      // Отчеты и аналитика
    }

    public enum NotificationPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public class DashboardStats
    {
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public int TodaySupplies { get; set; }
        public int UnreadNotifications { get; set; }
        public decimal TodayRevenue { get; set; }
        public int ThisWeekOrders { get; set; }
    }
}