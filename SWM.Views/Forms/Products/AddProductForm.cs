using System;
using System.Drawing;
using System.Windows.Forms;
using SWM.ViewModels;
using SWM.Core.Models;

public class AddProductForm : BaseForm
{
    private ProductViewModel _viewModel;
    private System.Windows.Forms.TextBox articleNumberTextBox;
    private System.Windows.Forms.TextBox productNameTextBox;
    private System.Windows.Forms.TextBox descriptionTextBox;
    private System.Windows.Forms.TextBox purchasePriceTextBox;
    private System.Windows.Forms.TextBox salePriceTextBox;
    private System.Windows.Forms.TextBox stockBalanceTextBox;
    private System.Windows.Forms.TextBox minStockTextBox;
    private System.Windows.Forms.TextBox maxStockTextBox;
    private System.Windows.Forms.ComboBox supplierComboBox;
    private ModernButton saveButton;

    public AddProductForm(ProductViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();

        if (_viewModel.SelectedProduct != null && _viewModel.SelectedProduct.ProductID > 0)
        {
            LoadProductData(_viewModel.SelectedProduct);
            this.Text = "Редактирование товара";
        }
        else
        {
            this.Text = "Добавление нового товара";
        }
    }

    private void InitializeComponent()
    {
        this.Size = new Size(500, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Заголовок
        var titleLabel = new Label
        {
            Text = this.Text,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(titleLabel);

        int y = 70;

        // Артикул
        var articleLabel = new Label { Text = "Артикул*:", Location = new Point(20, y), AutoSize = true };
        articleNumberTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(200, 25) };
        this.Controls.AddRange(new Control[] { articleLabel, articleNumberTextBox });
        y += 40;

        // Наименование
        var nameLabel = new Label { Text = "Наименование*:", Location = new Point(20, y), AutoSize = true };
        productNameTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(300, 25) };
        this.Controls.AddRange(new Control[] { nameLabel, productNameTextBox });
        y += 40;

        // Поставщик
        var supplierLabel = new Label { Text = "Поставщик:", Location = new Point(20, y), AutoSize = true };
        supplierComboBox = new System.Windows.Forms.ComboBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(250, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Заполняем комбобокс поставщиками
        foreach (var supplier in _viewModel.Suppliers)
        {
            supplierComboBox.Items.Add(new SupplierComboBoxItem
            {
                Text = supplier.SupplierName,
                Value = supplier.SupplierID
            });
        }
        supplierComboBox.DisplayMember = "Text";

        this.Controls.AddRange(new Control[] { supplierLabel, supplierComboBox });
        y += 40;

        // Цена закупки
        var purchasePriceLabel = new Label { Text = "Цена закупки:", Location = new Point(20, y), AutoSize = true };
        purchasePriceTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(150, 25), Text = "0" };
        this.Controls.AddRange(new Control[] { purchasePriceLabel, purchasePriceTextBox });
        y += 40;

        // Цена продажи
        var salePriceLabel = new Label { Text = "Цена продажи:", Location = new Point(20, y), AutoSize = true };
        salePriceTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(150, 25), Text = "0" };
        this.Controls.AddRange(new Control[] { salePriceLabel, salePriceTextBox });
        y += 40;

        // Остаток
        var stockLabel = new Label { Text = "Остаток:", Location = new Point(20, y), AutoSize = true };
        stockBalanceTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(150, 25), Text = "0" };
        this.Controls.AddRange(new Control[] { stockLabel, stockBalanceTextBox });
        y += 40;

        // Мин запас
        var minStockLabel = new Label { Text = "Мин. запас:", Location = new Point(20, y), AutoSize = true };
        minStockTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(150, 25), Text = "0" };
        this.Controls.AddRange(new Control[] { minStockLabel, minStockTextBox });
        y += 40;

        // Макс запас
        var maxStockLabel = new Label { Text = "Макс. запас:", Location = new Point(20, y), AutoSize = true };
        maxStockTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(150, 25), Text = "0" };
        this.Controls.AddRange(new Control[] { maxStockLabel, maxStockTextBox });
        y += 50;

        // Описание
        var descLabel = new Label { Text = "Описание:", Location = new Point(20, y), AutoSize = true };
        descriptionTextBox = new System.Windows.Forms.TextBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(300, 80),
            Multiline = true
        };
        this.Controls.AddRange(new Control[] { descLabel, descriptionTextBox });
        y += 100;

        // Кнопки
        saveButton = new ModernButton
        {
            Text = "💾 Сохранить",
            Location = new Point(120, y),
            Size = new Size(120, 35)
        };
        saveButton.Click += SaveButton_Click;

        var cancelButton = new ModernButton
        {
            Text = "❌ Отмена",
            Location = new Point(250, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(108, 117, 125)
        };
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { saveButton, cancelButton });
    }

    private void LoadProductData(Product product)
    {
        articleNumberTextBox.Text = product.ArticleNumber;
        productNameTextBox.Text = product.ProductName;
        descriptionTextBox.Text = product.Description;
        purchasePriceTextBox.Text = product.PurchasePrice.ToString("N2");
        salePriceTextBox.Text = product.SalePrice.ToString("N2");
        stockBalanceTextBox.Text = product.StockBalance.ToString();
        minStockTextBox.Text = product.MinStockLevel.ToString();
        maxStockTextBox.Text = product.MaxStockLevel.ToString();

        // Устанавливаем поставщика
        if (product.SupplierID.HasValue)
        {
            foreach (SupplierComboBoxItem item in supplierComboBox.Items)
            {
                if (item.Value == product.SupplierID.Value)
                {
                    supplierComboBox.SelectedItem = item;
                    break;
                }
            }
        }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        if (ValidateForm())
        {
            // Обновляем EditingProduct во ViewModel
            _viewModel.EditingProduct.ArticleNumber = articleNumberTextBox.Text.Trim();
            _viewModel.EditingProduct.ProductName = productNameTextBox.Text.Trim();
            _viewModel.EditingProduct.Description = descriptionTextBox.Text.Trim();
            _viewModel.EditingProduct.PurchasePrice = decimal.Parse(purchasePriceTextBox.Text);
            _viewModel.EditingProduct.SalePrice = decimal.Parse(salePriceTextBox.Text);
            _viewModel.EditingProduct.StockBalance = int.Parse(stockBalanceTextBox.Text);
            _viewModel.EditingProduct.MinStockLevel = int.Parse(minStockTextBox.Text);
            _viewModel.EditingProduct.MaxStockLevel = int.Parse(maxStockTextBox.Text);

            if (supplierComboBox.SelectedItem is SupplierComboBoxItem selectedSupplier)
            {
                _viewModel.EditingProduct.SupplierID = selectedSupplier.Value;
            }

            // Выполняем команду сохранения
            _viewModel.SaveProductCommand.Execute(null);

            if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(articleNumberTextBox.Text))
        {
            ShowError("Введите артикул товара");
            articleNumberTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(productNameTextBox.Text))
        {
            ShowError("Введите наименование товара");
            productNameTextBox.Focus();
            return false;
        }

        if (!decimal.TryParse(purchasePriceTextBox.Text, out decimal purchasePrice) || purchasePrice < 0)
        {
            ShowError("Введите корректную цену закупки");
            purchasePriceTextBox.Focus();
            return false;
        }

        if (!decimal.TryParse(salePriceTextBox.Text, out decimal salePrice) || salePrice < 0)
        {
            ShowError("Введите корректную цену продажи");
            salePriceTextBox.Focus();
            return false;
        }

        if (!int.TryParse(stockBalanceTextBox.Text, out int stock) || stock < 0)
        {
            ShowError("Введите корректное количество на складе");
            stockBalanceTextBox.Focus();
            return false;
        }

        return true;
    }
}

// Вспомогательный класс для комбобокса
public class SupplierComboBoxItem
{
    public string Text { get; set; } = string.Empty;
    public int Value { get; set; }
}