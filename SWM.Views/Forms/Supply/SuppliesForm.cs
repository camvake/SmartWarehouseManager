using System;
using System.Drawing;
using System.Windows.Forms;
using SWM.Views.Forms;

public class SuppliesForm : BaseForm
{
    private DataGridView suppliesGrid;
    private ModernButton addButton;
    private ModernButton receiveButton;
    private TextBox searchTextBox;
    private ComboBox statusFilterCombo;

    public SuppliesForm()
    {
        InitializeComponent();
        LoadSuppliesData();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(980, 740);
        this.BackColor = Color.FromArgb(250, 250, 250);

        // Заголовок
        var titleLabel = new Label();
        titleLabel.Text = "Управление поставками";
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
        searchTextBox.PlaceholderText = "Поиск поставок...";
        searchTextBox.BorderStyle = BorderStyle.FixedSingle;
        searchTextBox.TextChanged += SearchTextBox_TextChanged;
        toolbarPanel.Controls.Add(searchTextBox);

        // Фильтр по статусу
        statusFilterCombo = new System.Windows.Forms.ComboBox();
        statusFilterCombo.Size = new Size(150, 30);
        statusFilterCombo.Location = new Point(220, 10);
        statusFilterCombo.Font = new Font("Segoe UI", 9);
        statusFilterCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        statusFilterCombo.Items.AddRange(new object[] { "Все статусы", "Ожидается", "В пути", "Частично получено", "Получено", "Отменено" });
        statusFilterCombo.SelectedIndex = 0;
        statusFilterCombo.SelectedIndexChanged += StatusFilterCombo_SelectedIndexChanged;
        toolbarPanel.Controls.Add(statusFilterCombo);

        // Кнопки
        addButton = new ModernButton();
        addButton.Text = "➕ Новая поставка";
        addButton.Size = new Size(140, 30);
        addButton.Location = new Point(380, 10);
        addButton.Click += (s, e) => CreateNewSupply();
        toolbarPanel.Controls.Add(addButton);

        receiveButton = new ModernButton();
        receiveButton.Text = "📦 Принять";
        receiveButton.Size = new Size(100, 30);
        receiveButton.Location = new Point(530, 10);
        receiveButton.BackColor = Color.FromArgb(40, 167, 69);
        receiveButton.Click += (s, e) => ReceiveSupply();
        toolbarPanel.Controls.Add(receiveButton);

        var detailsButton = new ModernButton();
        detailsButton.Text = "👁️ Детали";
        detailsButton.Size = new Size(100, 30);
        detailsButton.Location = new Point(640, 10);
        detailsButton.BackColor = Color.FromArgb(111, 66, 193);
        detailsButton.Click += (s, e) => ShowSupplyDetails();
        toolbarPanel.Controls.Add(detailsButton);

        // Таблица поставок
        suppliesGrid = new DataGridView();
        suppliesGrid.Size = new Size(920, 580);
        suppliesGrid.Location = new Point(30, 140);
        suppliesGrid.BackgroundColor = Color.White;
        suppliesGrid.BorderStyle = BorderStyle.None;
        suppliesGrid.Font = new Font("Segoe UI", 9);
        suppliesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        suppliesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        suppliesGrid.RowHeadersVisible = false;
        suppliesGrid.AllowUserToAddRows = false;
        suppliesGrid.ReadOnly = true;

        // Стилизация таблицы
        suppliesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        suppliesGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        suppliesGrid.EnableHeadersVisualStyles = false;
        suppliesGrid.RowTemplate.Height = 40;

        this.Controls.Add(suppliesGrid);
    }

    private void LoadSuppliesData()
    {
        suppliesGrid.Columns.Clear();

        // Создаем колонки
        suppliesGrid.Columns.Add("Id", "ID");
        suppliesGrid.Columns.Add("Supplier", "Поставщик");
        suppliesGrid.Columns.Add("OrderDate", "Дата заказа");
        suppliesGrid.Columns.Add("ExpectedDate", "Ожидаемая дата");
        suppliesGrid.Columns.Add("TotalAmount", "Сумма");
        suppliesGrid.Columns.Add("Status", "Статус");
        suppliesGrid.Columns.Add("ItemsCount", "Товаров");

        // Настройка ширины колонок
        suppliesGrid.Columns["Id"].Width = 60;
        suppliesGrid.Columns["OrderDate"].Width = 100;
        suppliesGrid.Columns["ExpectedDate"].Width = 120;
        suppliesGrid.Columns["TotalAmount"].Width = 100;
        suppliesGrid.Columns["ItemsCount"].Width = 80;

        // Пример данных
        AddSupplyRow("001", "ООО 'ТехноПоставка'", "20.05.2024", "25.05.2024", "45 200 ₽", "В пути", "8");
        AddSupplyRow("002", "ИП Иванов", "22.05.2024", "28.05.2024", "18 500 ₽", "Ожидается", "5");
        AddSupplyRow("003", "ООО 'Комплектующие+'", "18.05.2024", "23.05.2024", "32 100 ₽", "Получено", "12");
        AddSupplyRow("004", "ЗАО 'Электроник'", "25.05.2024", "30.05.2024", "27 800 ₽", "Частично получено", "6");
    }

    private void AddSupplyRow(string id, string supplier, string orderDate, string expectedDate, string amount, string status, string itemsCount)
    {
        int rowIndex = suppliesGrid.Rows.Add(id, supplier, orderDate, expectedDate, amount, status, itemsCount);

        // Раскрашиваем статусы
        var statusCell = suppliesGrid.Rows[rowIndex].Cells["Status"];
        statusCell.Style.ForeColor = GetStatusColor(status);
        statusCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
    }

    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Ожидается" => Color.FromArgb(255, 193, 7),
            "В пути" => Color.FromArgb(0, 123, 255),
            "Частично получено" => Color.FromArgb(111, 66, 193),
            "Получено" => Color.FromArgb(40, 167, 69),
            "Отменено" => Color.FromArgb(220, 53, 69),
            _ => Color.Black
        };
    }

    private void SearchTextBox_TextChanged(object sender, EventArgs e)
    {
        foreach (DataGridViewRow row in suppliesGrid.Rows)
        {
            bool visible = string.IsNullOrEmpty(searchTextBox.Text) ||
                          row.Cells["Supplier"].Value.ToString().Contains(searchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                          row.Cells["Id"].Value.ToString().Contains(searchTextBox.Text, StringComparison.OrdinalIgnoreCase);

            row.Visible = visible;
        }
    }

    private void StatusFilterCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedStatus = statusFilterCombo.SelectedItem.ToString();

        foreach (DataGridViewRow row in suppliesGrid.Rows)
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

    private void CreateNewSupply()
    {
        var createForm = new CreateSupplyForm();
        if (createForm.ShowDialog() == DialogResult.OK)
        {
            LoadSuppliesData();
            ShowSuccess("Новая поставка успешно создана!");
        }
    }

    private void ReceiveSupply()
    {
        if (suppliesGrid.SelectedRows.Count > 0)
        {
            var supplyId = suppliesGrid.SelectedRows[0].Cells["Id"].Value.ToString();
            var supplier = suppliesGrid.SelectedRows[0].Cells["Supplier"].Value.ToString();

            var receiveForm = new ReceiveSupplyForm(supplyId, supplier);
            if (receiveForm.ShowDialog() == DialogResult.OK)
            {
                LoadSuppliesData();
                ShowSuccess($"Поставка #{supplyId} от {supplier} успешно принята!");
            }
        }
        else
        {
            ShowWarning("Выберите поставку для приема");
        }
    }

    private void ShowSupplyDetails()
    {
        if (suppliesGrid.SelectedRows.Count > 0)
        {
            var supplyId = suppliesGrid.SelectedRows[0].Cells["Id"].Value.ToString();
            var supplier = suppliesGrid.SelectedRows[0].Cells["Supplier"].Value.ToString();

            var detailsForm = new SupplyDetailsForm(supplyId, supplier);
            detailsForm.ShowDialog();
            LoadSuppliesData();
        }
        else
        {
            ShowWarning("Выберите поставку для просмотра деталей");
        }
    }
}