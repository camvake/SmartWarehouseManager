using SWM.Core.Models;
using SWM.Core.Services;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SWM.ViewModels
{
    public class NotificationViewModel : INotifyPropertyChanged
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationService _notificationService;
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;
        private readonly SupplyRepository _supplyRepository;

        private ObservableCollection<Notification> _notifications;
        private DashboardStats _dashboardStats;
        private int _unreadCount;

        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged(nameof(Notifications));
            }
        }

        public DashboardStats DashboardStats
        {
            get => _dashboardStats;
            set
            {
                _dashboardStats = value;
                OnPropertyChanged(nameof(DashboardStats));
            }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged(nameof(UnreadCount));
            }
        }

        public NotificationViewModel(string connectionString)
        {
            _notificationRepository = new NotificationRepository(connectionString);
            _notificationService = new NotificationService(connectionString);
            _orderRepository = new OrderRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);
            _supplyRepository = new SupplyRepository(connectionString);

            LoadNotifications();
            LoadDashboardStats();
        }

        public void LoadNotifications()
        {
            var notifications = _notificationRepository.GetUnreadNotifications();
            Notifications = new ObservableCollection<Notification>(notifications);
            UnreadCount = _notificationRepository.GetUnreadCount();
        }

        public void LoadDashboardStats()
        {
            var stats = new DashboardStats();

            // Заказы на сегодня
            var today = DateTime.Today;
            var orders = _orderRepository.GetAllOrders();
            stats.PendingOrders = orders.Count(o => o.StatusID == 1); // Новые заказы
            stats.ThisWeekOrders = orders.Count(o => o.OrderDate >= today.AddDays(-7));
            stats.TodayRevenue = orders.Where(o => o.OrderDate.Date == today)
                                     .Sum(o => o.TotalAmount);

            // Товары с низким запасом
            var products = _productRepository.GetAllProducts();
            stats.LowStockProducts = products.Count(p => p.StockBalance < 10 && p.StockBalance > 0);

            // Поставки на сегодня
            var supplies = _supplyRepository.GetAllSupplies();
            stats.TodaySupplies = supplies.Count(s => s.SupplyDate.Date == today);

            // Непрочитанные уведомления
            stats.UnreadNotifications = UnreadCount;

            DashboardStats = stats;
        }

        public void CheckAllNotifications()
        {
            _notificationService.CheckStockNotifications();
            _notificationService.CheckOrderNotifications();
            LoadNotifications();
            LoadDashboardStats();
        }

        public void MarkAsRead(int notificationId)
        {
            _notificationRepository.MarkAsRead(notificationId);
            LoadNotifications();
            LoadDashboardStats();
        }

        public void MarkAllAsRead()
        {
            _notificationRepository.MarkAllAsRead();
            LoadNotifications();
            LoadDashboardStats();
        }

        public void CreateTestNotification()
        {
            _notificationService.CreateSystemNotification("Тестовое уведомление",
                "Это тестовое уведомление для проверки системы");
            LoadNotifications();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}