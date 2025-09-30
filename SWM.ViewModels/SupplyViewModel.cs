using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SWM.ViewModels
{
    public class SupplyViewModel : BaseViewModel
    {
        public SupplyViewModel(string connectionString)
        {
            LoadSuppliesCommand = new RelayCommand(LoadSupplies);
            CreateSupplyCommand = new RelayCommand(CreateSupply);
            ReceiveSupplyCommand = new RelayCommand(ReceiveSupply);
            DeleteSupplyCommand = new RelayCommand(DeleteSupply);

            LoadSupplies();
            LoadSuppliers();
        }

        #region Commands
        public ICommand LoadSuppliesCommand { get; }
        public ICommand CreateSupplyCommand { get; }
        public ICommand ReceiveSupplyCommand { get; }
        public ICommand DeleteSupplyCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<Supply> _supplies;
        public ObservableCollection<Supply> Supplies
        {
            get => _supplies;
            set => SetProperty(ref _supplies, value);
        }

        private ObservableCollection<Supplier> _suppliers;
        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set => SetProperty(ref _suppliers, value);
        }

        private Supply _selectedSupply;
        public Supply SelectedSupply
        {
            get => _selectedSupply;
            set => SetProperty(ref _selectedSupply, value);
        }

        private Supply _editingSupply = new Supply();
        public Supply EditingSupply
        {
            get => _editingSupply;
            set => SetProperty(ref _editingSupply, value);
        }
        #endregion

        #region Methods
        private void LoadSupplies()
        {
            try
            {
                IsLoading = true;

                // Тестовые данные
                var supplies = new List<Supply>
                {
                    new Supply
                    {
                        SupplyID = 1,
                        SupplyNumber = "SUP-001",
                        Supplier = new Supplier { SupplierName = "ООО 'ТехноПоставка'" },
                        OrderDate = DateTime.Now.AddDays(-5),
                        ExpectedDate = DateTime.Now.AddDays(2),
                        TotalAmount = 45200m,
                        Status = "В пути",
                        ItemsCount = 8
                    },
                    new Supply
                    {
                        SupplyID = 2,
                        SupplyNumber = "SUP-002",
                        Supplier = new Supplier { SupplierName = "ИП Иванов" },
                        OrderDate = DateTime.Now.AddDays(-3),
                        ExpectedDate = DateTime.Now.AddDays(4),
                        TotalAmount = 18500m,
                        Status = "Ожидается",
                        ItemsCount = 5
                    },
                    new Supply
                    {
                        SupplyID = 3,
                        SupplyNumber = "SUP-003",
                        Supplier = new Supplier { SupplierName = "ООО 'Комплектующие+'" },
                        OrderDate = DateTime.Now.AddDays(-7),
                        ExpectedDate = DateTime.Now.AddDays(-1),
                        TotalAmount = 32100m,
                        Status = "Получено",
                        ItemsCount = 12
                    }
                };

                Supplies = new ObservableCollection<Supply>(supplies);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки поставок: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = new List<Supplier>
                {
                    new Supplier { SupplierID = 1, SupplierName = "ООО 'ТехноПоставка'" },
                    new Supplier { SupplierID = 2, SupplierName = "ИП Иванов" },
                    new Supplier { SupplierID = 3, SupplierName = "ЗАО 'Электроник'" },
                    new Supplier { SupplierID = 4, SupplierName = "ООО 'Комплектующие+'" }
                };

                Suppliers = new ObservableCollection<Supplier>(suppliers);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки поставщиков: {ex.Message}";
            }
        }

        private void CreateSupply()
        {
            try
            {
                if (EditingSupply.SupplierID == 0)
                {
                    ErrorMessage = "Выберите поставщика";
                    return;
                }

                if (EditingSupply.ExpectedDate < DateTime.Now)
                {
                    ErrorMessage = "Дата поставки не может быть в прошлом";
                    return;
                }

                // Логика создания поставки
                EditingSupply.SupplyID = Supplies.Count + 1;
                EditingSupply.SupplyNumber = $"SUP-{EditingSupply.SupplyID:000}";
                EditingSupply.Status = "Ожидается";
                EditingSupply.OrderDate = DateTime.Now;

                Supplies.Add(new Supply
                {
                    SupplyID = EditingSupply.SupplyID,
                    SupplyNumber = EditingSupply.SupplyNumber,
                    SupplierID = EditingSupply.SupplierID,
                    Supplier = Suppliers.FirstOrDefault(s => s.SupplierID == EditingSupply.SupplierID),
                    OrderDate = EditingSupply.OrderDate,
                    ExpectedDate = EditingSupply.ExpectedDate,
                    TotalAmount = EditingSupply.TotalAmount,
                    Status = EditingSupply.Status,
                    ItemsCount = EditingSupply.ItemsCount
                });

                ErrorMessage = null;
                EditingSupply = new Supply();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания поставки: {ex.Message}";
            }
        }

        private void ReceiveSupply()
        {
            try
            {
                if (SelectedSupply == null)
                {
                    ErrorMessage = "Выберите поставку для приема";
                    return;
                }

                SelectedSupply.Status = "Получено";
                SelectedSupply.ReceivedDate = DateTime.Now;

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка приема поставки: {ex.Message}";
            }
        }

        private void DeleteSupply(object parameter)
        {
            try
            {
                if (parameter is int supplyId)
                {
                    var supply = Supplies.FirstOrDefault(s => s.SupplyID == supplyId);
                    if (supply != null)
                    {
                        Supplies.Remove(supply);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления поставки: {ex.Message}";
            }
        }
        #endregion
    }

    // Модель поставки
    public class Supply
    {
        public int SupplyID { get; set; }
        public string SupplyNumber { get; set; } = string.Empty;
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Ожидается";
        public int ItemsCount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}