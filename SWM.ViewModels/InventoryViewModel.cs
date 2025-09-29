using SWM.Core.Models;
using SWM.Data.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SWM.ViewModels
{
    public class InventoryViewModels : INotifyPropertyChanged
    {
        private readonly ProductRepository _productRepository;
        private ObservableCollection<Inventory> _inventories;

        public ObservableCollection<Inventory> Inventories
        {
            get => _inventories;
            set
            {
                _inventories = value;
                OnPropertyChanged(nameof(Inventories));
            }
        }

        public InventoryViewModels(string connectionString)
        {
            _productRepository = new ProductRepository(connectionString);
            Inventories = new ObservableCollection<Inventory>();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // Временные данные для тестирования
            var inventory = new Inventory
            {
                InventoryID = 1,
                InventoryNumber = "INV-TEST-001",
                InventoryDate = System.DateTime.Now,
                Status = InventoryStatus.Draft,
                WarehouseID = 1
            };

            Inventories.Add(inventory);
        }

        public System.Collections.Generic.List<Product> GetProducts()
        {
            return _productRepository.GetAllProducts().ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}