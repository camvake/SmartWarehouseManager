using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Linq;

namespace SWM.Core.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;

        public NotificationService(string connectionString)
        {
            _notificationRepository = new NotificationRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);
            _orderRepository = new OrderRepository(connectionString);
        }

        public void CheckStockNotifications()
        {
            var lowStockProducts = _productRepository.GetAllProducts()
                .Where(p => p.StockBalance < 10 && p.StockBalance > 0)
                .ToList();

            var outOfStockProducts = _productRepository.GetAllProducts()
                .Where(p => p.StockBalance == 0)
                .ToList();

            // Уведомления о низком запасе
            foreach (var product in lowStockProducts)
            {
                var existingNotification = _notificationRepository.GetUnreadNotifications()
                    .FirstOrDefault(n => n.RelatedEntityID == product.ProductID &&
                                       n.Type == NotificationType.Stock);

                if (existingNotification == null)
                {
                    var notification = new Notification
                    {
                        Title = "📦 Низкий запас товара",
                        Message = $"Товар '{product.Name}' заканчивается. Осталось: {product.StockBalance} шт.",
                        Type = NotificationType.Stock,
                        Priority = product.StockBalance < 5 ? NotificationPriority.High : NotificationPriority.Medium,
                        RelatedEntityID = product.ProductID,
                        RelatedEntityType = "Product"
                    };

                    _notificationRepository.CreateNotification(notification);
                }
            }

            // Уведомления о отсутствии товара
            foreach (var product in outOfStockProducts)
            {
                var existingNotification = _notificationRepository.GetUnreadNotifications()
                    .FirstOrDefault(n => n.RelatedEntityID == product.ProductID &&
                                       n.Type == NotificationType.Stock);

                if (existingNotification == null)
                {
                    var notification = new Notification
                    {
                        Title = "🚨 Товар отсутствует",
                        Message = $"Товар '{product.Name}' закончился на складе.",
                        Type = NotificationType.Stock,
                        Priority = NotificationPriority.Critical,
                        RelatedEntityID = product.ProductID,
                        RelatedEntityType = "Product"
                    };

                    _notificationRepository.CreateNotification(notification);
                }
            }
        }

        public void CheckOrderNotifications()
        {
            var pendingOrders = _orderRepository.GetAllOrders()
                .Where(o => o.StatusID == 1) // Новые заказы
                .ToList();

            foreach (var order in pendingOrders)
            {
                var existingNotification = _notificationRepository.GetUnreadNotifications()
                    .FirstOrDefault(n => n.RelatedEntityID == order.OrderID &&
                                       n.Type == NotificationType.Order);

                if (existingNotification == null)
                {
                    var notification = new Notification
                    {
                        Title = "📋 Новый заказ",
                        Message = $"Поступил новый заказ №{order.OrderNumber} на сумму {order.TotalAmount:N2} руб.",
                        Type = NotificationType.Order,
                        Priority = NotificationPriority.Medium,
                        RelatedEntityID = order.OrderID,
                        RelatedEntityType = "Order"
                    };

                    _notificationRepository.CreateNotification(notification);
                }
            }
        }

        public void CreateSystemNotification(string title, string message, NotificationPriority priority = NotificationPriority.Medium)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = NotificationType.System,
                Priority = priority,
                CreatedDate = DateTime.Now
            };

            _notificationRepository.CreateNotification(notification);
        }
    }
}