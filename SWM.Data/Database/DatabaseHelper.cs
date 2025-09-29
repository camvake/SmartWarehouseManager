using System;
using System.Data.SQLite;
using System.IO;

namespace SWM.Data.Database
{
    public class DatabaseHelper
    {
        private string _databasePath;
        private string _connectionString;

        public DatabaseHelper()
        {
            // Используем твой конкретный путь
            _databasePath = @"C:\Users\Doll\source\repos\SmartWarehouseManager\SmartWarehouseManager\SWM.Data\Database\SmartWarehouseManager.db";
            _connectionString = $"Data Source={_databasePath};Version=3;";
        }

        public void InitializeDatabase()
        {
            // Создаем папку если ее нет
            string directory = Path.GetDirectoryName(_databasePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Создана папка: {directory}");
            }

            if (!File.Exists(_databasePath))
            {
                SQLiteConnection.CreateFile(_databasePath);
                Console.WriteLine($"База данных создана: {_databasePath}");

                // После создания БД создаем таблицы
                CreateTables();
            }
            else
            {
                Console.WriteLine("База данных уже существует");
            }
        }

        private void CreateTables()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // SQL скрипт создания таблиц
                    string createScript = @"
                        CREATE TABLE IF NOT EXISTS Warehouses (
                            WarehouseID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name VARCHAR(100) NOT NULL,
                            Description TEXT NULL,
                            Capacity INTEGER NOT NULL,
                            IsActivity BOOLEAN NOT NULL DEFAULT 1
                        );

                        CREATE TABLE IF NOT EXISTS Suppliers (
                            SupplierID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name VARCHAR(100) NOT NULL,
                            ContactDetails VARCHAR(255) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Statuses (
                            StatusID INTEGER PRIMARY KEY AUTOINCREMENT,
                            StatusName VARCHAR(50) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS PaymentMethods (
                            PaymentMethodID INTEGER PRIMARY KEY AUTOINCREMENT,
                            PaymentMethodName VARCHAR(50) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Users (
                            UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Login VARCHAR(50) NOT NULL UNIQUE,
                            Email VARCHAR(100) NOT NULL,
                            PasswordHash VARCHAR(255) NOT NULL,
                            Salt VARCHAR(50) NOT NULL,
                            Role VARCHAR(50) NOT NULL,
                            LastName VARCHAR(50) NOT NULL,
                            FirstName VARCHAR(50) NOT NULL,
                            PhoneNumber VARCHAR(20) NULL,
                            DateCreated DATETIME NOT NULL,
                            IsActive BOOLEAN NOT NULL DEFAULT 1
                        );

                        CREATE TABLE IF NOT EXISTS Products (
                            ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ArticleNumber VARCHAR(50) NOT NULL UNIQUE,
                            Name VARCHAR(100) NOT NULL,
                            Price DECIMAL(10,2) NOT NULL,
                            Description TEXT NOT NULL,
                            StockBalance INTEGER NOT NULL DEFAULT 0,
                            Image BLOB NULL,
                            Category VARCHAR(50) NOT NULL,
                            Characteristics TEXT NOT NULL,
                            WarehouseID INTEGER NOT NULL DEFAULT 1,
                            IsActive BOOLEAN NOT NULL DEFAULT 1,
                            CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );

                        CREATE TABLE IF NOT EXISTS Orders (
                            OrderID INTEGER PRIMARY KEY AUTOINCREMENT,
                            OrderNumber VARCHAR(50) NOT NULL UNIQUE,
                            OrderContent TEXT NOT NULL,
                            TotalAmount DECIMAL(10,2) NOT NULL,
                            DeliveryAddress VARCHAR(255) NOT NULL,
                            OrderDate DATETIME NOT NULL,
                            StatusID INTEGER NOT NULL DEFAULT 1,
                            PaymentMethodID INTEGER NOT NULL DEFAULT 1,
                            UserID INTEGER NOT NULL DEFAULT 1
                        );

                        CREATE TABLE IF NOT EXISTS OrderProducts (
                            OrderProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductID INTEGER NOT NULL,
                            OrderID INTEGER NOT NULL,
                            Quantity INTEGER NOT NULL,
                            UnitPrice DECIMAL(10,2) NOT NULL,
                            TotalPrice DECIMAL(10,2) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Receipts (
                            ReceiptID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ReceiptNumber VARCHAR(50) NOT NULL UNIQUE,
                            Quantity INTEGER NOT NULL,
                            ReceiptDate DATETIME NOT NULL,
                            SupplierID INTEGER NOT NULL,
                            WarehouseID INTEGER NOT NULL,
                            ProductID INTEGER NOT NULL
                        );
                    ";

                    using (var command = new SQLiteCommand(createScript, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Таблицы созданы успешно");
                    }

                    // Заполняем начальными данными
                    SeedInitialData(connection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании таблиц: {ex.Message}");
            }
        }

        private void SeedInitialData(SQLiteConnection connection)
        {
            try
            {
                // Проверяем, есть ли уже данные
                string checkUsers = "SELECT COUNT(*) FROM Users";
                using (var command = new SQLiteCommand(checkUsers, connection))
                {
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0) return; // Данные уже есть
                }

                // Заполняем начальными данными
                string seedScript = @"
                    INSERT INTO Warehouses (Name, Description, Capacity, IsActivity) VALUES 
                    ('Основной склад', 'Главный склад компании', 1000, 1);

                    INSERT INTO Suppliers (Name, ContactDetails) VALUES 
                    ('ООО ""ТехноПоставка""', 'Иван Иванов, +7-999-123-45-67');

                    INSERT INTO Statuses (StatusName) VALUES 
                    ('Новый'), ('В обработке'), ('Отправлен'), ('Доставлен'), ('Отменен');

                    INSERT INTO PaymentMethods (PaymentMethodName) VALUES 
                    ('Наличные'), ('Карта'), ('Перевод');

                    INSERT INTO Users (Login, Email, PasswordHash, Salt, Role, LastName, FirstName, PhoneNumber, DateCreated) VALUES 
                    ('admin', 'admin@warehouse.com', 'admin123', 'salt1', 'Admin', 'Админ', 'Система', '+7-000-000-00-00', datetime('now'));

                    INSERT INTO Products (ArticleNumber, Name, Price, Description, StockBalance, Category, Characteristics) VALUES 
                    ('NB-001', 'Ноутбук Dell XPS 13', 89999.99, '13-дюймовый бизнес-ноутбук', 15, 'Электроника', 'Intel i7, 16GB RAM, 512GB SSD'),
                    ('MON-002', 'Монитор Samsung 24""', 15999.00, '24-дюймовый офисный монитор', 30, 'Электроника', '1920x1080, IPS матрица');
                ";

                using (var command = new SQLiteCommand(seedScript, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Тестовые данные добавлены");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при заполнении данных: {ex.Message}");
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public void TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT SQLITE_VERSION();", connection))
                    {
                        var version = command.ExecuteScalar();
                        Console.WriteLine($"SQLite version: {version}");
                    }
                    Console.WriteLine("Подключение к БД успешно!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
        }
    }
}