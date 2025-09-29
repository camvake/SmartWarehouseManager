using System;
using System.Collections.Generic;

namespace SWM.Core.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string OrderContent { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public int StatusID { get; set; }
        public string StatusName { get; set; } // Для удобства
        public int PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; } // Для удобства
        public int UserID { get; set; }
        public string CustomerName { get; set; } // Для отображения
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int OrderProductID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}