using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class NotificationViewModel : BaseViewModel
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly ProductRepository _productRepository;

        public NotificationViewModel(string connectionString)
        {
            _notificationRepository = new NotificationRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);

            LoadNotificationsCommand = new RelayCommand(LoadNotifications);
            MarkAsReadCommand = new RelayCommand((param) => MarkAsRead(param));
            MarkAllAsReadCommand = new RelayCommand(MarkAllAsRead);
            DeleteNotificationCommand = new RelayCommand((param) => DeleteNotification(param));
            CheckStockAlertsCommand = new RelayCommand(CheckStockAlerts);

            LoadNotifications();

            // Автоматическая проверка уведомлений при запуске
            CheckStockAlerts();
        }

        #region Commands
        public ICommand LoadNotificationsCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand DeleteNotificationCommand { get; }
        public ICommand CheckStockAlertsCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<Notification> _notifications;
        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }

        private ObservableCollection<StockAlert> _stockAlerts;
        public ObservableCollection<StockAlert> StockAlerts
        {
            get => _stockAlerts;
            set => SetProperty(ref _stockAlerts, value);
        }

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set => SetProperty(ref _unreadCount, value);
        }

        private string _notificationFilter = "Все";
        public string NotificationFilter
        {
            get => _notificationFilter;
            set
            {
                SetProperty(ref _notificationFilter, value);
                FilterNotifications();
            }
        }
        #endregion

        #region Methods
        private void LoadNotifications()
        {
            try
            {
                IsLoading = true;
                var notifications = _notificationRepository.GetAllNotifications(limit: 100);
                Notifications = new ObservableCollection<Notification>(notifications);
                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки уведомлений: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void MarkAsRead(object parameter)
        {
            try
            {
                if (parameter is int notificationId)
                {
                    _notificationRepository.MarkAsRead(notificationId);

                    // Обновляем в списке
                    var notification = Notifications.FirstOrDefault(n => n.NotificationID == notificationId);
                    if (notification != null)
                    {
                        notification.IsRead = true;
                        notification.ReadDate = DateTime.Now;
                    }

                    UpdateUnreadCount();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка отметки уведомления как прочитанного: {ex.Message}";
            }
        }

        private void MarkAllAsRead()
        {
            try
            {
                _notificationRepository.MarkAllAsRead();

                // Обновляем все уведомления в списке
                foreach (var notification in Notifications.Where(n => !n.IsRead))
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;
                }

                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка отметки всех уведомлений как прочитанных: {ex.Message}";
            }
        }

        private void DeleteNotification(object parameter)
        {
            try
            {
                if (parameter is int notificationId)
                {
                    _notificationRepository.Delete(notificationId);

                    // Удаляем из списка
                    var notification = Notifications.FirstOrDefault(n => n.NotificationID == notificationId);
                    if (notification != null)
                    {
                        Notifications.Remove(notification);
                    }

                    UpdateUnreadCount();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления уведомления: {ex.Message}";
            }
        }

        private void CheckStockAlerts()
        {
            try
            {
                var lowStockProducts = _productRepository.GetLowStock();
                var stockAlerts = new ObservableCollection<StockAlert>();

                foreach (var product in lowStockProducts)
                {
                    string alertType = product.StockBalance == 0 ? "OutOfStock" : "LowStock";
                    string message = product.StockBalance == 0
                        ? $"Товар {product.ProductName} закончился на складе"
                        : $"Товар {product.ProductName} имеет низкий запас: {product.StockBalance} шт.";

                    stockAlerts.Add(new StockAlert
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        ArticleNumber = product.ArticleNumber,
                        CurrentStock = product.StockBalance,
                        MinStockLevel = product.MinStockLevel,
                        AlertType = alertType,
                        Message = message,
                        AlertDate = DateTime.Now
                    });

                    // Создаем уведомление в БД
                    _notificationRepository.CreateLowStockNotification(new List<StockAlert> {
                        new StockAlert {
                            ProductID = product.ProductID,
                            ProductName = product.ProductName,
                            ArticleNumber = product.ArticleNumber,
                            CurrentStock = product.StockBalance,
                            AlertType = alertType
                        }
                    });
                }

                StockAlerts = stockAlerts;

                // Перезагружаем уведомления чтобы показать новые
                if (stockAlerts.Any())
                {
                    LoadNotifications();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка проверки запасов: {ex.Message}";
            }
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications?.Count(n => !n.IsRead) ?? 0;
        }

        private void FilterNotifications()
        {
            try
            {
                var allNotifications = _notificationRepository.GetAllNotifications(limit: 100);
                var filtered = allNotifications.AsEnumerable();

                if (NotificationFilter != "Все")
                {
                    var notificationType = NotificationFilter switch
                    {
                        "Склад" => NotificationType.Stock,
                        "Заказы" => NotificationType.Order,
                        "Поставки" => NotificationType.Supply,
                        "Инвентаризация" => NotificationType.Inventory,
                        "Система" => NotificationType.System,
                        _ => NotificationType.System
                    };
                    filtered = filtered.Where(n => n.Type == notificationType);
                }

                Notifications = new ObservableCollection<Notification>(filtered);
                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка фильтрации уведомлений: {ex.Message}";
            }
        }
        #endregion
    }
}