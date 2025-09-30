using System;
using System.Drawing;
using System.Windows.Forms;

public class SupplyDetailsForm : BaseForm
{
    private string _supplyId;
    private string _supplier;

    public SupplyDetailsForm()
    {
        InitializeComponent();
    }

    public SupplyDetailsForm(string supplyId, string supplier)
    {
        _supplyId = supplyId;
        _supplier = supplier;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);
        this.Text = $"Детали поставки #{_supplyId}";

        // Заголовок
        var titleLabel = new Label
        {
            Text = $"Детали поставки #{_supplyId}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(titleLabel);

        int y = 70;

        // Информация о поставке
        var infoLabel = new Label
        {
            Text = "Информация о поставке:",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        this.Controls.Add(infoLabel);
        y += 35;

        AddDetail("Поставщик:", _supplier, ref y);
        AddDetail("Номер поставки:", _supplyId, ref y);
        AddDetail("Статус:", "В пути", ref y);
        AddDetail("Дата заказа:", DateTime.Now.AddDays(-5).ToString("dd.MM.yyyy"), ref y);
        AddDetail("Ожидаемая дата:", DateTime.Now.AddDays(2).ToString("dd.MM.yyyy"), ref y);
        AddDetail("Сумма:", "45 200 ₽", ref y);

        y += 20;

        // Состав поставки
        var itemsLabel = new Label
        {
            Text = "Состав поставки:",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        this.Controls.Add(itemsLabel);
        y += 35;

        // Таблица товаров
        var itemsGrid = new DataGridView();
        itemsGrid.Location = new Point(20, y);
        itemsGrid.Size = new Size(540, 200);
        itemsGrid.BackgroundColor = Color.White;
        itemsGrid.BorderStyle = BorderStyle.None;
        itemsGrid.RowHeadersVisible = false;
        itemsGrid.AllowUserToAddRows = false;
        itemsGrid.ReadOnly = true;

        itemsGrid.Columns.Add("Product", "Товар");
        itemsGrid.Columns.Add("Quantity", "Кол-во");
        itemsGrid.Columns.Add("Price", "Цена");
        itemsGrid.Columns.Add("Total", "Сумма");

        itemsGrid.Columns["Quantity"].Width = 80;
        itemsGrid.Columns["Price"].Width = 100;
        itemsGrid.Columns["Total"].Width = 100;

        // Пример данных
        itemsGrid.Rows.Add("Ноутбук Dell XPS 13", "2", "89 990 ₽", "179 980 ₽");
        itemsGrid.Rows.Add("Монитор Samsung 27\"", "3", "24 990 ₽", "74 970 ₽");
        itemsGrid.Rows.Add("Клавиатура механическая", "5", "5 490 ₽", "27 450 ₽");

        this.Controls.Add(itemsGrid);
        y += 220;

        // Кнопка закрытия
        var closeButton = new ModernButton
        {
            Text = "Закрыть",
            Location = new Point(240, y),
            Size = new Size(100, 35)
        };
        closeButton.Click += (s, e) => this.Close();

        this.Controls.Add(closeButton);
    }

    private void AddDetail(string label, string value, ref int y)
    {
        var labelControl = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };

        var valueControl = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 9),
            Location = new Point(150, y),
            AutoSize = true
        };

        this.Controls.Add(labelControl);
        this.Controls.Add(valueControl);
        y += 25;
    }
}