using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class OrderRepository : BaseRepository
    {
        public OrderRepository(string connectionString) : base(connectionString) { }

        public Order GetById(int orderId)
        {
            var sql = @"
                SELECT o.*, w.WarehouseName, os.StatusName, pm.MethodName, u.FirstName, u.LastName
                FROM Orders o
                LEFT JOIN Warehouses w ON o.WarehouseID = w.WarehouseID
                LEFT JOIN OrderStatuses os ON o.StatusID = os.StatusID
                LEFT JOIN PaymentMethods pm ON o.PaymentMethodID = pm.PaymentMethodID
                LEFT JOIN Users u ON o.UserID = u.UserID
                WHERE o.OrderID = @OrderID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@OrderID", orderId)))
            {
                if (reader.Read())
                {
                    var order = MapOrder(reader);
                    order.OrderItems = GetOrderItems(orderId);
                    return order;
                }
            }
            return null;
        }

        public Order GetByNumber(string orderNumber)
        {
            var sql = @"
                SELECT o.*, w.WarehouseName, os.StatusName, pm.MethodName, u.FirstName, u.LastName
                FROM Orders o
                LEFT JOIN Warehouses w ON o.WarehouseID = w.WarehouseID
                LEFT JOIN OrderStatuses os ON o.StatusID = os.StatusID
                LEFT JOIN PaymentMethods pm ON o.PaymentMethodID = pm.PaymentMethodID
                LEFT JOIN Users u ON o.UserID = u.UserID
                WHERE o.OrderNumber = @OrderNumber";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@OrderNumber", orderNumber)))
            {
                if (reader.Read())
                {
                    var order = MapOrder(reader);
                    order.OrderItems = GetOrderItems(Convert.ToInt32(reader["OrderID"]));
                    return order;
                }
            }
            return null;
        }

        public List<Order> GetAll()
        {
            var orders = new List<Order>();
            var sql = @"
                SELECT o.*, w.WarehouseName, os.StatusName, pm.MethodName, u.FirstName, u.LastName
                FROM Orders o
                LEFT JOIN Warehouses w ON o.WarehouseID = w.WarehouseID
                LEFT JOIN OrderStatuses os ON o.StatusID = os.StatusID
                LEFT JOIN PaymentMethods pm ON o.PaymentMethodID = pm.PaymentMethodID
                LEFT JOIN Users u ON o.UserID = u.UserID
                ORDER BY o.OrderDate DESC";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    var order = MapOrder(reader);
                    orders.Add(order);
                }
            }
            return orders;
        }

        public List<Order> GetByStatus(int statusId)
        {
            var orders = new List<Order>();
            var sql = @"
                SELECT o.*, w.WarehouseName, os.StatusName, pm.MethodName, u.FirstName, u.LastName
                FROM Orders o
                LEFT JOIN Warehouses w ON o.WarehouseID = w.WarehouseID
                LEFT JOIN OrderStatuses os ON o.StatusID = os.StatusID
                LEFT JOIN PaymentMethods pm ON o.PaymentMethodID = pm.PaymentMethodID
                LEFT JOIN Users u ON o.UserID = u.UserID
                WHERE o.StatusID = @StatusID
                ORDER BY o.OrderDate DESC";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@StatusID", statusId)))
            {
                while (reader.Read())
                {
                    var order = MapOrder(reader);
                    orders.Add(order);
                }
            }
            return orders;
        }

        public List<Order> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var orders = new List<Order>();
            var sql = @"
                SELECT o.*, w.WarehouseName, os.StatusName, pm.MethodName, u.FirstName, u.LastName
                FROM Orders o
                LEFT JOIN Warehouses w ON o.WarehouseID = w.WarehouseID
                LEFT JOIN OrderStatuses os ON o.StatusID = os.StatusID
                LEFT JOIN PaymentMethods pm ON o.PaymentMethodID = pm.PaymentMethodID
                LEFT JOIN Users u ON o.UserID = u.UserID
                WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                ORDER BY o.OrderDate DESC";

            var parameters = new[]
            {
                new SQLiteParameter("@StartDate", startDate),
                new SQLiteParameter("@EndDate", endDate)
            };

            using (var reader = ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    var order = MapOrder(reader);
                    orders.Add(order);
                }
            }
            return orders;
        }

        public int Create(Order order)
        {
            var sql = @"
                INSERT INTO Orders (OrderNumber, CustomerName, CustomerPhone, CustomerEmail, DeliveryAddress,
                                  WarehouseID, TotalAmount, DiscountAmount, FinalAmount, StatusID, 
                                  PaymentMethodID, UserID, RequiredDate, Notes)
                VALUES (@OrderNumber, @CustomerName, @CustomerPhone, @CustomerEmail, @DeliveryAddress,
                       @WarehouseID, @TotalAmount, @DiscountAmount, @FinalAmount, @StatusID,
                       @PaymentMethodID, @UserID, @RequiredDate, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@OrderNumber", order.OrderNumber),
                new SQLiteParameter("@CustomerName", order.CustomerName),
                new SQLiteParameter("@CustomerPhone", order.CustomerPhone),
                new SQLiteParameter("@CustomerEmail", order.CustomerEmail ?? (object)DBNull.Value),
                new SQLiteParameter("@DeliveryAddress", order.DeliveryAddress),
                new SQLiteParameter("@WarehouseID", order.WarehouseID),
                new SQLiteParameter("@TotalAmount", order.TotalAmount),
                new SQLiteParameter("@DiscountAmount", order.DiscountAmount),
                new SQLiteParameter("@FinalAmount", order.FinalAmount),
                new SQLiteParameter("@StatusID", order.StatusID),
                new SQLiteParameter("@PaymentMethodID", order.PaymentMethodID),
                new SQLiteParameter("@UserID", order.UserID),
                new SQLiteParameter("@RequiredDate", order.RequiredDate ?? (object)DBNull.Value),
                new SQLiteParameter("@Notes", order.Notes ?? (object)DBNull.Value)
            };

            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        public void CreateOrderItem(OrderItem item)
        {
            var sql = @"
                INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice, Discount, TotalPrice)
                VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice, @Discount, @TotalPrice)";

            var parameters = new[]
            {
                new SQLiteParameter("@OrderID", item.OrderID),
                new SQLiteParameter("@ProductID", item.ProductID),
                new SQLiteParameter("@Quantity", item.Quantity),
                new SQLiteParameter("@UnitPrice", item.UnitPrice),
                new SQLiteParameter("@Discount", item.Discount),
                new SQLiteParameter("@TotalPrice", item.TotalPrice)
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void UpdateStatus(int orderId, int statusId)
        {
            var sql = "UPDATE Orders SET StatusID = @StatusID WHERE OrderID = @OrderID";
            ExecuteNonQuery(sql,
                new SQLiteParameter("@StatusID", statusId),
                new SQLiteParameter("@OrderID", orderId));
        }

        public void UpdateShippedDate(int orderId, DateTime shippedDate)
        {
            var sql = "UPDATE Orders SET ShippedDate = @ShippedDate WHERE OrderID = @OrderID";
            ExecuteNonQuery(sql,
                new SQLiteParameter("@ShippedDate", shippedDate),
                new SQLiteParameter("@OrderID", orderId));
        }

        private List<OrderItem> GetOrderItems(int orderId)
        {
            var items = new List<OrderItem>();
            var sql = @"
                SELECT oi.*, p.ProductName, p.ArticleNumber
                FROM OrderItems oi
                LEFT JOIN Products p ON oi.ProductID = p.ProductID
                WHERE oi.OrderID = @OrderID";

            using (var reader = ExecuteReader(sql, new SQLiteParameter("@OrderID", orderId)))
            {
                while (reader.Read())
                {
                    items.Add(new OrderItem
                    {
                        OrderItemID = Convert.ToInt32(reader["OrderItemID"]),
                        OrderID = Convert.ToInt32(reader["OrderID"]),
                        ProductID = Convert.ToInt32(reader["ProductID"]),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                        UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                        Discount = Convert.ToDecimal(reader["Discount"]),
                        TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                        Product = new Product
                        {
                            ProductName = reader["ProductName"].ToString(),
                            ArticleNumber = reader["ArticleNumber"].ToString()
                        }
                    });
                }
            }
            return items;
        }

        private Order MapOrder(SQLiteDataReader reader)
        {
            return new Order
            {
                OrderID = Convert.ToInt32(reader["OrderID"]),
                OrderNumber = reader["OrderNumber"].ToString(),
                CustomerName = reader["CustomerName"].ToString(),
                CustomerPhone = reader["CustomerPhone"].ToString(),
                CustomerEmail = reader["CustomerEmail"]?.ToString(),
                DeliveryAddress = reader["DeliveryAddress"].ToString(),
                WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                DiscountAmount = Convert.ToDecimal(reader["DiscountAmount"]),
                FinalAmount = Convert.ToDecimal(reader["FinalAmount"]),
                StatusID = Convert.ToInt32(reader["StatusID"]),
                PaymentMethodID = Convert.ToInt32(reader["PaymentMethodID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                RequiredDate = reader["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(reader["RequiredDate"]) : null,
                ShippedDate = reader["ShippedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ShippedDate"]) : null,
                Notes = reader["Notes"]?.ToString(),
                Warehouse = new Warehouse { WarehouseName = reader["WarehouseName"]?.ToString() },
                Status = new OrderStatus { StatusName = reader["StatusName"]?.ToString() },
                PaymentMethod = new PaymentMethod { MethodName = reader["MethodName"]?.ToString() },
                User = new User
                {
                    FirstName = reader["FirstName"]?.ToString(),
                    LastName = reader["LastName"]?.ToString()
                }
            };
        }
    }
}