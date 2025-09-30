using System;
using System.Drawing;
using System.Windows.Forms;
using SWM.ViewModels;

public class OrderDetailsForm : BaseForm
{
    private OrderDetailsViewModel _viewModel;
    private DataGridView orderItemsGrid;
    private System.Windows.Forms.ComboBox statusComboBox;
    private System.Windows.Forms.TextBox customerNameTextBox;
    private System.Windows.Forms.TextBox customerPhoneTextBox;
    private System.Windows.Forms.TextBox deliveryAddressTextBox;
    private System.Windows.Forms.TextBox discountTextBox;
    private Label totalAmountLabel;
    private Label finalAmountLabel;
    private ModernButton saveButton;
    private ModernButton editButton;
    private ModernButton cancelButton;
    private ModernButton printButton;

    public OrderDetailsForm(string connectionString, int orderId)
    {
        _viewModel = new OrderDetailsViewModel(connectionString, orderId);
        InitializeComponent();
        BindData();

        _viewModel.ShowSuccessMessage = (message) => ShowSuccess(message);
    }

    private void InitializeComponent()
    {
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);
        this.Text = "Детали заказа #";

        CreateHeaderSection();
        CreateCustomerSection();
        CreateOrderItemsSection();
        CreateAddItemSection();
        CreateTotalsSection();
        CreateActionButtons();
    }

    private void CreateHeaderSection()
    {
        // Номер заказа и дата
        var orderNumberLabel = new Label
        {
            Text = "Заказ #",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(orderNumberLabel);

        // Статус заказа
        var statusLabel = new Label { Text = "Статус:", Location = new Point(20, 60), AutoSize = true };
        statusComboBox = new System.Windows.Forms.ComboBox
        {
            Location = new Point(80, 57),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false
        };
        statusComboBox.SelectedValueChanged += (s, e) =>
        {
            if (statusComboBox.SelectedItem is Status selectedStatus)
            {
                _viewModel.Order.StatusID = selectedStatus.StatusID;
            }
        };

        this.Controls.AddRange(new Control[] { statusLabel, statusComboBox });
    }

    private void CreateCustomerSection()
    {
        int y = 100;

        var customerSectionLabel = new Label
        {
            Text = "Информация о клиенте",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        this.Controls.Add(customerSectionLabel);
        y += 35;

        // ФИО клиента
        var nameLabel = new Label { Text = "ФИО*:", Location = new Point(20, y), AutoSize = true };
        customerNameTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(250, 25), Enabled = false };
        this.Controls.AddRange(new Control[] { nameLabel, customerNameTextBox });
        y += 35;

        // Телефон
        var phoneLabel = new Label { Text = "Телефон*:", Location = new Point(20, y), AutoSize = true };
        customerPhoneTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(200, 25), Enabled = false };
        this.Controls.AddRange(new Control[] { phoneLabel, customerPhoneTextBox });
        y += 35;

        // Адрес доставки
        var addressLabel = new Label { Text = "Адрес доставки:", Location = new Point(20, y), AutoSize = true };
        deliveryAddressTextBox = new System.Windows.Forms.TextBox { Location = new Point(120, y - 3), Size = new Size(300, 25), Enabled = false };
        this.Controls.AddRange(new Control[] { addressLabel, deliveryAddressTextBox });
    }

    private void CreateOrderItemsSection()
    {
        int y = 200;

        var itemsLabel = new Label
        {
            Text = "Состав заказа",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        this.Controls.Add(itemsLabel);
        y += 35;

        // Таблица товаров
        orderItemsGrid = new DataGridView();
        orderItemsGrid.Location = new Point(20, y);
        orderItemsGrid.Size = new Size(700, 200);
        orderItemsGrid.BackgroundColor = Color.White;
        orderItemsGrid.BorderStyle = BorderStyle.None;
        orderItemsGrid.RowHeadersVisible = false;
        orderItemsGrid.AllowUserToAddRows = false;
        orderItemsGrid.ReadOnly = true;
        orderItemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // Настройка колонок
        orderItemsGrid.Columns.Add("ProductName", "Товар");
        orderItemsGrid.Columns.Add("Quantity", "Кол-во");
        orderItemsGrid.Columns.Add("UnitPrice", "Цена");
        orderItemsGrid.Columns.Add("Discount", "Скидка %");
        orderItemsGrid.Columns.Add("TotalPrice", "Сумма");

        orderItemsGrid.Columns["Quantity"].Width = 80;
        orderItemsGrid.Columns["UnitPrice"].Width = 100;
        orderItemsGrid.Columns["Discount"].Width = 80;
        orderItemsGrid.Columns["TotalPrice"].Width = 120;

        orderItemsGrid.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
        orderItemsGrid.Columns["TotalPrice"].DefaultCellStyle.Format = "N2";
        orderItemsGrid.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        orderItemsGrid.Columns["TotalPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        orderItemsGrid.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        orderItemsGrid.Columns["Discount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        this.Controls.Add(orderItemsGrid);
    }

    private void CreateAddItemSection()
    {
        int y = 420;

        var addItemLabel = new Label
        {
            Text = "Добавить товар",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        this.Controls.Add(addItemLabel);
        y += 30;

        // Выбор товара
        var productLabel = new Label { Text = "Товар:", Location = new Point(20, y), AutoSize = true };
        var productComboBox = new System.Windows.Forms.ComboBox
        {
            Location = new Point(80, y - 3),
            Size = new Size(250, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false
        };
        productComboBox.DisplayMember = "ProductName";
        productComboBox.SelectedValueChanged += (s, e) =>
        {
            if (productComboBox.SelectedItem is Product selectedProduct)
            {
                _viewModel.SelectedProduct = selectedProduct;
            }
        };

        // Количество
        var quantityLabel = new Label { Text = "Кол-во:", Location = new Point(340, y), AutoSize = true };
        var quantityNumeric = new NumericUpDown
        {
            Location = new Point(400, y - 3),
            Size = new Size(80, 25),
            Minimum = 1,
            Maximum = 1000,
            Enabled = false
        };
        quantityNumeric.ValueChanged += (s, e) =>
        {
            _viewModel.NewOrderItem.Quantity = (int)quantityNumeric.Value;
            _viewModel.CalculateNewItemTotal();
        };

        // Кнопка добавления
        var addButton = new ModernButton
        {
            Text = "➕ Добавить",
            Location = new Point(490, y - 3),
            Size = new Size(100, 25),
            Enabled = false
        };
        addButton.Click += (s, e) => _viewModel.AddOrderItemCommand.Execute(null);

        this.Controls.AddRange(new Control[] {
            productLabel, productComboBox,
            quantityLabel, quantityNumeric,
            addButton
        });

        // Привязка данных для комбобокса товаров
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.AvailableProducts))
            {
                productComboBox.DataSource = null;
                productComboBox.DataSource = _viewModel.AvailableProducts;
                productComboBox.Enabled = _viewModel.IsEditing;
            }

            if (e.PropertyName == nameof(_viewModel.IsEditing))
            {
                productComboBox.Enabled = _viewModel.IsEditing;
                quantityNumeric.Enabled = _viewModel.IsEditing;
                addButton.Enabled = _viewModel.IsEditing;
            }
        };
    }

    private void CreateTotalsSection()
    {
        int y = 470;

        var totalsLabel = new Label
        {
            Text = "Итоги заказа",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        this.Controls.Add(totalsLabel);
        y += 35;

        // Скидка
        var discountLabel = new Label { Text = "Скидка:", Location = new Point(20, y), AutoSize = true };
        discountTextBox = new System.Windows.Forms.TextBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(100, 25),
            Text = "0",
            Enabled = false
        };
        discountTextBox.TextChanged += (s, e) =>
        {
            if (decimal.TryParse(discountTextBox.Text, out decimal discount))
            {
                _viewModel.Order.DiscountAmount = discount;
                UpdateTotals();
            }
        };
        this.Controls.AddRange(new Control[] { discountLabel, discountTextBox });
        y += 35;

        // Общая сумма
        var totalLabel = new Label { Text = "Общая сумма:", Location = new Point(20, y), AutoSize = true };
        totalAmountLabel = new Label
        {
            Text = "0 ₽",
            Location = new Point(120, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60)
        };
        this.Controls.AddRange(new Control[] { totalLabel, totalAmountLabel });
        y += 30;

        // Итоговая сумма
        var finalLabel = new Label { Text = "Итого к оплате:", Location = new Point(20, y), AutoSize = true };
        finalAmountLabel = new Label
        {
            Text = "0 ₽",
            Location = new Point(120, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204)
        };
        this.Controls.AddRange(new Control[] { finalLabel, finalAmountLabel });
    }

    private void CreateActionButtons()
    {
        int y = 550;

        editButton = new ModernButton
        {
            Text = "✏️ Редактировать",
            Location = new Point(20, y),
            Size = new Size(120, 35)
        };
        editButton.Click += (s, e) => SetEditMode(true);

        saveButton = new ModernButton
        {
            Text = "💾 Сохранить",
            Location = new Point(150, y),
            Size = new Size(120, 35)
        };
        saveButton.Click += (s, e) => _viewModel.UpdateOrderCommand.Execute(null);
        saveButton.Visible = false;

        cancelButton = new ModernButton
        {
            Text = "❌ Отмена",
            Location = new Point(280, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(108, 117, 125)
        };
        cancelButton.Click += (s, e) => SetEditMode(false);
        cancelButton.Visible = false;

        printButton = new ModernButton
        {
            Text = "🖨️ Печать",
            Location = new Point(390, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(40, 167, 69)
        };
        printButton.Click += (s, e) => _viewModel.PrintOrderCommand.Execute(null);

        this.Controls.AddRange(new Control[] { editButton, saveButton, cancelButton, printButton });
    }

    private void BindData()
    {
        // Привязка данных заказа
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.Order) && _viewModel.Order != null)
            {
                this.Text = $"Детали заказа #{_viewModel.Order.OrderNumber}";

                customerNameTextBox.Text = _viewModel.Order.CustomerName;
                customerPhoneTextBox.Text = _viewModel.Order.CustomerPhone;
                deliveryAddressTextBox.Text = _viewModel.Order.DeliveryAddress ?? "";

                // Исправление для оператора ??
                decimal discountAmount = _viewModel.Order.DiscountAmount;
                discountTextBox.Text = discountAmount.ToString("N2");

                UpdateTotals();
                RefreshOrderItems();
            }

            if (e.PropertyName == nameof(_viewModel.Statuses))
            {
                statusComboBox.DataSource = null;
                statusComboBox.DataSource = _viewModel.Statuses;
                statusComboBox.DisplayMember = "StatusName";
                statusComboBox.ValueMember = "StatusID";

                if (_viewModel.Order != null)
                {
                    statusComboBox.SelectedValue = _viewModel.Order.StatusID;
                }
            }

            if (e.PropertyName == nameof(_viewModel.IsEditing))
            {
                SetEditMode(_viewModel.IsEditing);
            }

            if (e.PropertyName == nameof(_viewModel.ErrorMessage) && !string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                ShowError(_viewModel.ErrorMessage);
            }
        };

        _viewModel.LoadOrderCommand.Execute(null);
    }

    private void SetEditMode(bool isEditing)
    {
        customerNameTextBox.Enabled = isEditing;
        customerPhoneTextBox.Enabled = isEditing;
        deliveryAddressTextBox.Enabled = isEditing;
        discountTextBox.Enabled = isEditing;
        statusComboBox.Enabled = isEditing;

        editButton.Visible = !isEditing;
        saveButton.Visible = isEditing;
        cancelButton.Visible = isEditing;

        _viewModel.IsEditing = isEditing;
    }

    private void UpdateTotals()
    {
        if (_viewModel.Order != null)
        {
            totalAmountLabel.Text = $"{_viewModel.OrderTotal:N2} ₽";

            // Исправление для оператора ??
            decimal discountAmount = _viewModel.Order.DiscountAmount;
            decimal finalAmount = _viewModel.OrderTotal - discountAmount;
            finalAmountLabel.Text = $"{finalAmount:N2} ₽";
        }
    }

    private void RefreshOrderItems()
    {
        orderItemsGrid.Rows.Clear();

        if (_viewModel.Order?.OrderItems != null)
        {
            foreach (var item in _viewModel.Order.OrderItems)
            {
                // Исправление для оператора ??
                decimal discount = item.Discount ?? 0;
                string productName = item.Product?.ProductName ?? "Товар не найден";

                int rowIndex = orderItemsGrid.Rows.Add(
                    productName,
                    item.Quantity,
                    item.UnitPrice,
                    discount,
                    item.TotalPrice
                );

                // Добавляем кнопку удаления если в режиме редактирования
                if (_viewModel.IsEditing && orderItemsGrid.Columns["Delete"] == null)
                {
                    var deleteButton = new DataGridViewButtonColumn
                    {
                        Name = "Delete",
                        HeaderText = "",
                        Text = "🗑️",
                        UseColumnTextForButtonValue = true,
                        Width = 40
                    };
                    orderItemsGrid.Columns.Add(deleteButton);
                }

                if (_viewModel.IsEditing)
                {
                    orderItemsGrid.Rows[rowIndex].Cells["Delete"].Value = "🗑️";
                }
            }
        }

        // Обработчик клика по кнопке удаления
        orderItemsGrid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == orderItemsGrid.Columns["Delete"]?.Index && _viewModel.IsEditing)
            {
                var item = _viewModel.Order.OrderItems[e.RowIndex];
                var result = ShowQuestion($"Удалить товар \"{item.Product?.ProductName}\" из заказа?");
                if (result == DialogResult.Yes)
                {
                    _viewModel.RemoveOrderItemCommand.Execute(item);
                }
            }
        };
    }
}