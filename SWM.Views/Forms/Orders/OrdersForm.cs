using System;
using System.Drawing;
using System.Windows.Forms;

public class OrdersForm : BaseForm
{
    private DataGridView ordersGrid;
    private ModernButton addButton;
    private ModernButton editButton;
    private ModernButton deleteButton;
    private ModernButton detailsButton;
    private TextBox searchTextBox;
    private ComboBox statusFilterCombo;

    public OrdersForm()
    {
        InitializeComponent();
        LoadOrdersData();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(980, 740);
        this.BackColor = Color.FromArgb(250, 250, 250);

        // Заголовок
        var titleLabel = new Label();
        titleLabel.Text = "Управление заказами";
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
        toolbarPanel.Paint += (s, e) =>
        {
            using (var pen = new Pen(Color.FromArgb(240, 240, 240)))
                e.Graphics.DrawLine(pen, 0, 49, toolbarPanel.Width, 49);
        };
        this.Controls.Add(toolbarPanel);

        // Поле поиска
        searchTextBox = new TextBox();
        searchTextBox.Size = new Size(200, 30);
        searchTextBox.Location = new Point(10, 10);
        searchTextBox.Font = new Font("Segoe UI", 9);
        searchTextBox.PlaceholderText = "Поиск заказов...";
        searchTextBox.BorderStyle = BorderStyle.FixedSingle;
        searchTextBox.TextChanged += SearchTextBox_TextChanged;
        toolbarPanel.Controls.Add(searchTextBox);

        // Фильтр по статусу
        statusFilterCombo = new ComboBox();
        statusFilterCombo.Size = new Size(150, 30);
        statusFilterCombo.Location = new Point(220, 10);
        statusFilterCombo.Font = new Font("Segoe UI", 9);
        statusFilterCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        statusFilterCombo.Items.AddRange(new object[] { "Все статусы", "Новый", "В обработке", "Выполнен", "Отменен" });
        statusFilterCombo.SelectedIndex = 0;
        statusFilterCombo.SelectedIndexChanged += StatusFilterCombo_SelectedIndexChanged;
        toolbarPanel.Controls.Add(statusFilterCombo);

        // Кнопки
        addButton = new ModernButton();
        addButton.Text = "➕ Новый заказ";
        addButton.Size = new Size(120, 30);
        addButton.Location = new Point(380, 10);
        addButton.Click += (s, e) => CreateNewOrder();
        toolbarPanel.Controls.Add(addButton);

        editButton = new ModernButton();
        editButton.Text = "✏️ Редактировать";
        editButton.Size = new Size(140, 30);
        editButton.Location = new Point(510, 10);
        editButton.BackColor = Color.FromArgb(255, 193, 7);
        editButton.Click += (s, e) => EditOrder();
        toolbarPanel.Controls.Add(editButton);

        deleteButton = new ModernButton();
        deleteButton.Text = "🗑️ Удалить";
        deleteButton.Size = new Size(100, 30);
        deleteButton.Location = new Point(660, 10);
        deleteButton.BackColor = Color.FromArgb(220, 53, 69);
        deleteButton.Click += (s, e) => DeleteOrder();
        toolbarPanel.Controls.Add(deleteButton);

        // Кнопка деталей
        detailsButton = new ModernButton();
        detailsButton.Text = "👁️ Детали";
        detailsButton.Size = new Size(100, 30);
        detailsButton.Location = new Point(770, 10);
        detailsButton.BackColor = Color.FromArgb(111, 66, 193);
        detailsButton.Click += (s, e) => ShowOrderDetails();
        toolbarPanel.Controls.Add(detailsButton);

        // Таблица заказов
        ordersGrid = new DataGridView();
        ordersGrid.Size = new Size(920, 580);
        ordersGrid.Location = new Point(30, 140);
        ordersGrid.BackgroundColor = Color.White;
        ordersGrid.BorderStyle = BorderStyle.None;
        ordersGrid.Font = new Font("Segoe UI", 9);
        ordersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        ordersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        ordersGrid.RowHeadersVisible = false;
        ordersGrid.AllowUserToAddRows = false;
        ordersGrid.ReadOnly = true;

        // Стилизация таблицы
        ordersGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        ordersGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        ordersGrid.EnableHeadersVisualStyles = false;
        ordersGrid.RowTemplate.Height = 40;

        this.Controls.Add(ordersGrid);
    }

    private void LoadOrdersData()
    {
        ordersGrid.Columns.Clear();

        // Создаем колонки
        ordersGrid.Columns.Add("Id", "ID");
        ordersGrid.Columns.Add("Date", "Дата");
        ordersGrid.Columns.Add("Customer", "Клиент");
        ordersGrid.Columns.Add("Amount", "Сумма");
        ordersGrid.Columns.Add("Status", "Статус");
        ordersGrid.Columns.Add("Products", "Товары");

        // Настройка ширины колонок
        ordersGrid.Columns["Id"].Width = 60;
        ordersGrid.Columns["Date"].Width = 100;
        ordersGrid.Columns["Amount"].Width = 100;

        // Пример данных
        AddOrderRow("001", "25.05.2024", "ООО 'Вектор'", "15 240 ₽", "Выполнен", "Ноутбук (2), Мышь (1)");
        AddOrderRow("002", "25.05.2024", "ИП Петров А.В.", "8 500 ₽", "В обработке", "Монитор (1)");
        AddOrderRow("003", "24.05.2024", "ООО 'Альфа'", "21 100 ₽", "Доставка", "Стол офисный (1), Кресло (2)");
        AddOrderRow("004", "24.05.2024", "ИП Сидорова М.К.", "5 300 ₽", "Новый", "Клавиатура (3)");
    }

    private void AddOrderRow(string id, string date, string customer, string amount, string status, string products)
    {
        int rowIndex = ordersGrid.Rows.Add(id, date, customer, amount, status, products);

        // Раскрашиваем статусы
        var statusCell = ordersGrid.Rows[rowIndex].Cells["Status"];
        statusCell.Style.ForeColor = GetStatusColor(status);
        statusCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
    }

    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Новый" => Color.FromArgb(0, 123, 255),
            "В обработке" => Color.FromArgb(255, 193, 7),
            "Выполнен" => Color.FromArgb(40, 167, 69),
            "Доставка" => Color.FromArgb(111, 66, 193),
            "Отменен" => Color.FromArgb(220, 53, 69),
            _ => Color.Black
        };
    }

    private void SearchTextBox_TextChanged(object sender, EventArgs e)
    {
        // Фильтрация данных
        foreach (DataGridViewRow row in ordersGrid.Rows)
        {
            bool visible = string.IsNullOrEmpty(searchTextBox.Text) ||
                          row.Cells["Customer"].Value.ToString().Contains(searchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                          row.Cells["Id"].Value.ToString().Contains(searchTextBox.Text, StringComparison.OrdinalIgnoreCase);

            row.Visible = visible;
        }
    }

    private void StatusFilterCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedStatus = statusFilterCombo.SelectedItem.ToString();

        foreach (DataGridViewRow row in ordersGrid.Rows)
        {
            if (selectedStatus == "Все статусы")
            {
                row.Visible = true;
            }
            else
            {
                row.Visible = row.Cells["Status"].Value.ToString() == selectedStatus;
            }
        }
    }

    private void CreateNewOrder()
    {
        var createForm = new CreateOrderForm();
        if (createForm.ShowDialog() == DialogResult.OK)
        {
            // Обновляем данные
            LoadOrdersData();
            ShowSuccess("Новый заказ успешно создан!");
        }
    }

    private void ShowOrderDetails()
    {
        if (ordersGrid.SelectedRows.Count > 0)
        {
            var selectedRow = ordersGrid.SelectedRows[0];
            var orderId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            // Используем заглушку, так как OrderDetailsForm требует connectionString
            ShowWarning("Функция просмотра деталей заказа в разработке");

            // Раскомментируйте когда будет готова OrderDetailsForm
            // var detailsForm = new OrderDetailsForm("Your_Connection_String", orderId);
            // detailsForm.ShowDialog();
            // LoadOrdersData();
        }
        else
        {
            ShowWarning("Выберите заказ для просмотра деталей");
        }
    }

    private void EditOrder()
    {
        if (ordersGrid.SelectedRows.Count > 0)
        {
            var orderId = ordersGrid.SelectedRows[0].Cells["Id"].Value.ToString();
            var editForm = new CreateOrderForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadOrdersData();
            }
        }
        else
        {
            ShowWarning("Выберите заказ для редактирования");
        }
    }

    private void DeleteOrder()
    {
        if (ordersGrid.SelectedRows.Count > 0)
        {
            var orderId = ordersGrid.SelectedRows[0].Cells["Id"].Value.ToString();
            var customer = ordersGrid.SelectedRows[0].Cells["Customer"].Value.ToString();

            var result = ShowQuestion($"Вы уверены, что хотите удалить заказ #{orderId} от {customer}?");

            if (result == DialogResult.Yes)
            {
                // Логика удаления
                ordersGrid.Rows.Remove(ordersGrid.SelectedRows[0]);
                ShowSuccess("Заказ успешно удален");
            }
        }
        else
        {
            ShowWarning("Выберите заказ для удаления");
        }
    }
}