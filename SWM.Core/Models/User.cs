using System;

namespace SWM.Core.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public string FullName => $"{FirstName} {LastName}";
        public string RoleDisplay => GetRoleDisplayName(Role);

        private string GetRoleDisplayName(UserRole role)
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

    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        WarehouseWorker = 3,
        Viewer = 4
    }
}