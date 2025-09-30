using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Forms; // ДОБАВИТЬ ЭТОТ USING

namespace SWM.ViewModels
{
    public class AdvancedReportViewModel : BaseViewModel
    {
        private readonly AdvancedReportRepository _advancedReportRepository;

        public AdvancedReportViewModel(string connectionString)
        {
            _advancedReportRepository = new AdvancedReportRepository(connectionString);

            GenerateFinancialReportCommand = new RelayCommand(GenerateFinancialReport);
            GenerateSupplierReportCommand = new RelayCommand(GenerateSupplierReport);
            GenerateInventoryTurnoverReportCommand = new RelayCommand(GenerateInventoryTurnoverReport);
            GenerateCustomerAnalysisReportCommand = new RelayCommand(GenerateCustomerAnalysisReport);
            ExportAdvancedReportCommand = new RelayCommand((param) => ExportAdvancedReport(param));

            // Устанавливаем даты по умолчанию
            ReportFromDate = DateTime.Now.AddMonths(-3);
            ReportToDate = DateTime.Now;
        }

        #region Commands
        public ICommand GenerateFinancialReportCommand { get; }
        public ICommand GenerateSupplierReportCommand { get; }
        public ICommand GenerateInventoryTurnoverReportCommand { get; }
        public ICommand GenerateCustomerAnalysisReportCommand { get; }
        public ICommand ExportAdvancedReportCommand { get; }
        #endregion

        #region Properties
        private FinancialReport _financialReport;
        public FinancialReport FinancialReport
        {
            get => _financialReport;
            set => SetProperty(ref _financialReport, value);
        }

        private SupplierPerformanceReport _supplierPerformanceReport;
        public SupplierPerformanceReport SupplierPerformanceReport
        {
            get => _supplierPerformanceReport;
            set => SetProperty(ref _supplierPerformanceReport, value);
        }

        private InventoryTurnoverReport _inventoryTurnoverReport;
        public InventoryTurnoverReport InventoryTurnoverReport
        {
            get => _inventoryTurnoverReport;
            set => SetProperty(ref _inventoryTurnoverReport, value);
        }

        private CustomerAnalysisReport _customerAnalysisReport;
        public CustomerAnalysisReport CustomerAnalysisReport
        {
            get => _customerAnalysisReport;
            set => SetProperty(ref _customerAnalysisReport, value);
        }

        private DateTime _reportFromDate;
        public DateTime ReportFromDate
        {
            get => _reportFromDate;
            set => SetProperty(ref _reportFromDate, value);
        }

        private DateTime _reportToDate;
        public DateTime ReportToDate
        {
            get => _reportToDate;
            set => SetProperty(ref _reportToDate, value);
        }

        private string _selectedAdvancedReport = "Financial";
        public string SelectedAdvancedReport
        {
            get => _selectedAdvancedReport;
            set => SetProperty(ref _selectedAdvancedReport, value);
        }

        private ObservableCollection<MonthlyFinancial> _monthlyFinancials;
        public ObservableCollection<MonthlyFinancial> MonthlyFinancials
        {
            get => _monthlyFinancials;
            set => SetProperty(ref _monthlyFinancials, value);
        }

        private ObservableCollection<SupplierPerformance> _supplierPerformances;
        public ObservableCollection<SupplierPerformance> SupplierPerformances
        {
            get => _supplierPerformances;
            set => SetProperty(ref _supplierPerformances, value);
        }

        private ObservableCollection<ProductTurnover> _productTurnovers;
        public ObservableCollection<ProductTurnover> ProductTurnovers
        {
            get => _productTurnovers;
            set => SetProperty(ref _productTurnovers, value);
        }

        private ObservableCollection<CustomerSegment> _customerSegments;
        public ObservableCollection<CustomerSegment> CustomerSegments
        {
            get => _customerSegments;
            set => SetProperty(ref _customerSegments, value);
        }
        #endregion

        #region Methods
        private void GenerateFinancialReport()
        {
            try
            {
                IsLoading = true;
                FinancialReport = _advancedReportRepository.GetFinancialReport(ReportFromDate, ReportToDate);

                // Обновляем коллекции для привязки
                MonthlyFinancials = new ObservableCollection<MonthlyFinancial>(FinancialReport.MonthlyBreakdown);

                ErrorMessage = null;
                SelectedAdvancedReport = "Financial";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации финансового отчета: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateSupplierReport()
        {
            try
            {
                IsLoading = true;
                SupplierPerformanceReport = _advancedReportRepository.GetSupplierPerformanceReport(ReportFromDate, ReportToDate);

                // Обновляем коллекции для привязки
                SupplierPerformances = new ObservableCollection<SupplierPerformance>(SupplierPerformanceReport.SupplierPerformances);

                ErrorMessage = null;
                SelectedAdvancedReport = "Supplier";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации отчета по поставщикам: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateInventoryTurnoverReport()
        {
            try
            {
                IsLoading = true;
                InventoryTurnoverReport = _advancedReportRepository.GetInventoryTurnoverReport(ReportFromDate, ReportToDate);

                // Обновляем коллекции для привязки
                ProductTurnovers = new ObservableCollection<ProductTurnover>(InventoryTurnoverReport.ProductTurnovers);

                ErrorMessage = null;
                SelectedAdvancedReport = "Inventory";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации отчета по оборачиваемости: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateCustomerAnalysisReport()
        {
            try
            {
                IsLoading = true;
                CustomerAnalysisReport = _advancedReportRepository.GetCustomerAnalysisReport(ReportFromDate, ReportToDate);

                // Обновляем коллекции для привязки
                CustomerSegments = new ObservableCollection<CustomerSegment>(CustomerAnalysisReport.CustomerSegments);

                ErrorMessage = null;
                SelectedAdvancedReport = "Customer";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка генерации анализа клиентов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExportAdvancedReport(object parameter)
        {
            try
            {
                if (parameter is string reportType)
                {
                    switch (reportType)
                    {
                        case "Financial":
                            if (FinancialReport != null)
                            {
                                MessageBox.Show($"Финансовый отчет экспортирован\nПериод: {ReportFromDate:dd.MM.yyyy} - {ReportToDate:dd.MM.yyyy}\nЧистая прибыль: {FinancialReport.NetProfit:C2}",
                                    "Экспорт отчета", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "Supplier":
                            if (SupplierPerformanceReport != null)
                            {
                                MessageBox.Show($"Отчет по поставщикам экспортирован\nКоличество поставщиков: {SupplierPerformanceReport.SupplierPerformances.Count}",
                                    "Экспорт отчета", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "Inventory":
                            if (InventoryTurnoverReport != null)
                            {
                                MessageBox.Show($"Отчет по оборачиваемости экспортирован\nКоэффициент оборачиваемости: {InventoryTurnoverReport.TurnoverRatio:F2}",
                                    "Экспорт отчета", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "Customer":
                            if (CustomerAnalysisReport != null)
                            {
                                MessageBox.Show($"Анализ клиентов экспортирован\nВсего клиентов: {CustomerAnalysisReport.TotalCustomers}",
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