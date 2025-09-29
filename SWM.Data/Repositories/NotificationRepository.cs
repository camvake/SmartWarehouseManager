using System.Data.SQLite;
using System.Collections.Generic;
using SWM.Core.Models;

namespace SWM.Data.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>
    {
        public NotificationRepository(string connectionString) : base(connectionString) { }

        public List<Notification> GetUnreadNotifications()
        {
            var notifications = new List<Notification>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM Notifications 
                    WHERE IsRead = 0 
                    ORDER BY CreatedDate DESC 
                    LIMIT 50";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(new Notification
                        {
                            NotificationID = reader.GetInt32(reader.GetOrdinal("NotificationID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Message = reader.GetString(reader.GetOrdinal("Message")),
                            Type = (NotificationType)reader.GetInt32(reader.GetOrdinal("Type")),
                            Priority = (NotificationPriority)reader.GetInt32(reader.GetOrdinal("Priority")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                            RelatedEntityID = reader.IsDBNull(reader.GetOrdinal("RelatedEntityID")) ?
                                            null : (int?)reader.GetInt32(reader.GetOrdinal("RelatedEntityID")),
                            RelatedEntityType = reader.IsDBNull(reader.GetOrdinal("RelatedEntityType")) ?
                                              string.Empty : reader.GetString(reader.GetOrdinal("RelatedEntityType"))
                        });
                    }
                }
            }

            return notifications;
        }

        public void CreateNotification(Notification notification)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Notifications (Title, Message, Type, Priority, CreatedDate, IsRead, RelatedEntityID, RelatedEntityType)
                    VALUES (@Title, @Message, @Type, @Priority, @CreatedDate, @IsRead, @RelatedEntityID, @RelatedEntityType)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", notification.Title);
                    command.Parameters.AddWithValue("@Message", notification.Message);
                    command.Parameters.AddWithValue("@Type", (int)notification.Type);
                    command.Parameters.AddWithValue("@Priority", (int)notification.Priority);
                    command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    command.Parameters.AddWithValue("@IsRead", false);
                    command.Parameters.AddWithValue("@RelatedEntityID",
                        notification.RelatedEntityID.HasValue ? (object)notification.RelatedEntityID.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@RelatedEntityType",
                        string.IsNullOrEmpty(notification.RelatedEntityType) ? DBNull.Value : notification.RelatedEntityType);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void MarkAsRead(int notificationId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "UPDATE Notifications SET IsRead = 1 WHERE NotificationID = @NotificationID";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NotificationID", notificationId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void MarkAllAsRead()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "UPDATE Notifications SET IsRead = 1 WHERE IsRead = 0";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetUnreadCount()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0";

                using (var command = new SQLiteCommand(query, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}