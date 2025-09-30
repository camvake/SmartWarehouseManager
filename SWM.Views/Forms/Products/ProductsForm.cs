using System;
using System.Drawing;
using System.Windows.Forms;
using SWM.ViewModels;
using SWM.Core.Models;

public class ProductsForm : BaseForm
{
    private ProductViewModel _viewModel;
    private DataGridView productsGrid;
    private ModernButton addButton;
    private ModernButton editButton;
    private ModernButton deleteButton;
    private ModernButton refreshButton;
    private System.Windows.Forms.TextBox searchTextBox;
    private CheckBox lowStockCheckBox;

    public ProductsForm(string connectionString)
    {
        _viewModel = new ProductViewModel(connectionString);
        InitializeComponent();
        BindData();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(980, 740);
        this.BackColor = Color.FromArgb(250, 250, 250);

        // Заголовок
        var titleLabel = new Label();
        titleLabel.Text = "Управление товарами";
        titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        titleLabel.ForeColor = Color.FromArgb(60, 60, 60);
        titleLabel.Location = new Point(30, 30);
        titleLabel.AutoSize = true;
        this.Controls.Add(titleLabel);

        // Панель инструментов
        var toolbarPanel = new Panel();
        toolbarPanel.Size = new Size(920, 50);
        toolbarPanel.Location = new Point(30, 80);
        toolbarPanel.BackColor = Color.White;
        this.Controls.Add(toolbarPanel);

        // Поиск
        searchTextBox = new System.Windows.Forms.TextBox();
        searchTextBox.Size = new Size(200, 30);
        searchTextBox.Location = new Point(10, 10);
        searchTextBox.Font = new Font("Segoe UI", 9);
        searchTextBox.PlaceholderText = "Поиск товаров...";
        searchTextBox.BorderStyle = BorderStyle.FixedSingle;
        searchTextBox.TextChanged += (s, e) =>
        {
            _viewModel.SearchText = searchTextBox.Text;
            _viewModel.SearchProductsCommand.Execute(null);
        };
        toolbarPanel.Controls.Add(searchTextBox);

        // Чекбокс низкого запаса
        lowStockCheckBox = new CheckBox();
        lowStockCheckBox.Text = "Только низкий запас";
        lowStockCheckBox.Location = new Point(220, 12);
        lowStockCheckBox.Font = new Font("Segoe UI", 9);
        lowStockCheckBox.CheckedChanged += (s, e) => _viewModel.ShowLowStockOnly = lowStockCheckBox.Checked;
        toolbarPanel.Controls.Add(lowStockCheckBox);

        // Кнопки
        addButton = new ModernButton();
        addButton.Text = "➕ Добавить";
        addButton.Size = new Size(100, 30);
        addButton.Location = new Point(380, 10);
        addButton.Click += (s, e) => AddProduct();
        toolbarPanel.Controls.Add(addButton);

        editButton = new ModernButton();
        editButton.Text = "✏️ Изменить";
        editButton.Size = new Size(100, 30);
        editButton.Location = new Point(490, 10);
        editButton.BackColor = Color.FromArgb(255, 193, 7);
        editButton.Click += (s, e) => EditProduct();
        toolbarPanel.Controls.Add(editButton);

        deleteButton = new ModernButton();
        deleteButton.Text = "🗑️ Удалить";
        deleteButton.Size = new Size(100, 30);
        deleteButton.Location = new Point(600, 10);
        deleteButton.BackColor = Color.FromArgb(220, 53, 69);
        deleteButton.Click += (s, e) => DeleteProduct();
        toolbarPanel.Controls.Add(deleteButton);

        refreshButton = new ModernButton();
        refreshButton.Text = "🔄 Обновить";
        refreshButton.Size = new Size(100, 30);
        refreshButton.Location = new Point(710, 10);
        refreshButton.BackColor = Color.FromArgb(108, 117, 125);
        refreshButton.Click += (s, e) => _viewModel.LoadProductsCommand.Execute(null);
        toolbarPanel.Controls.Add(refreshButton);

        // Таблица товаров
        productsGrid = new DataGridView();
        productsGrid.Size = new Size(920, 580);
        productsGrid.Location = new Point(30, 140);
        productsGrid.BackgroundColor = Color.White;
        productsGrid.BorderStyle = BorderStyle.None;
        productsGrid.Font = new Font("Segoe UI", 9);
        productsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        productsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        productsGrid.RowHeadersVisible = false;
        productsGrid.AllowUserToAddRows = false;
        productsGrid.ReadOnly = true;
        productsGrid.SelectionChanged += (s, e) => UpdateButtonStates();

        // Стилизация
        productsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        productsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        productsGrid.EnableHeadersVisualStyles = false;

        this.Controls.Add(productsGrid);
    }

    private void BindData()
    {
        // Настройка колонок
        productsGrid.Columns.Clear();
        productsGrid.Columns.Add("ArticleNumber", "Артикул");
        productsGrid.Columns.Add("ProductName", "Наименование");
        productsGrid.Columns.Add("Category", "Категория");
        productsGrid.Columns.Add("SalePrice", "Цена продажи");
        productsGrid.Columns.Add("StockBalance", "Остаток");
        productsGrid.Columns.Add("MinStockLevel", "Мин. запас");
        productsGrid.Columns.Add("Status", "Статус");

        productsGrid.Columns["SalePrice"].DefaultCellStyle.Format = "N2";
        productsGrid.Columns["SalePrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        productsGrid.Columns["StockBalance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        productsGrid.Columns["MinStockLevel"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        // Привязка данных
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.Products))
            {
                productsGrid.Rows.Clear();
                foreach (var product in _viewModel.Products)
                {
                    int rowIndex = productsGrid.Rows.Add(
                        product.ArticleNumber,
                        product.ProductName,
                        product.Category?.CategoryName ?? "",
                        product.SalePrice,
                        product.StockBalance,
                        product.MinStockLevel,
                        product.IsLowStock ? "Низкий запас" : "Норма"
                    );

                    // Раскрашиваем строку если низкий запас
                    if (product.IsLowStock)
                    {
                        productsGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 238, 238);
                        productsGrid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(220, 53, 69);
                    }
                }
            }

            if (e.PropertyName == nameof(_viewModel.ErrorMessage) && !string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                ShowError(_viewModel.ErrorMessage);
            }
        };

        _viewModel.LoadProductsCommand.Execute(null);
    }

    private void UpdateButtonStates()
    {
        bool hasSelection = productsGrid.SelectedRows.Count > 0;
        editButton.Enabled = hasSelection;
        deleteButton.Enabled = hasSelection;
    }

    private void AddProduct()
    {
        var addForm = new AddProductForm(_viewModel);
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            _viewModel.LoadProductsCommand.Execute(null);
        }
    }

    private void EditProduct()
    {
        if (productsGrid.SelectedRows.Count > 0)
        {
            var selectedRow = productsGrid.SelectedRows[0];
            var articleNumber = selectedRow.Cells["ArticleNumber"].Value?.ToString();

            if (!string.IsNullOrEmpty(articleNumber))
            {
                var product = _viewModel.Products.FirstOrDefault(p => p.ArticleNumber == articleNumber);

                if (product != null)
                {
                    _viewModel.SelectedProduct = product;
                    var editForm = new AddProductForm(_viewModel);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _viewModel.LoadProductsCommand.Execute(null);
                    }
                }
            }
        }
        else
        {
            ShowWarning("Выберите товар для редактирования");
        }
    }

    private void DeleteProduct()
    {
        if (productsGrid.SelectedRows.Count > 0)
        {
            var selectedRow = productsGrid.SelectedRows[0];
            var articleNumber = selectedRow.Cells["ArticleNumber"].Value?.ToString();
            var productName = selectedRow.Cells["ProductName"].Value?.ToString();

            if (!string.IsNullOrEmpty(articleNumber) && !string.IsNullOrEmpty(productName))
            {
                var product = _viewModel.Products.FirstOrDefault(p => p.ArticleNumber == articleNumber);

                if (product != null)
                {
                    var result = ShowQuestion($"Вы уверены, что хотите удалить товар \"{productName}\"?");
                    if (result == DialogResult.Yes)
                    {
                        _viewModel.DeleteProductCommand.Execute(product.ProductID);
                        _viewModel.LoadProductsCommand.Execute(null);
                    }
                }
            }
        }
        else
        {
            ShowWarning("Выберите товар для удаления");
        }
    }
}