using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly WarehouseRepository _warehouseRepository;

        public UserViewModel(string connectionString)
        {
            _userRepository = new UserRepository(connectionString);
            _warehouseRepository = new WarehouseRepository(connectionString);

            LoadUsersCommand = new RelayCommand(LoadUsers);
            SaveUserCommand = new RelayCommand(SaveUser);
            DeleteUserCommand = new RelayCommand((param) => DeleteUser(param));
            ResetPasswordCommand = new RelayCommand((param) => ResetPassword(param));

            LoadUsers();
            LoadWarehouses();
        }

        #region Commands
        public ICommand LoadUsersCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private ObservableCollection<Warehouse> _warehouses;
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    EditingUser = new User
                    {
                        UserID = value.UserID,
                        Login = value.Login,
                        Email = value.Email,
                        FirstName = value.FirstName,
                        LastName = value.LastName,
                        PhoneNumber = value.PhoneNumber,
                        Role = value.Role,
                        WarehouseID = value.WarehouseID,
                        IsActive = value.IsActive
                    };
                    ConfirmPassword = "";
                }
                else
                {
                    EditingUser = new User();
                    ConfirmPassword = "";
                }
            }
        }

        private User _editingUser = new User();
        public User EditingUser
        {
            get => _editingUser;
            set => SetProperty(ref _editingUser, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private string _roleFilter = "Все";
        public string RoleFilter
        {
            get => _roleFilter;
            set
            {
                SetProperty(ref _roleFilter, value);
                FilterUsers();
            }
        }
        #endregion

        #region Methods
        private void LoadUsers()
        {
            try
            {
                IsLoading = true;
                var users = _userRepository.GetAll();
                Users = new ObservableCollection<User>(users);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки пользователей: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
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

        private void SaveUser()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditingUser.Login) ||
                    string.IsNullOrWhiteSpace(EditingUser.Email) ||
                    string.IsNullOrWhiteSpace(EditingUser.FirstName) ||
                    string.IsNullOrWhiteSpace(EditingUser.LastName))
                {
                    ErrorMessage = "Заполните обязательные поля: Логин, Email, Имя и Фамилия";
                    return;
                }

                // Проверка пароля для нового пользователя
                if (EditingUser.UserID == 0 && string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Для нового пользователя необходимо установить пароль";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Password) && Password != ConfirmPassword)
                {
                    ErrorMessage = "Пароли не совпадают";
                    return;
                }

                if (EditingUser.UserID == 0)
                {
                    // Новый пользователь
                    var existing = _userRepository.GetByLogin(EditingUser.Login);
                    if (existing != null)
                    {
                        ErrorMessage = "Пользователь с таким логином уже существует";
                        return;
                    }

                    // Хэширование пароля (упрощенное - в реальном приложении используйте надежное хэширование)
                    EditingUser.PasswordHash = Password; // В реальном приложении: HashPassword(Password)
                    EditingUser.PasswordSalt = "salt"; // В реальном приложении: GenerateSalt()

                    EditingUser.UserID = _userRepository.Create(EditingUser);
                    Users.Add(new User
                    {
                        UserID = EditingUser.UserID,
                        Login = EditingUser.Login,
                        Email = EditingUser.Email,
                        FirstName = EditingUser.FirstName,
                        LastName = EditingUser.LastName,
                        PhoneNumber = EditingUser.PhoneNumber,
                        Role = EditingUser.Role,
                        WarehouseID = EditingUser.WarehouseID,
                        IsActive = EditingUser.IsActive,
                        CreatedDate = DateTime.Now
                    });
                }
                else
                {
                    // Редактирование
                    var existingUser = _userRepository.GetById(EditingUser.UserID);
                    if (existingUser != null)
                    {
                        // Сохраняем хэш пароля если он не менялся
                        EditingUser.PasswordHash = existingUser.PasswordHash;
                        EditingUser.PasswordSalt = existingUser.PasswordSalt;
                    }

                    _userRepository.Update(EditingUser);

                    // Обновляем в списке
                    var existing = Users.FirstOrDefault(u => u.UserID == EditingUser.UserID);
                    if (existing != null)
                    {
                        existing.Login = EditingUser.Login;
                        existing.Email = EditingUser.Email;
                        existing.FirstName = EditingUser.FirstName;
                        existing.LastName = EditingUser.LastName;
                        existing.PhoneNumber = EditingUser.PhoneNumber;
                        existing.Role = EditingUser.Role;
                        existing.WarehouseID = EditingUser.WarehouseID;
                        existing.IsActive = EditingUser.IsActive;
                    }
                }

                ErrorMessage = null;
                SelectedUser = null;
                EditingUser = new User();
                Password = "";
                ConfirmPassword = "";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения пользователя: {ex.Message}";
            }
        }

        private void DeleteUser(object parameter)
        {
            try
            {
                if (parameter is int userId)
                {
                    _userRepository.Delete(userId);
                    var user = Users.FirstOrDefault(u => u.UserID == userId);
                    if (user != null)
                    {
                        Users.Remove(user);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления пользователя: {ex.Message}";
            }
        }

        private void ResetPassword(object parameter)
        {
            try
            {
                if (parameter is int userId)
                {
                    // В реальном приложении: сброс пароля с отправкой email
                    var defaultPassword = "123456"; // Временный пароль
                    var user = _userRepository.GetById(userId);
                    if (user != null)
                    {
                        user.PasswordHash = defaultPassword; // В реальном приложении: HashPassword(defaultPassword)
                        _userRepository.Update(user);

                        ErrorMessage = $"Пароль сброшен. Временный пароль: {defaultPassword}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сброса пароля: {ex.Message}";
            }
        }

        private void FilterUsers()
        {
            try
            {
                var allUsers = _userRepository.GetAll();
                var filtered = allUsers.AsEnumerable();

                if (RoleFilter != "Все")
                {
                    var role = RoleFilter switch
                    {
                        "Администратор" => UserRole.Admin,
                        "Менеджер" => UserRole.Manager,
                        "Работник склада" => UserRole.WarehouseWorker,
                        "Наблюдатель" => UserRole.Viewer,
                        _ => UserRole.Viewer
                    };
                    filtered = filtered.Where(u => u.Role == role);
                }

                Users = new ObservableCollection<User>(filtered);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка фильтрации пользователей: {ex.Message}";
            }
        }
        #endregion
    }
}