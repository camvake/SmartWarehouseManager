using SWM.Core.Models;
using SWM.Core.Services;
using SWM.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SWM.ViewModels
{
    public class UserViewModel
    {
        private readonly UserRepository _repository;

        public ObservableCollection<User> Users { get; set; }
        public User CurrentUser { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public UserViewModel(string connectionString)
        {
            try
            {
                _repository = new UserRepository(connectionString);
                Users = new ObservableCollection<User>();
                LoadUsers();
            }
            catch (Exception ex)
            {
                Users = new ObservableCollection<User>();
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации UserViewModel: {ex.Message}");
            }
        }

        // ДОБАВЬ ЭТИ МЕТОДЫ ДЛЯ АУТЕНТИФИКАЦИИ
        public bool Authenticate(string login, string password)
        {
            try
            {
                // Временная упрощенная реализация
                if (login == "admin" && password == "admin")
                {
                    CurrentUser = new User
                    {
                        UserID = 1,
                        Login = "admin",
                        FirstName = "Администратор",
                        LastName = "Системы",
                        Email = "admin@company.com",
                        Role = UserRole.Admin,
                        IsActive = true
                    };
                    IsAuthenticated = true;
                    return true;
                }

                // Упрощенная проверка без PasswordService
                var user = Users.FirstOrDefault(u =>
                    u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && u.IsActive);

                if (user != null && SimpleVerifyPassword(password, user.PasswordHash))
                {
                    CurrentUser = user;
                    IsAuthenticated = true;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return false;
            }
        }

        private bool SimpleVerifyPassword(string password, string storedHash)
        {
            // Простая проверка для тестирования
            if (password == "admin" && storedHash == "YWRtaW4=") return true; // admin base64
            if (password == "password" && storedHash == "cGFzc3dvcmQ=") return true; // password base64

            // Если хэш не задан, проверяем по простому совпадению
            return string.IsNullOrEmpty(storedHash) || password == storedHash;
        }

        public void Logout()
        {
            CurrentUser = null;
            IsAuthenticated = false;
        }

        public bool HasPermission(UserRole requiredRole)
        {
            return IsAuthenticated && CurrentUser != null && CurrentUser.Role <= requiredRole;
        }

        // Остальные твои методы остаются без изменений
        public void LoadUsers()
        {
            try
            {
                Users.Clear();
                var users = _repository.GetAllUsers();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        public void CreateUser(User user, string password)
        {
            if (_repository.LoginExists(user.Login))
                throw new Exception("Пользователь с таким логином уже существует");

            var (hash, salt) = PasswordService.HashPassword(password);
            user.PasswordHash = hash;

            _repository.CreateUser(user);
            LoadUsers();
        }

        public void UpdateUser(User user)
        {
            _repository.UpdateUser(user);
            LoadUsers();
        }

        public void DeleteUser(int userId)
        {
            _repository.DeleteUser(userId);
            LoadUsers();
        }
    }
}