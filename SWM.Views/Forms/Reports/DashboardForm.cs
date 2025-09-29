using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SWM.ViewModels;

namespace SWM.Views.Forms.Reports
{
    public class DashboardForm : Form
    {
        private ReportViewModel _viewModel;

        // Элементы для отображения статистики
        private Label lblTotalOrders, lblTotalRevenue, lblAvgOrderValue;
        private Label lblTotalProducts, lblLowStock, lblOutOfStock, lblInventoryValue;
        private DataGridView gridPopularProducts;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private Button btnRefresh;

        public DashboardForm(string connectionString)
        {
            _viewModel = new ReportViewModel(connectionString);
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Дашборд - Аналитика склада";
            this.ClientSize = new Size(1000, 700);
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
                Text = "📊 Дашборд управления складом",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 40;

            // Период для отчетов
            var lblPeriod = new Label() { Text = "Период отчета:", Location = new Point(20, yPos + 3) };
            dtpStartDate = new DateTimePicker()
            {
                Location = new Point(120, yPos),
                Width = 120,
                Value = DateTime.Today.AddDays(-30)
            };
            var lblTo = new Label() { Text = "по", Location = new Point(250, yPos + 3) };
            dtpEndDate = new DateTimePicker()
            {
                Location = new Point(270, yPos),
                Width = 120,
                Value = DateTime.Today
            };
            btnRefresh = new Button()
            {
                Text = "🔄 Обновить",
                Location = new Point(400, yPos),
                Size = new Size(100, 23)
            };
            btnRefresh.Click += BtnRefresh_Click;
            yPos += 35;

            // Панель продаж
            var salesPanel = CreateSalesPanel(20, yPos);
            yPos += 120;

            // Панель инвентаризации
            var inventoryPanel = CreateInventoryPanel(20, yPos);
            yPos += 120;

            // Популярные товары
            var popularPanel = CreatePopularProductsPanel(20, yPos);

            this.Controls.AddRange(new Control[] {
                lblTitle, lblPeriod, dtpStartDate, lblTo, dtpEndDate, btnRefresh,
                salesPanel, inventoryPanel, popularPanel
            });
        }

        private Panel CreateSalesPanel(int x, int y)
        {
            var panel = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(600, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightCyan
            };

            var lblTitle = new Label()
            {
                Text = "💰 Продажи",
                Location = new Point(10, 10),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };

            lblTotalOrders = new Label()
            {
                Text = "Всего заказов: 0",
                Location = new Point(10, 35),
                AutoSize = true
            };

            lblTotalRevenue = new Label()
            {
                Text = "Общая выручка: 0 руб",
                Location = new Point(10, 55),
                AutoSize = true
            };

            lblAvgOrderValue = new Label()
            {
                Text = "Средний чек: 0 руб",
                Location = new Point(10, 75),
                AutoSize = true
            };

            panel.Controls.AddRange(new Control[] { lblTitle, lblTotalOrders, lblTotalRevenue, lblAvgOrderValue });
            return panel;
        }

        private Panel CreateInventoryPanel(int x, int y)
        {
            var panel = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(600, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            var lblTitle = new Label()
            {
                Text = "📦 Инвентаризация",
                Location = new Point(10, 10),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };

            lblTotalProducts = new Label()
            {
                Text = "Всего товаров: 0",
                Location = new Point(10, 35),
                AutoSize = true
            };

            lblLowStock = new Label()
            {
                Text = "Товаров мало: 0",
                Location = new Point(200, 35),
                ForeColor = Color.Orange,
                AutoSize = true
            };

            lblOutOfStock = new Label()
            {
                Text = "Нет в наличии: 0",
                Location = new Point(350, 35),
                ForeColor = Color.Red,
                AutoSize = true
            };

            lblInventoryValue = new Label()
            {
                Text = "Общая стоимость: 0 руб",
                Location = new Point(10, 75),
                Font = new Font("Arial", 9, FontStyle.Bold),
                AutoSize = true
            };

            panel.Controls.AddRange(new Control[] {
                lblTitle, lblTotalProducts, lblLowStock, lblOutOfStock, lblInventoryValue
            });
            return panel;
        }

        private Panel CreatePopularProductsPanel(int x, int y)
        {
            var panel = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(600, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label()
            {
                Text = "🔥 Популярные товары (за 30 дней)",
                Location = new Point(10, 10),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };

            gridPopularProducts = new DataGridView()
            {
                Location = new Point(10, 35),
                Size = new Size(580, 150),
                ReadOnly = true,
                RowHeadersVisible = false
            };

            panel.Controls.AddRange(new Control[] { lblTitle, gridPopularProducts });
            return panel;
        }

        private void LoadDashboard()
        {
            UpdateSalesDisplay();
            UpdateInventoryDisplay();
            SetupPopularProductsGrid();
        }

        private void UpdateSalesDisplay()
        {
            if (_viewModel.SalesReport != null)
            {
                lblTotalOrders.Text = $"Всего заказов: {_viewModel.SalesReport.TotalOrders}";
                lblTotalRevenue.Text = $"Общая выручка: {_viewModel.SalesReport.TotalRevenue:N2} руб";
                lblAvgOrderValue.Text = $"Средний чек: {_viewModel.SalesReport.AverageOrderValue:N2} руб";
            }
        }

        private void UpdateInventoryDisplay()
        {
            if (_viewModel.InventoryReport != null)
            {
                lblTotalProducts.Text = $"Всего товаров: {_viewModel.InventoryReport.TotalProducts}";
                lblLowStock.Text = $"Товаров мало: {_viewModel.InventoryReport.LowStockProducts}";
                lblOutOfStock.Text = $"Нет в наличии: {_viewModel.InventoryReport.OutOfStockProducts}";
                lblInventoryValue.Text = $"Общая стоимость: {_viewModel.InventoryReport.TotalInventoryValue:N2} руб";
            }
        }

        private void SetupPopularProductsGrid()
        {
            gridPopularProducts.AutoGenerateColumns = false;
            gridPopularProducts.Columns.Clear();

            gridPopularProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductName",
                HeaderText = "Товар",
                Width = 200
            });
            gridPopularProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ArticleNumber",
                HeaderText = "Артикул",
                Width = 100
            });
            gridPopularProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "QuantitySold",
                HeaderText = "Продано",
                Width = 80
            });
            gridPopularProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TotalRevenue",
                HeaderText = "Выручка",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            gridPopularProducts.DataSource = _viewModel.PopularProducts;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _viewModel.RefreshReports(dtpStartDate.Value, dtpEndDate.Value);
            UpdateSalesDisplay();
            UpdateInventoryDisplay();
            gridPopularProducts.DataSource = _viewModel.PopularProducts;

            MessageBox.Show("Отчеты обновлены!", "Обновление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}