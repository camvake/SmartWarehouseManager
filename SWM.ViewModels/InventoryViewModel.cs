using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly WarehouseRepository _warehouseRepository;

        public InventoryViewModel(string connectionString)
        {
            _inventoryRepository = new InventoryRepository(connectionString);
            _productRepository = new ProductRepository(connectionString);
            _warehouseRepository = new WarehouseRepository(connectionString);

            LoadInventoriesCommand = new RelayCommand(LoadInventories);
            CreateInventoryCommand = new RelayCommand(CreateInventory);
            AddInventoryItemCommand = new RelayCommand(AddInventoryItem);
            UpdateInventoryItemCommand = new RelayCommand((param) => UpdateInventoryItem(param));
            CompleteInventoryCommand = new RelayCommand(CompleteInventory);

            LoadInventories();
            LoadProducts();
            LoadWarehouses();
        }

        #region Commands
        public ICommand LoadInventoriesCommand { get; }
        public ICommand CreateInventoryCommand { get; }
        public ICommand AddInventoryItemCommand { get; }
        public ICommand UpdateInventoryItemCommand { get; }
        public ICommand CompleteInventoryCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<Inventory> _inventories;
        public ObservableCollection<Inventory> Inventories
        {
            get => _inventories;
            set => SetProperty(ref _inventories, value);
        }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<Warehouse> _warehouses;
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        private ObservableCollection<InventoryItemDisplayDto> _inventoryItems;
        public ObservableCollection<InventoryItemDisplayDto> InventoryItems
        {
            get => _inventoryItems;
            set => SetProperty(ref _inventoryItems, value);
        }

        // Добавляем коллекцию для хранения полных InventoryItem
        private ObservableCollection<InventoryItem> _fullInventoryItems;
        public ObservableCollection<InventoryItem> FullInventoryItems
        {
            get => _fullInventoryItems;
            set => SetProperty(ref _fullInventoryItems, value);
        }

        private Inventory _selectedInventory;
        public Inventory SelectedInventory
        {
            get => _selectedInventory;
            set
            {
                SetProperty(ref _selectedInventory, value);
                if (value != null)
                {
                    LoadInventoryItems(value.InventoryID);
                }
            }
        }

        private Inventory _newInventory = new Inventory();
        public Inventory NewInventory
        {
            get => _newInventory;
            set => SetProperty(ref _newInventory, value);
        }

        private InventoryItem _newInventoryItem = new InventoryItem();
        public InventoryItem NewInventoryItem
        {
            get => _newInventoryItem;
            set => SetProperty(ref _newInventoryItem, value);
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                if (value != null)
                {
                    NewInventoryItem.ProductID = value.ProductID;
                    NewInventoryItem.ExpectedQuantity = value.StockBalance;
                    NewInventoryItem.ActualQuantity = value.StockBalance;
                }
            }
        }

        private string _inventoryStatusFilter = "Все";
        public string InventoryStatusFilter
        {
            get => _inventoryStatusFilter;
            set
            {
                SetProperty(ref _inventoryStatusFilter, value);
                FilterInventories();
            }
        }
        #endregion

        #region Methods
        private void LoadInventories()
        {
            try
            {
                IsLoading = true;
                var inventories = _inventoryRepository.GetAll();
                Inventories = new ObservableCollection<Inventory>(inventories);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки инвентаризаций: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAll();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
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

        private void LoadInventoryItems(int inventoryId)
        {
            try
            {
                // Загружаем DTO для отображения
                var itemsDto = _inventoryRepository.GetInventoryItemsForDisplay(inventoryId);
                InventoryItems = new ObservableCollection<InventoryItemDisplayDto>(itemsDto);

                // Загружаем полные InventoryItem для работы с ProductID
                var fullItems = _inventoryRepository.GetById(inventoryId)?.InventoryItems;
                FullInventoryItems = new ObservableCollection<InventoryItem>(fullItems ?? new List<InventoryItem>());
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки позиций инвентаризации: {ex.Message}";
            }
        }

        private void CreateInventory()
        {
            try
            {
                if (NewInventory.WarehouseID == 0)
                {
                    ErrorMessage = "Выберите склад";
                    return;
                }

                // Генерация номера инвентаризации
                NewInventory.InventoryNumber = $"INV-{DateTime.Now:yyyyMMdd-HHmmss}";
                NewInventory.Status = "В процессе";
                NewInventory.CreatedBy = 1; // Текущий пользователь

                var inventoryId = _inventoryRepository.Create(NewInventory);

                // Обновление списка инвентаризаций
                var newInventory = _inventoryRepository.GetById(inventoryId);
                Inventories.Insert(0, newInventory);
                SelectedInventory = newInventory;

                // Сброс формы
                NewInventory = new Inventory();

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания инвентаризации: {ex.Message}";
            }
        }

        private void AddInventoryItem()
        {
            try
            {
                if (SelectedInventory == null)
                {
                    ErrorMessage = "Сначала создайте или выберите инвентаризацию";
                    return;
                }

                if (SelectedProduct == null)
                {
                    ErrorMessage = "Выберите товар";
                    return;
                }

                var inventoryItem = new InventoryItem
                {
                    InventoryID = SelectedInventory.InventoryID,
                    ProductID = SelectedProduct.ProductID,
                    ExpectedQuantity = NewInventoryItem.ExpectedQuantity,
                    ActualQuantity = NewInventoryItem.ActualQuantity,
                    Notes = NewInventoryItem.Notes
                };

                _inventoryRepository.CreateInventoryItem(inventoryItem);

                // Обновление списка позиций
                LoadInventoryItems(SelectedInventory.InventoryID);

                // Сброс формы добавления товара
                NewInventoryItem = new InventoryItem();
                SelectedProduct = null;

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка добавления товара: {ex.Message}";
            }
        }

        private void UpdateInventoryItem(object parameter)
        {
            try
            {
                if (parameter is InventoryItemDisplayDto itemDto)
                {
                    // Находим соответствующий полный InventoryItem по InventoryItemID
                    var fullItem = FullInventoryItems.FirstOrDefault(i => i.InventoryItemID == itemDto.InventoryItemID);
                    if (fullItem != null)
                    {
                        var item = new InventoryItem
                        {
                            InventoryItemID = itemDto.InventoryItemID,
                            InventoryID = fullItem.InventoryID,
                            ProductID = fullItem.ProductID,
                            ExpectedQuantity = fullItem.ExpectedQuantity,
                            ActualQuantity = itemDto.ActualQuantity,
                            Notes = itemDto.Notes
                        };

                        _inventoryRepository.UpdateInventoryItem(item);

                        // Обновление списка
                        LoadInventoryItems(SelectedInventory.InventoryID);

                        ErrorMessage = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления позиции: {ex.Message}";
            }
        }

        private void CompleteInventory()
        {
            try
            {
                if (SelectedInventory == null) return;

                // Расчет общей суммы расхождений
                var totalDiscrepancy = InventoryItems.Sum(i => i.CostDifference);

                // Обновление остатков товаров на основе расхождений
                foreach (var fullItem in FullInventoryItems)
                {
                    var displayItem = InventoryItems.FirstOrDefault(i => i.InventoryItemID == fullItem.InventoryItemID);
                    if (displayItem != null && displayItem.QuantityDifference != 0)
                    {
                        var product = _productRepository.GetById(fullItem.ProductID);
                        if (product != null)
                        {
                            product.StockBalance = displayItem.ActualQuantity;
                            _productRepository.Update(product);
                        }
                    }
                }

                _inventoryRepository.CompleteInventory(SelectedInventory.InventoryID, totalDiscrepancy);

                // Обновление в списке
                var updatedInventory = _inventoryRepository.GetById(SelectedInventory.InventoryID);
                var index = Inventories.IndexOf(SelectedInventory);
                Inventories[index] = updatedInventory;
                SelectedInventory = updatedInventory;

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка завершения инвентаризации: {ex.Message}";
            }
        }

        private void FilterInventories()
        {
            try
            {
                var allInventories = _inventoryRepository.GetAll();
                var filtered = allInventories.AsEnumerable();

                if (InventoryStatusFilter != "Все")
                {
                    filtered = filtered.Where(i => i.Status == InventoryStatusFilter);
                }

                Inventories = new ObservableCollection<Inventory>(filtered);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка фильтрации инвентаризаций: {ex.Message}";
            }
        }
        #endregion
    }
}