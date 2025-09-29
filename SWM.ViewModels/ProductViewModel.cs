using SWM.Core.Models;
using SWM.Data.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWM.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly ProductRepository _productRepository;
        private ObservableCollection<Product> _products;
        private Product _selectedProduct;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public ProductViewModel(string connectionString)
        {
            _productRepository = new ProductRepository(connectionString);
            LoadProducts();
        }

        public void LoadProducts()
        {
            var products = _productRepository.GetAllProducts();
            Products = new ObservableCollection<Product>(products);
        }

        public void AddProduct(Product product)
        {
            _productRepository.AddProduct(product);
            LoadProducts(); // Обновляем список
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}