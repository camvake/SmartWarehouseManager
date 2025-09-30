using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class OrderDetailsViewModel : BaseViewModel
    {
        public OrderDetailsViewModel(string connectionString, int orderId)
        {
            // Упрощенная версия без репозиториев
            LoadOrderCommand = new RelayCommand(LoadOrder);
            UpdateOrderCommand = new RelayCommand(UpdateOrder);
            AddOrderItemCommand = new RelayCommand(AddOrderItem);
            RemoveOrderItemCommand = new RelayCommand((param) => RemoveOrderItem(param));
            PrintOrderCommand = new RelayCommand(PrintOrder);

            OrderId = orderId;
            LoadOrder();
            LoadStatuses();
        }

        #region Commands
        public ICommand LoadOrderCommand { get; }
        public ICommand UpdateOrderCommand { get; }
        public ICommand AddOrderItemCommand { get; }
        public ICommand RemoveOrderItemCommand { get; }
        public ICommand PrintOrderCommand { get; }
        #endregion

        #region Properties
        public int OrderId { get; }

        private Order _order;
        public Order Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        private ObservableCollection<Status> _statuses;
        public ObservableCollection<Status> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        private ObservableCollection<Product> _availableProducts;
        public ObservableCollection<Product> AvailableProducts
        {
            get => _availableProducts;
            set => SetProperty(ref _availableProducts, value);
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
                    NewOrderItem = new OrderItem
                    {
                        ProductID = value.ProductID,
                        UnitPrice = value.SalePrice,
                        Quantity = 1,
                        Product = value
                    };
                    CalculateNewItemTotal();
                }
            }
        }

        private OrderItem _newOrderItem = new OrderItem();
        public OrderItem NewOrderItem
        {
            get => _newOrderItem;
            set => SetProperty(ref _newOrderItem, value);
        }

        private decimal _orderTotal;
        public decimal OrderTotal
        {
            get => _orderTotal;
            set => SetProperty(ref _orderTotal, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
        #endregion

        #region Methods
        private void LoadOrder()
        {
            try
            {
                IsLoading = true;

                // Заглушка - создаем тестовый заказ
                Order = new Order
                {
                    OrderID = OrderId,
                    OrderNumber = $"ORD-{OrderId:0000}",
                    CustomerName = "ООО 'Тестовая компания'",
                    CustomerPhone = "+7 (999) 123-45-67",
                    DeliveryAddress = "г. Москва, ул. Тестовая, д. 1",
                    OrderDate = DateTime.Now,
                    TotalAmount = 15240,
                    DiscountAmount = 0,
                    FinalAmount = 15240,
                    StatusID = 1,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem {
                            Product = new Product { ProductName = "Ноутбук Dell XPS 13" },
                            Quantity = 1,
                            UnitPrice = 89990,
                            TotalPrice = 89990
                        },
                        new OrderItem {
                            Product = new Product { ProductName = "Компьютерная мышь" },
                            Quantity = 2,
                            UnitPrice = 1290,
                            TotalPrice = 2580
                        }
                    }
                };

                CalculateOrderTotal();
                LoadAvailableProducts();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки заказа: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadStatuses()
        {
            try
            {
                // Заглушка - тестовые статусы
                Statuses = new ObservableCollection<Status>
                {
                    new Status { StatusID = 1, StatusName = "Новый" },
                    new Status { StatusID = 2, StatusName = "В обработке" },
                    new Status { StatusID = 3, StatusName = "Подтвержден" },
                    new Status { StatusID = 4, StatusName = "Выполнен" },
                    new Status { StatusID = 5, StatusName = "Отменен" }
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки статусов: {ex.Message}";
            }
        }

        private void LoadAvailableProducts()
        {
            try
            {
                // Заглушка - тестовые товары
                AvailableProducts = new ObservableCollection<Product>
                {
                    new Product { ProductID = 1, ProductName = "Монитор Samsung 27\"", SalePrice = 24990, StockBalance = 5 },
                    new Product { ProductID = 2, ProductName = "Клавиатура механическая", SalePrice = 5490, StockBalance = 10 },
                    new Product { ProductID = 3, ProductName = "Офисный стол", SalePrice = 12990, StockBalance = 3 }
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
            }
        }

        private void UpdateOrder()
        {
            try
            {
                if (Order == null) return;

                if (string.IsNullOrWhiteSpace(Order.CustomerName) ||
                    string.IsNullOrWhiteSpace(Order.CustomerPhone))
                {
                    ErrorMessage = "Заполните обязательные поля: ФИО клиента и Телефон";
                    return;
                }

                // Пересчитываем итоги перед сохранением
                CalculateOrderTotal();
                Order.TotalAmount = OrderTotal;
                Order.FinalAmount = OrderTotal - Order.DiscountAmount;

                IsEditing = false;
                ErrorMessage = null;
                ShowSuccessMessage?.Invoke("Заказ успешно обновлен");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления заказа: {ex.Message}";
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

                var orderItem = new OrderItem
                {
                    OrderID = OrderId,
                    ProductID = SelectedProduct.ProductID,
                    Quantity = NewOrderItem.Quantity,
                    UnitPrice = NewOrderItem.UnitPrice,
                    Discount = NewOrderItem.Discount,
                    TotalPrice = NewOrderItem.TotalPrice,
                    Product = SelectedProduct
                };

                if (Order.OrderItems == null)
                    Order.OrderItems = new List<OrderItem>();

                Order.OrderItems.Add(orderItem);
                CalculateOrderTotal();
                LoadAvailableProducts(); // Обновляем список доступных товаров

                // Сброс формы добавления
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
                    Order.OrderItems?.Remove(item);
                    CalculateOrderTotal();
                    LoadAvailableProducts(); // Обновляем список доступных товаров
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления товара: {ex.Message}";
            }
        }

        private void PrintOrder()
        {
            try
            {
                // Логика печати заказа
                ShowSuccessMessage?.Invoke("Печать заказа запущена");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка печати: {ex.Message}";
            }
        }

        public void CalculateOrderTotal()
        {
            if (Order?.OrderItems != null)
            {
                OrderTotal = Order.OrderItems.Sum(i => i.TotalPrice);
            }
            else
            {
                OrderTotal = 0;
            }
        }

        public void CalculateNewItemTotal()
        {
            if (NewOrderItem.Quantity > 0 && NewOrderItem.UnitPrice > 0)
            {
                // Исправление для оператора ??
                decimal discount = NewOrderItem.Discount ?? 0;
                NewOrderItem.TotalPrice = NewOrderItem.Quantity * NewOrderItem.UnitPrice * (1 - discount / 100);
            }
        }

        // Событие для уведомления об успешных операциях
        public Action<string> ShowSuccessMessage { get; set; }
        #endregion
    }

    // Простые модели для демонстрации
    public class Order
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int StatusID { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public Product Product { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal SalePrice { get; set; }
        public int StockBalance { get; set; }
    }

    public class Status
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; }
    }
}