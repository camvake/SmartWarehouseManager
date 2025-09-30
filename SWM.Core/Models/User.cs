using System;

namespace SWM.Core.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public int? WarehouseID { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public Warehouse Warehouse { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string RoleDisplay => Role.GetDisplayName();
    }

    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        WarehouseWorker = 3,
        Viewer = 4
    }

    public static class UserRoleExtensions
    {
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Администратор",
                UserRole.Manager => "Менеджер",
                UserRole.WarehouseWorker => "Работник склада",
                UserRole.Viewer => "Наблюдатель",
                _ => "Неизвестно"
            };
        }
    }
}