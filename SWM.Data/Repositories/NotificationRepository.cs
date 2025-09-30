using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class NotificationRepository : BaseRepository
    {
        public NotificationRepository(string connectionString) : base(connectionString) { }

        public List<Notification> GetUnreadNotifications(int? userId = null)
        {
            var notifications = new List<Notification>();
            var sql = @"SELECT * FROM Notifications WHERE IsRead = 0";

            if (userId.HasValue)
            {
                sql += " AND (UserID IS NULL OR UserID = @UserID)";
            }

            sql += " ORDER BY CreatedDate DESC";

            var parameters = new List<SQLiteParameter>();
            if (userId.HasValue)
            {
                parameters.Add(new SQLiteParameter("@UserID", userId.Value));
            }

            using (var reader = ExecuteReader(sql, parameters.ToArray()))
            {
                while (reader.Read())
                {
                    notifications.Add(MapNotification(reader));
                }
            }
            return notifications;
        }

        public List<Notification> GetAllNotifications(int? userId = null, int limit = 50)
        {
            var notifications = new List<Notification>();
            var sql = @"SELECT * FROM Notifications WHERE 1=1";

            if (userId.HasValue)
            {
                sql += " AND (UserID IS NULL OR UserID = @UserID)";
            }

            sql += " ORDER BY CreatedDate DESC LIMIT @Limit";

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@Limit", limit)
            };

            if (userId.HasValue)
            {
                parameters.Add(new SQLiteParameter("@UserID", userId.Value));
            }

            using (var reader = ExecuteReader(sql, parameters.ToArray()))
            {
                while (reader.Read())
                {
                    notifications.Add(MapNotification(reader));
                }
            }
            return notifications;
        }

        public void Create(Notification notification)
        {
            var sql = @"
                INSERT INTO Notifications (Title, Message, Type, Priority, RelatedID, RelatedType, UserID)
                VALUES (@Title, @Message, @Type, @Priority, @RelatedID, @RelatedType, @UserID)";

            var parameters = new[]
            {
                new SQLiteParameter("@Title", notification.Title),
                new SQLiteParameter("@Message", notification.Message),
                new SQLiteParameter("@Type", (int)notification.Type),
                new SQLiteParameter("@Priority", (int)notification.Priority),
                new SQLiteParameter("@RelatedID", notification.RelatedID ?? (object)DBNull.Value),
                new SQLiteParameter("@RelatedType", notification.RelatedType ?? (object)DBNull.Value),
                new SQLiteParameter("@UserID", notification.UserID ?? (object)DBNull.Value)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void MarkAsRead(int notificationId)
        {
            var sql = "UPDATE Notifications SET IsRead = 1, ReadDate = CURRENT_TIMESTAMP WHERE NotificationID = @NotificationID";
            ExecuteNonQuery(sql, new SQLiteParameter("@NotificationID", notificationId));
        }

        public void MarkAllAsRead(int? userId = null)
        {
            var sql = "UPDATE Notifications SET IsRead = 1, ReadDate = CURRENT_TIMESTAMP WHERE IsRead = 0";

            if (userId.HasValue)
            {
                sql += " AND (UserID IS NULL OR UserID = @UserID)";
                ExecuteNonQuery(sql, new SQLiteParameter("@UserID", userId.Value));
            }
            else
            {
                ExecuteNonQuery(sql);
            }
        }

        public void Delete(int notificationId)
        {
            var sql = "DELETE FROM Notifications WHERE NotificationID = @NotificationID";
            ExecuteNonQuery(sql, new SQLiteParameter("@NotificationID", notificationId));
        }

        public void DeleteOldNotifications(int daysOld)
        {
            var sql = "DELETE FROM Notifications WHERE CreatedDate < datetime('now', '-' || @Days || ' days')";
            ExecuteNonQuery(sql, new SQLiteParameter("@Days", daysOld));
        }

        // Методы для создания системных уведомлений
        public void CreateLowStockNotification(List<StockAlert> lowStockProducts)
        {
            foreach (var product in lowStockProducts)
            {
                var notification = new Notification
                {
                    Title = "Низкий запас товара",
                    Message = $"Товар {product.ProductName} ({product.ArticleNumber}) имеет низкий запас: {product.CurrentStock} шт.",
                    Type = NotificationType.Stock,
                    Priority = product.AlertType == "OutOfStock" ? NotificationPriority.High : NotificationPriority.Medium,
                    RelatedID = product.ProductID,
                    RelatedType = "Product"
                };
                Create(notification);
            }
        }

        public void CreateOrderStatusNotification(int orderId, string orderNumber, string status)
        {
            var notification = new Notification
            {
                Title = "Изменение статуса заказа",
                Message = $"Статус заказа {orderNumber} изменен на: {status}",
                Type = NotificationType.Order,
                Priority = NotificationPriority.Medium,
                RelatedID = orderId,
                RelatedType = "Order"
            };
            Create(notification);
        }

        public void CreateInventoryDiscrepancyNotification(int inventoryId, string inventoryNumber, int discrepanciesCount)
        {
            var notification = new Notification
            {
                Title = "Расхождения при инвентаризации",
                Message = $"В инвентаризации {inventoryNumber} обнаружено {discrepanciesCount} расхождений",
                Type = NotificationType.Inventory,
                Priority = NotificationPriority.High,
                RelatedID = inventoryId,
                RelatedType = "Inventory"
            };
            Create(notification);
        }

        private Notification MapNotification(SQLiteDataReader reader)
        {
            return new Notification
            {
                NotificationID = Convert.ToInt32(reader["NotificationID"]),
                Title = reader["Title"].ToString(),
                Message = reader["Message"].ToString(),
                Type = (NotificationType)Convert.ToInt32(reader["Type"]),
                Priority = (NotificationPriority)Convert.ToInt32(reader["Priority"]),
                RelatedID = reader["RelatedID"] != DBNull.Value ? Convert.ToInt32(reader["RelatedID"]) : null,
                RelatedType = reader["RelatedType"]?.ToString(),
                UserID = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : null,
                IsRead = Convert.ToBoolean(reader["IsRead"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                ReadDate = reader["ReadDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReadDate"]) : null
            };
        }
    }
}