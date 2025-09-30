using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;
        private readonly WarehouseRepository _warehouseRepository;

        public OrderViewModel(string connectionString)
        {
            _orderRepository = new OrderRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);
            _warehouseRepository = new WarehouseRepository(connectionString);

            // Исправленные команды
            LoadOrdersCommand = new RelayCommand(LoadOrders);
            CreateOrderCommand = new RelayCommand(CreateOrder);
            UpdateOrderStatusCommand = new RelayCommand((param) => UpdateOrderStatus(param));
            AddOrderItemCommand = new RelayCommand(AddOrderItem);
            RemoveOrderItemCommand = new RelayCommand((param) => RemoveOrderItem(param));

            LoadOrders();
            LoadProducts();
            LoadWarehouses();
        }

        #region Commands
        public ICommand LoadOrdersCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand AddOrderItemCommand { get; }
        public ICommand RemoveOrderItemCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<Warehouse> _warehouses;
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        private Order _newOrder = new Order
        {
            OrderDate = DateTime.Now,
            RequiredDate = DateTime.Now.AddDays(3),
            OrderItems = new List<OrderItem>()
        };
        public Order NewOrder
        {
            get => _newOrder;
            set => SetProperty(ref _newOrder, value);
        }

        private OrderItem _newOrderItem = new OrderItem();
        public OrderItem NewOrderItem
        {
            get => _newOrderItem;
            set => SetProperty(ref _newOrderItem, value);
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                if (value != null)
                {
                    NewOrderItem.ProductID = value.ProductID;
                    NewOrderItem.UnitPrice = value.SalePrice;
                    NewOrderItem.Quantity = 1;
                    CalculateOrderItemTotal();
                }
            }
        }

        private string _orderStatusFilter = "Все";
        public string OrderStatusFilter
        {
            get => _orderStatusFilter;
            set
            {
                SetProperty(ref _orderStatusFilter, value);
                FilterOrders();
            }
        }
        #endregion

        #region Methods
        private void LoadOrders()
        {
            try
            {
                IsLoading = true;
                var orders = _orderRepository.GetAll();
                Orders = new ObservableCollection<Order>(orders);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки заказов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAll();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
            }
        }

        private void LoadWarehouses()
        {
            try
            {
                var warehouses = _warehouseRepository.GetActiveWarehouses();
                Warehouses = new ObservableCollection<Warehouse>(warehouses);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки складов: {ex.Message}";
            }
        }

        private void CreateOrder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewOrder.CustomerName) ||
                    string.IsNullOrWhiteSpace(NewOrder.CustomerPhone) ||
                    string.IsNullOrWhiteSpace(NewOrder.DeliveryAddress))
                {
                    ErrorMessage = "Заполните обязательные поля: ФИО клиента, Телефон и Адрес доставки";
                    return;
                }

                if (NewOrder.OrderItems.Count == 0)
                {
                    ErrorMessage = "Добавьте хотя бы один товар в заказ";
                    return;
                }

                // Генерация номера заказа
                NewOrder.OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}";
                NewOrder.StatusID = 1; // Новый
                NewOrder.PaymentMethodID = 1; // Наличные
                NewOrder.UserID = 1; // Текущий пользователь

                // Расчет итогов
                NewOrder.TotalAmount = NewOrder.OrderItems.Sum(i => i.TotalPrice);
                NewOrder.FinalAmount = NewOrder.TotalAmount - NewOrder.DiscountAmount;

                var orderId = _orderRepository.Create(NewOrder);

                // Сохранение позиций заказа
                foreach (var item in NewOrder.OrderItems)
                {
                    item.OrderID = orderId;
                    _orderRepository.CreateOrderItem(item);
                }

                // Обновление списка заказов
                var newOrder = _orderRepository.GetById(orderId);
                Orders.Insert(0, newOrder);

                // Сброс формы
                NewOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    RequiredDate = DateTime.Now.AddDays(3),
                    OrderItems = new List<OrderItem>()
                };
                NewOrderItem = new OrderItem();
                SelectedProduct = null;

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания заказа: {ex.Message}";
            }
        }

        private void AddOrderItem()
        {
            try
            {
                if (SelectedProduct == null || NewOrderItem.Quantity <= 0)
                {
                    ErrorMessage = "Выберите товар и укажите количество";
                    return;
                }

                if (NewOrderItem.Quantity > SelectedProduct.StockBalance)
                {
                    ErrorMessage = $"Недостаточно товара на складе. Доступно: {SelectedProduct.StockBalance}";
                    return;
                }

                var orderItem = new OrderItem
                {
                    ProductID = SelectedProduct.ProductID,
                    Quantity = NewOrderItem.Quantity,
                    UnitPrice = NewOrderItem.UnitPrice,
                    Discount = NewOrderItem.Discount,
                    TotalPrice = NewOrderItem.TotalPrice,
                    Product = SelectedProduct
                };

                NewOrder.OrderItems.Add(orderItem);
                CalculateOrderTotal();

                // Сброс формы добавления товара
                NewOrderItem = new OrderItem();
                SelectedProduct = null;

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка добавления товара: {ex.Message}";
            }
        }

        private void RemoveOrderItem(object parameter)
        {
            try
            {
                if (parameter is OrderItem item)
                {
                    NewOrder.OrderItems.Remove(item);
                    CalculateOrderTotal();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления товара: {ex.Message}";
            }
        }

        private void UpdateOrderStatus(object parameter)
        {
            try
            {
                if (SelectedOrder == null || parameter is not int statusId) return;

                _orderRepository.UpdateStatus(SelectedOrder.OrderID, statusId);

                if (statusId == 5) // Отгружен
                {
                    _orderRepository.UpdateShippedDate(SelectedOrder.OrderID, DateTime.Now);
                }

                // Обновление в списке
                var updatedOrder = _orderRepository.GetById(SelectedOrder.OrderID);
                var index = Orders.IndexOf(SelectedOrder);
                Orders[index] = updatedOrder;
                SelectedOrder = updatedOrder;

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления статуса: {ex.Message}";
            }
        }

        private void CalculateOrderItemTotal()
        {
            if (NewOrderItem.Quantity > 0 && NewOrderItem.UnitPrice > 0)
            {
                NewOrderItem.TotalPrice = NewOrderItem.Quantity * NewOrderItem.UnitPrice * (1 - NewOrderItem.Discount / 100);
            }
        }

        private void CalculateOrderTotal()
        {
            NewOrder.TotalAmount = NewOrder.OrderItems.Sum(i => i.TotalPrice);
            NewOrder.FinalAmount = NewOrder.TotalAmount - NewOrder.DiscountAmount;
        }

        private void FilterOrders()
        {
            try
            {
                var allOrders = _orderRepository.GetAll();
                var filtered = allOrders.AsEnumerable();

                if (OrderStatusFilter != "Все")
                {
                    filtered = filtered.Where(o => o.Status?.StatusName == OrderStatusFilter);
                }

                Orders = new ObservableCollection<Order>(filtered);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка фильтрации заказов: {ex.Message}";
            }
        }
        #endregion
    }
}