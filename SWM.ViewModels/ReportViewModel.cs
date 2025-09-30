using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Forms; // ДОБАВИТЬ ЭТОТ USING

namespace SWM.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly ReportRepository _reportRepository;

        public ReportViewModel(string connectionString)
        {
            _reportRepository = new ReportRepository(connectionString);

            GenerateSalesReportCommand = new RelayCommand(GenerateSalesReport);
            GenerateStockReportCommand = new RelayCommand(GenerateStockReport);
            ExportReportCommand = new RelayCommand((param) => ExportReport(param));

            // Загружаем отчет по складу по умолчанию
            GenerateStockReport();
        }

        #region Commands
        public ICommand GenerateSalesReportCommand { get; }
        public ICommand GenerateStockReportCommand { get; }
        public ICommand ExportReportCommand { get; }
        #endregion

        #region Properties
        private SalesReport _salesReport;
        public SalesReport SalesReport
        {
            get => _salesReport;
            set => SetProperty(ref _salesReport, value);
        }

        private StockReport _stockReport;
        public StockReport StockReport
        {
            get => _stockReport;
            set => SetProperty(ref _stockReport, value);
        }

        private DateTime _salesReportFromDate = DateTime.Now.AddMonths(-1);
        public DateTime SalesReportFromDate
        {
            get => _salesReportFromDate;
            set => SetProperty(ref _salesReportFromDate, value);
        }

        private DateTime _salesReportToDate = DateTime.Now;
        public DateTime SalesReportToDate
        {
            get => _salesReportToDate;
            set => SetProperty(ref _salesReportToDate, value);
        }

        private string _selectedReportType = "Stock";
        public string SelectedReportType
        {
            get => _selectedReportType;
            set => SetProperty(ref _selectedReportType, value);
        }

        private ObservableCollection<SalesByCategory> _salesByCategory;
        public ObservableCollection<SalesByCategory> SalesByCategory
        {
            get => _salesByCategory;
            set => SetProperty(ref _salesByCategory, value);
        }

        private ObservableCollection<SalesByProduct> _topSellingProducts;
        public ObservableCollection<SalesByProduct> TopSellingProducts
        {
            get => _topSellingProducts;
            set => SetProperty(ref _topSellingProducts, value);
        }

        private ObservableCollection<StockStatusItem> _stockStatus;
        public ObservableCollection<StockStatusItem> StockStatus
        {
            get => _stockStatus;
            set => SetProperty(ref _stockStatus, value);
        }
        #endregion

        #region Methods
        private void GenerateSalesReport()
        {
            try
            {
                IsLoading = true;
                SalesReport = _reportRepository.GetSalesReport(SalesReportFromDate, SalesReportToDate);

                // Обновляем коллекции для привязки
                SalesByCategory = new ObservableCollection<SalesByCategory>(SalesReport.SalesByCategories);
                TopSellingProducts = new ObservableCollection<SalesByProduct>(SalesReport.TopSellingProducts);

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации отчета по продажам: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateStockReport()
        {
            try
            {
                IsLoading = true;
                StockReport = _reportRepository.GetStockReport();

                // Обновляем коллекцию для привязки
                StockStatus = new ObservableCollection<StockStatusItem>(StockReport.StockStatus);

                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации отчета по складу: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExportReport(object parameter)
        {
            try
            {
                if (parameter is string reportType)
                {
                    // В реальном приложении: реализовать экспорт в Excel/PDF
                    switch (reportType)
                    {
                        case "Sales":
                            if (SalesReport != null)
                            {
                                MessageBox.Show($"Отчет по продажам экспортирован\nПериод: {SalesReportFromDate:dd.MM.yyyy} - {SalesReportToDate:dd.MM.yyyy}\nВыручка: {SalesReport.TotalRevenue:C2}",
                                    "Экспорт отчета", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "Stock":
                            if (StockReport != null)
                            {
                                MessageBox.Show($"Отчет по складу экспортирован\nДата: {StockReport.ReportDate:dd.MM.yyyy}\nОбщая стоимость: {StockReport.TotalStockValue:C2}",
                                    "Экспорт отчета", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка экспорта отчета: {ex.Message}";
            }
        }
        #endregion
    }
}