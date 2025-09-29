using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace SWM.ViewModels
{
    public class AdvancedReportViewModel : INotifyPropertyChanged
    {
        private readonly AdvancedReportRepository _reportRepository;

        private ObservableCollection<SalesTrend> _salesTrends;
        private ObservableCollection<ProductPerformance> _productPerformance;
        private ObservableCollection<SupplierAnalysis> _supplierAnalysis;
        private FinancialReport _financialReport;
        private ReportFilter _currentFilter;

        public ObservableCollection<SalesTrend> SalesTrends
        {
            get => _salesTrends;
            set
            {
                _salesTrends = value;
                OnPropertyChanged(nameof(SalesTrends));
            }
        }

        public ObservableCollection<ProductPerformance> ProductPerformance
        {
            get => _productPerformance;
            set
            {
                _productPerformance = value;
                OnPropertyChanged(nameof(ProductPerformance));
            }
        }

        public ObservableCollection<SupplierAnalysis> SupplierAnalysis
        {
            get => _supplierAnalysis;
            set
            {
                _supplierAnalysis = value;
                OnPropertyChanged(nameof(SupplierAnalysis));
            }
        }

        public FinancialReport FinancialReport
        {
            get => _financialReport;
            set
            {
                _financialReport = value;
                OnPropertyChanged(nameof(FinancialReport));
            }
        }

        public ReportFilter CurrentFilter
        {
            get => _currentFilter;
            set
            {
                _currentFilter = value;
                OnPropertyChanged(nameof(CurrentFilter));
            }
        }

        public AdvancedReportViewModel(string connectionString)
        {
            _reportRepository = new AdvancedReportRepository(connectionString);
            CurrentFilter = new ReportFilter();
            LoadReports();
        }

        public void LoadReports()
        {
            SalesTrends = new ObservableCollection<SalesTrend>(_reportRepository.GetSalesTrends(CurrentFilter));
            ProductPerformance = new ObservableCollection<ProductPerformance>(_reportRepository.GetProductPerformance(CurrentFilter));
            SupplierAnalysis = new ObservableCollection<SupplierAnalysis>(_reportRepository.GetSupplierAnalysis(CurrentFilter));
            FinancialReport = _reportRepository.GetFinancialReport(CurrentFilter);
        }

        public void ApplyFilter(ReportFilter filter)
        {
            CurrentFilter = filter;
            LoadReports();
        }

        public void ExportToExcel()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Экспорт отчета в Excel",
                    FileName = $"Отчет_склада_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Упрощенный экспорт - в реальном приложении использовать библиотеку типа EPPlus
                    System.IO.File.WriteAllText(saveDialog.FileName.Replace(".xlsx", ".csv"), GenerateCsvReport());
                    MessageBox.Show("Отчет успешно экспортирован!", "Экспорт",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateCsvReport()
        {
            var csv = "Отчет по складу\n";
            csv += $"Период: {CurrentFilter.StartDate:dd.MM.yyyy} - {CurrentFilter.EndDate:dd.MM.yyyy}\n\n";

            csv += "Тренды продаж:\n";
            csv += "Период;Кол-во заказов;Выручка;Товаров продано;Средний чек\n";
            foreach (var trend in SalesTrends)
            {
                csv += $"{trend.PeriodDisplay};{trend.OrdersCount};{trend.Revenue:N2};{trend.ProductsSold};{trend.AverageOrderValue:N2}\n";
            }

            csv += "\nТоп товаров:\n";
            csv += "Товар;Артикул;Категория;Продано;Выручка;Оборачиваемость\n";
            foreach (var product in ProductPerformance.Take(10))
            {
                csv += $"{product.ProductName};{product.ArticleNumber};{product.Category};{product.QuantitySold};{product.TotalRevenue:N2};{product.TurnoverRate:N1}%\n";
            }

            return csv;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}