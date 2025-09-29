using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SWM.ViewModels
{
    public class SupplyViewModel : INotifyPropertyChanged
    {
        private readonly SupplyRepository _supplyRepository;
        private readonly ProductRepository _productRepository;
        private readonly SupplierRepository _supplierRepository;

        private ObservableCollection<Supply> _supplies;
        private ObservableCollection<Product> _availableProducts;
        private ObservableCollection<Supplier> _suppliers;
        private ObservableCollection<Inventory> _inventories;

        public ObservableCollection<Supply> Supplies
        {
            get => _supplies;
            set
            {
                _supplies = value;
                OnPropertyChanged(nameof(Supplies));
            }
        }

        public ObservableCollection<Product> AvailableProducts
        {
            get => _availableProducts;
            set
            {
                _availableProducts = value;
                OnPropertyChanged(nameof(AvailableProducts));
            }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set
            {
                _suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        public ObservableCollection<Inventory> Inventories
        {
            get => _inventories;
            set
            {
                _inventories = value;
                OnPropertyChanged(nameof(Inventories));
            }
        }

        public SupplyViewModel(string connectionString)
        {
            _supplyRepository = new SupplyRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);
            _supplierRepository = new SupplierRepository(connectionString);

            LoadData();
        }

        public void LoadData()
        {
            Supplies = new ObservableCollection<Supply>(_supplyRepository.GetAllSupplies());
            AvailableProducts = new ObservableCollection<Product>(_productRepository.GetAllProducts());
            Suppliers = new ObservableCollection<Supplier>(_supplierRepository.GetAllSuppliers());
            Inventories = new ObservableCollection<Inventory>(_supplyRepository.GetInventoryHistory());
        }

        public void CreateSupply(Supply supply)
        {
            _supplyRepository.CreateSupply(supply);
            LoadData(); // Обновляем данные
        }

        public void CreateInventory(Inventory inventory)
        {
            _supplyRepository.CreateInventory(inventory);
            LoadData();
        }

        public decimal CalculateInventoryValue(int warehouseId)
        {
            var products = AvailableProducts.Where(p => p.WarehouseID == warehouseId);
            return products.Sum(p => p.Price * p.StockBalance);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}