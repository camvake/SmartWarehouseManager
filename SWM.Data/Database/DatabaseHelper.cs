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
            // АБСОЛЮТНЫЙ путь к БД
            _databasePath = @"C:\Users\Doll\source\repos\SmartWarehouseManager\SmartWarehouseManager\SWM.Data\Database\SmartWarehouseManager.db";
            _connectionString = $"Data Source={_databasePath};Version=3;Foreign Keys=True;";
        }

        public void InitializeDatabase()
        {
            try
            {
                // Создаем папку если ее нет
                string directory = Path.GetDirectoryName(_databasePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(_databasePath))
                {
                    SQLiteConnection.CreateFile(_databasePath);
                    Console.WriteLine("База данных создана");
                }
                else
                {
                    Console.WriteLine("База данных уже существует");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                throw;
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
                    Console.WriteLine("Подключение к БД успешно!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                throw;
            }
        }
    }
}