using SWM.Core.Models;
using SWM.Data.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWM.ViewModels
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Product> _availableProducts;

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        public ObservableCollection<Product> AvailableProducts
        {
            get => _availableProducts;
            set
            {
                _availableProducts = value;
                OnPropertyChanged(nameof(AvailableProducts));
            }
        }

        public OrderViewModel(string connectionString)
        {
            _orderRepository = new OrderRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);
            LoadOrders();
            LoadAvailableProducts();
        }

        public void LoadOrders()
        {
            var orders = _orderRepository.GetAllOrders();
            Orders = new ObservableCollection<Order>(orders);
        }

        public void LoadAvailableProducts()
        {
            var products = _productRepository.GetAllProducts();
            AvailableProducts = new ObservableCollection<Product>(products);
        }

        public void CreateOrder(Order order)
        {
            _orderRepository.CreateOrder(order);
            LoadOrders();
        }

        public void UpdateOrderStatus(int orderId, int statusId)
        {
            _orderRepository.UpdateOrderStatus(orderId, statusId);
            LoadOrders();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}