using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SWM.ViewModels;
using SWM.Core.Models;

namespace SWM.Views.Forms.Reports
{
    public class AdvancedReportsForm : Form
    {
        private AdvancedReportViewModel _viewModel;

        // Элементы фильтрации
        private DateTimePicker dtpStartDate, dtpEndDate;
        private ComboBox cmbPeriodType;
        private Button btnApplyFilter, btnExport, btnRefresh;

        // Элементы отображения
        private TabControl tabControl;
        private DataGridView gridSalesTrends, gridProducts, gridSuppliers;
        private Label lblFinancialSummary;

        public AdvancedReportsForm(string connectionString)
        {
            _viewModel = new AdvancedReportViewModel(connectionString);
            InitializeComponent();
            LoadReports();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "📊 Расширенные отчеты и аналитика";
            this.ClientSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;

            CreateControls();
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            int yPos = 20;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "Расширенная аналитика склада",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 40;

            // Панель фильтров
            var filterPanel = CreateFilterPanel(20, yPos);
            yPos += 60;

            // Табы с отчетами
            tabControl = new TabControl()
            {
                Location = new Point(20, yPos),
                Size = new Size(1050, 550)
            };

            // Вкладка трендов продаж
            var tabSales = new TabPage("📈 Тренды продаж");
            gridSalesTrends = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            tabSales.Controls.Add(gridSalesTrends);

            // Вкладка эффективности товаров
            var tabProducts = new TabPage("📦 Эффективность товаров");
            gridProducts = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            tabProducts.Controls.Add(gridProducts);

            // Вкладка анализа поставщиков
            var tabSuppliers = new TabPage("🚚 Анализ поставщиков");
            gridSuppliers = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            tabSuppliers.Controls.Add(gridSuppliers);

            // Вкладка финансового отчета
            var tabFinancial = new TabPage("💰 Финансовый отчет");
            lblFinancialSummary = new Label()
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Font = new Font("Arial", 10),
                Padding = new Padding(20)
            };
            tabFinancial.Controls.Add(lblFinancialSummary);

            tabControl.TabPages.AddRange(new TabPage[] { tabSales, tabProducts, tabSuppliers, tabFinancial });

            this.Controls.AddRange(new Control[] {
                lblTitle, filterPanel, tabControl
            });

            SetupGrids();
        }

        private Panel CreateFilterPanel(int x, int y)
        {
            var panel = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(1050, 50),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPeriod = new Label() { Text = "Период:", Location = new Point(10, 15) };
            dtpStartDate = new DateTimePicker()
            {
                Location = new Point(60, 12),
                Width = 120,
                Value = DateTime.Today.AddMonths(-1)
            };
            var lblTo = new Label() { Text = "по", Location = new Point(190, 15) };
            dtpEndDate = new DateTimePicker()
            {
                Location = new Point(210, 12),
                Width = 120,
                Value = DateTime.Today
            };

            var lblPeriodType = new Label() { Text = "Группировка:", Location = new Point(350, 15) };
            cmbPeriodType = new ComboBox()
            {
                Location = new Point(430, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPeriodType.Items.AddRange(new object[] {
                new { Text = "По дням", Value = ReportPeriod.Daily },
                new { Text = "По неделям", Value = ReportPeriod.Weekly },
                new { Text = "По месяцам", Value = ReportPeriod.Monthly },
                new { Text = "По кварталам", Value = ReportPeriod.Quarterly },
                new { Text = "По годам", Value = ReportPeriod.Yearly }
            });
            cmbPeriodType.DisplayMember = "Text";
            cmbPeriodType.ValueMember = "Value";
            cmbPeriodType.SelectedIndex = 2;

            btnApplyFilter = new Button()
            {
                Text = "🔍 Применить фильтр",
                Location = new Point(570, 10),
                Size = new Size(140, 25),
                BackColor = Color.LightBlue
            };
            btnApplyFilter.Click += BtnApplyFilter_Click;

            btnExport = new Button()
            {
                Text = "📤 Экспорт в Excel",
                Location = new Point(720, 10),
                Size = new Size(120, 25),
                BackColor = Color.LightGreen
            };
            btnExport.Click += BtnExport_Click;

            btnRefresh = new Button()
            {
                Text = "🔄 Обновить",
                Location = new Point(850, 10),
                Size = new Size(100, 25)
            };
            btnRefresh.Click += BtnRefresh_Click;

            panel.Controls.AddRange(new Control[] {
                lblPeriod, dtpStartDate, lblTo, dtpEndDate,
                lblPeriodType, cmbPeriodType,
                btnApplyFilter, btnExport, btnRefresh
            });

            return panel;
        }

        private void SetupGrids()
        {
            // Настройка сетки трендов продаж
            gridSalesTrends.AutoGenerateColumns = false;
            gridSalesTrends.Columns.Clear();
            gridSalesTrends.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "PeriodDisplay",
                HeaderText = "Период",
                Width = 120
            });
            gridSalesTrends.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "OrdersCount",
                HeaderText = "Заказов",
                Width = 80
            });
            gridSalesTrends.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Revenue",
                HeaderText = "Выручка",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridSalesTrends.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductsSold",
                HeaderText = "Товаров продано",
                Width = 120
            });
            gridSalesTrends.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "AverageOrderValue",
                HeaderText = "Средний чек",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            // Настройка сетки товаров
            gridProducts.AutoGenerateColumns = false;
            gridProducts.Columns.Clear();
            gridProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductName",
                HeaderText = "Товар",
                Width = 200
            });
            gridProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Category",
                HeaderText = "Категория",
                Width = 100
            });
            gridProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "QuantitySold",
                HeaderText = "Продано",
                Width = 80
            });
            gridProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TotalRevenue",
                HeaderText = "Выручка",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TurnoverRate",
                HeaderText = "Оборачиваемость",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N1" }
            });
            gridProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "CurrentStock",
                HeaderText = "В наличии",
                Width = 80
            });

            // Настройка сетки поставщиков
            gridSuppliers.AutoGenerateColumns = false;
            gridSuppliers.Columns.Clear();
            gridSuppliers.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SupplierName",
                HeaderText = "Поставщик",
                Width = 200
            });
            gridSuppliers.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DeliveriesCount",
                HeaderText = "Поставок",
                Width = 80
            });
            gridSuppliers.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TotalDeliveredValue",
                HeaderText = "Общая стоимость",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridSuppliers.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductsSupplied",
                HeaderText = "Товаров",
                Width = 80
            });
            gridSuppliers.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ReliabilityScore",
                HeaderText = "Надежность",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N1" }
            });
        }

        private void LoadReports()
        {
            gridSalesTrends.DataSource = _viewModel.SalesTrends.ToList();
            gridProducts.DataSource = _viewModel.ProductPerformance.ToList();
            gridSuppliers.DataSource = _viewModel.SupplierAnalysis.ToList();
            UpdateFinancialSummary();
        }

        private void UpdateFinancialSummary()
        {
            if (_viewModel.FinancialReport != null)
            {
                var report = _viewModel.FinancialReport;
                lblFinancialSummary.Text =
                    $"💰 ФИНАНСОВЫЙ ОТЧЕТ\n" +
                    $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}\n\n" +
                    $"Выручка: {report.TotalRevenue:N2} руб\n" +
                    $"Себестоимость: {report.TotalCost:N2} руб\n" +
                    $"Валовая прибыль: {report.GrossProfit:N2} руб\n" +
                    $"Валовая маржа: {report.GrossMargin:N1}%\n\n" +
                    $"Операционные расходы: {report.OperatingExpenses:N2} руб\n" +
                    $"Чистая прибыль: {report.NetProfit:N2} руб\n" +
                    $"Чистая маржа: {report.NetMargin:N1}%\n\n" +
                    $"{(report.NetProfit > 0 ? "✅ Прибыльный период" : "❌ Убыточный период")}";
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            var filter = new ReportFilter
            {
                StartDate = dtpStartDate.Value,
                EndDate = dtpEndDate.Value,
                PeriodType = ((dynamic)cmbPeriodType.SelectedItem).Value
            };

            _viewModel.ApplyFilter(filter);
            LoadReports();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            _viewModel.ExportToExcel();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReports();
        }
    }
}