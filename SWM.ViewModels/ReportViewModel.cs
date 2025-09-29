using SWM.Core.Models;
using SWM.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWM.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private readonly ReportRepository _reportRepository;

        private SalesReport _salesReport;
        private InventoryReport _inventoryReport;
        private ObservableCollection<PopularProduct> _popularProducts;

        public SalesReport SalesReport
        {
            get => _salesReport;
            set
            {
                _salesReport = value;
                OnPropertyChanged(nameof(SalesReport));
            }
        }

        public InventoryReport InventoryReport
        {
            get => _inventoryReport;
            set
            {
                _inventoryReport = value;
                OnPropertyChanged(nameof(InventoryReport));
            }
        }

        public ObservableCollection<PopularProduct> PopularProducts
        {
            get => _popularProducts;
            set
            {
                _popularProducts = value;
                OnPropertyChanged(nameof(PopularProducts));
            }
        }

        public ReportViewModel(string connectionString)
        {
            _reportRepository = new ReportRepository(connectionString);
            LoadReports();
        }

        public void LoadReports()
        {
            // Загрузка отчетов за последние 30 дней
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30);

            SalesReport = _reportRepository.GetSalesReport(startDate, endDate);
            InventoryReport = _reportRepository.GetInventoryReport();

            var popularProducts = _reportRepository.GetPopularProducts(5);
            PopularProducts = new ObservableCollection<PopularProduct>(popularProducts);
        }

        public void RefreshReports(DateTime startDate, DateTime endDate)
        {
            SalesReport = _reportRepository.GetSalesReport(startDate, endDate);

            // Инвентаризация и популярные товары обновляются по текущему состоянию
            InventoryReport = _reportRepository.GetInventoryReport();

            var popularProducts = _reportRepository.GetPopularProducts(5);
            PopularProducts = new ObservableCollection<PopularProduct>(popularProducts);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}