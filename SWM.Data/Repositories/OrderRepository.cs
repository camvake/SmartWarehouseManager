using System.Data.SQLite;
using System.Collections.Generic;
using SWM.Core.Models;

namespace SWM.Data.Repositories
{
    public class OrderRepository : BaseRepository<Order>
    {
        public OrderRepository(string connectionString) : base(connectionString) { }

        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT o.*, s.StatusName, p.PaymentMethodName, u.FirstName || ' ' || u.LastName as CustomerName
                    FROM Orders o
                    LEFT JOIN Statuses s ON o.StatusID = s.StatusID
                    LEFT JOIN PaymentMethods p ON o.PaymentMethodID = p.PaymentMethodID
                    LEFT JOIN Users u ON o.UserID = u.UserID
                    ORDER BY o.OrderDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                            OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                            OrderContent = reader.GetString(reader.GetOrdinal("OrderContent")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                            StatusID = reader.GetInt32(reader.GetOrdinal("StatusID")),
                            StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
                            PaymentMethodID = reader.GetInt32(reader.GetOrdinal("PaymentMethodID")),
                            PaymentMethodName = reader.GetString(reader.GetOrdinal("PaymentMethodName")),
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"))
                        });
                    }
                }
            }

            return orders;
        }

        public void CreateOrder(Order order)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // Создаем заказ
                string orderQuery = @"
                    INSERT INTO Orders (OrderNumber, OrderContent, TotalAmount, DeliveryAddress, OrderDate, StatusID, PaymentMethodID, UserID)
                    VALUES (@OrderNumber, @OrderContent, @TotalAmount, @DeliveryAddress, @OrderDate, @StatusID, @PaymentMethodID, @UserID);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(orderQuery, connection))
                {
                    command.Parameters.AddWithValue("@OrderNumber", GenerateOrderNumber());
                    command.Parameters.AddWithValue("@OrderContent", order.OrderContent);
                    command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                    command.Parameters.AddWithValue("@DeliveryAddress", order.DeliveryAddress);
                    command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    command.Parameters.AddWithValue("@StatusID", 1); // Новый
                    command.Parameters.AddWithValue("@PaymentMethodID", order.PaymentMethodID);
                    command.Parameters.AddWithValue("@UserID", order.UserID);

                    var orderId = Convert.ToInt32(command.ExecuteScalar());

                    // Добавляем товары в заказ
                    foreach (var item in order.OrderItems)
                    {
                        AddOrderItem(connection, orderId, item);
                    }
                }
            }
        }

        private void AddOrderItem(SQLiteConnection connection, int orderId, OrderItem item)
        {
            string itemQuery = @"
                INSERT INTO OrderProducts (OrderID, ProductID, Quantity, UnitPrice, TotalPrice)
                VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";

            using (var command = new SQLiteCommand(itemQuery, connection))
            {
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);

                command.ExecuteNonQuery();
            }
        }

        private string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        public void UpdateOrderStatus(int orderId, int statusId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "UPDATE Orders SET StatusID = @StatusID WHERE OrderID = @OrderID";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    command.Parameters.AddWithValue("@StatusID", statusId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}