using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string DeliveryAddress { get; set; }
        public int WarehouseID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int StatusID { get; set; }
        public int PaymentMethodID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Warehouse Warehouse { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public User User { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Вычисляемые свойства
        public bool IsShipped => ShippedDate.HasValue;
        public bool IsUrgent => RequiredDate.HasValue && RequiredDate.Value.Date == DateTime.Today.AddDays(1);
    }

    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }

        // Навигационные свойства
        public Order Order { get; set; }
        public Product Product { get; set; }
    }

    public class OrderStatus
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PaymentMethod
    {
        public int PaymentMethodID { get; set; }
        public string MethodName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}