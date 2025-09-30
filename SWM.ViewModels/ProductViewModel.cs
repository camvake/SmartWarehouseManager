using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        public ProductViewModel(string connectionString)
        {
            LoadProductsCommand = new RelayCommand(LoadProducts);
            SaveProductCommand = new RelayCommand(SaveProduct);
            DeleteProductCommand = new RelayCommand(DeleteProduct);
            SearchProductsCommand = new RelayCommand(SearchProducts);

            LoadProducts();
            LoadSuppliers();
        }

        #region Commands
        public ICommand LoadProductsCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SearchProductsCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<SWM.Core.Models.Product> _products;
        public ObservableCollection<SWM.Core.Models.Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<SWM.Core.Models.Supplier> _suppliers;
        public ObservableCollection<SWM.Core.Models.Supplier> Suppliers
        {
            get => _suppliers;
            set => SetProperty(ref _suppliers, value);
        }

        private SWM.Core.Models.Product _selectedProduct;
        public SWM.Core.Models.Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                if (value != null)
                {
                    EditingProduct = new SWM.Core.Models.Product
                    {
                        ProductID = value.ProductID,
                        ArticleNumber = value.ArticleNumber,
                        ProductName = value.ProductName,
                        Description = value.Description,
                        CategoryID = value.CategoryID,
                        SupplierID = value.SupplierID,
                        PurchasePrice = value.PurchasePrice,
                        SalePrice = value.SalePrice,
                        StockBalance = value.StockBalance,
                        MinStockLevel = value.MinStockLevel,
                        MaxStockLevel = value.MaxStockLevel,
                        UnitOfMeasure = value.UnitOfMeasure,
                        Weight = value.Weight,
                        Dimensions = value.Dimensions,
                        Barcode = value.Barcode,
                        ImageURL = value.ImageURL,
                        Characteristics = value.Characteristics
                    };
                }
                else
                {
                    EditingProduct = new SWM.Core.Models.Product();
                }
            }
        }

        private SWM.Core.Models.Product _editingProduct = new SWM.Core.Models.Product();
        public SWM.Core.Models.Product EditingProduct
        {
            get => _editingProduct;
            set => SetProperty(ref _editingProduct, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private bool _showLowStockOnly;
        public bool ShowLowStockOnly
        {
            get => _showLowStockOnly;
            set
            {
                SetProperty(ref _showLowStockOnly, value);
                FilterProducts();
            }
        }
        #endregion

        #region Methods
        private void LoadProducts()
        {
            try
            {
                IsLoading = true;

                var products = new List<SWM.Core.Models.Product>
                {
                    CreateTestProduct(1, "NB-001", "Ноутбук Dell XPS 13", "Электроника", "ООО ТехноПоставка"),
                    CreateTestProduct(2, "MN-002", "Монитор Samsung 27\"", "Электроника", "ИП Иванов"),
                    CreateTestProduct(3, "KB-003", "Клавиатура механическая", "Аксессуары", "ЗАО Электроник")
                };

                Products = new ObservableCollection<SWM.Core.Models.Product>(products);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private SWM.Core.Models.Product CreateTestProduct(int id, string article, string name, string categoryName, string supplierName)
        {
            return new SWM.Core.Models.Product
            {
                ProductID = id,
                ArticleNumber = article,
                ProductName = name,
                Description = $"Описание для {name}",
                PurchasePrice = id * 10000m,
                SalePrice = id * 12000m,
                StockBalance = id * 2,
                MinStockLevel = 5,
                MaxStockLevel = 50,
                UnitOfMeasure = "шт.",
                Category = new SWM.Core.Models.Category { CategoryName = categoryName },
                Supplier = new SWM.Core.Models.Supplier { SupplierName = supplierName }
            };
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = new List<SWM.Core.Models.Supplier>
                {
                    new SWM.Core.Models.Supplier { SupplierID = 1, SupplierName = "ООО 'ТехноПоставка'" },
                    new SWM.Core.Models.Supplier { SupplierID = 2, SupplierName = "ИП Иванов" },
                    new SWM.Core.Models.Supplier { SupplierID = 3, SupplierName = "ЗАО 'Электроник'" }
                };

                Suppliers = new ObservableCollection<SWM.Core.Models.Supplier>(suppliers);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки поставщиков: {ex.Message}";
            }
        }

        private void SaveProduct()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditingProduct.ArticleNumber) ||
                    string.IsNullOrWhiteSpace(EditingProduct.ProductName))
                {
                    ErrorMessage = "Заполните обязательные поля: Артикул и Наименование";
                    return;
                }

                if (EditingProduct.ProductID == 0)
                {
                    // Новый товар
                    var existing = Products.FirstOrDefault(p => p.ArticleNumber == EditingProduct.ArticleNumber);
                    if (existing != null)
                    {
                        ErrorMessage = "Товар с таким артикулом уже существует";
                        return;
                    }

                    EditingProduct.ProductID = Products.Count + 1;
                    Products.Add(CreateProductFromEditing());
                }
                else
                {
                    // Редактирование существующего товара
                    var existing = Products.FirstOrDefault(p => p.ProductID == EditingProduct.ProductID);
                    if (existing != null)
                    {
                        UpdateProductFromEditing(existing);
                    }
                }

                ErrorMessage = null;
                SelectedProduct = null;
                EditingProduct = new SWM.Core.Models.Product();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения товара: {ex.Message}";
            }
        }

        private SWM.Core.Models.Product CreateProductFromEditing()
        {
            return new SWM.Core.Models.Product
            {
                ProductID = EditingProduct.ProductID,
                ArticleNumber = EditingProduct.ArticleNumber,
                ProductName = EditingProduct.ProductName,
                Description = EditingProduct.Description,
                CategoryID = EditingProduct.CategoryID,
                SupplierID = EditingProduct.SupplierID,
                PurchasePrice = EditingProduct.PurchasePrice,
                SalePrice = EditingProduct.SalePrice,
                StockBalance = EditingProduct.StockBalance,
                MinStockLevel = EditingProduct.MinStockLevel,
                MaxStockLevel = EditingProduct.MaxStockLevel,
                UnitOfMeasure = EditingProduct.UnitOfMeasure,
                Category = EditingProduct.Category,
                Supplier = EditingProduct.Supplier
            };
        }

        private void UpdateProductFromEditing(SWM.Core.Models.Product existing)
        {
            existing.ArticleNumber = EditingProduct.ArticleNumber;
            existing.ProductName = EditingProduct.ProductName;
            existing.Description = EditingProduct.Description;
            existing.CategoryID = EditingProduct.CategoryID;
            existing.SupplierID = EditingProduct.SupplierID;
            existing.PurchasePrice = EditingProduct.PurchasePrice;
            existing.SalePrice = EditingProduct.SalePrice;
            existing.StockBalance = EditingProduct.StockBalance;
            existing.MinStockLevel = EditingProduct.MinStockLevel;
            existing.MaxStockLevel = EditingProduct.MaxStockLevel;
            existing.UnitOfMeasure = EditingProduct.UnitOfMeasure;

            if (EditingProduct.Category != null)
            {
                existing.Category = EditingProduct.Category;
            }

            if (EditingProduct.Supplier != null)
            {
                existing.Supplier = EditingProduct.Supplier;
            }
        }

        private void DeleteProduct(object parameter)
        {
            try
            {
                if (parameter is int productId)
                {
                    var product = Products.FirstOrDefault(p => p.ProductID == productId);
                    if (product != null)
                    {
                        Products.Remove(product);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления товара: {ex.Message}";
            }
        }

        private void SearchProducts()
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            try
            {
                var allProducts = GetAllProducts();
                var filtered = allProducts.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(p =>
                        p.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        p.ArticleNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description != null && p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }

                if (ShowLowStockOnly)
                {
                    filtered = filtered.Where(p => p.IsLowStock);
                }

                Products = new ObservableCollection<SWM.Core.Models.Product>(filtered);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка фильтрации товаров: {ex.Message}";
            }
        }

        private List<SWM.Core.Models.Product> GetAllProducts()
        {
            return new List<SWM.Core.Models.Product>
            {
                CreateTestProduct(1, "NB-001", "Ноутбук Dell XPS 13", "Электроника", "ООО ТехноПоставка"),
                CreateTestProduct(2, "MN-002", "Монитор Samsung 27\"", "Электроника", "ИП Иванов"),
                CreateTestProduct(3, "KB-003", "Клавиатура механическая", "Аксессуары", "ЗАО Электроник")
            };
        }
        #endregion
    }
}