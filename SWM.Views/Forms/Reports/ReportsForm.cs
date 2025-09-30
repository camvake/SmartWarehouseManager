using System;
using System.Drawing;
using System.Windows.Forms;

public class ReportsForm : BaseForm
{
    private ComboBox reportTypeCombo;
    private DateTimePicker dateFromPicker;
    private DateTimePicker dateToPicker;
    private ModernButton generateButton;
    private ModernButton exportButton;
    private Panel chartPanel;
    private DataGridView reportGrid;

    public ReportsForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(980, 740);
        this.BackColor = Color.FromArgb(250, 250, 250);

        // Заголовок
        var titleLabel = new Label();
        titleLabel.Text = "Отчеты и аналитика";
        titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        titleLabel.ForeColor = Color.FromArgb(60, 60, 60);
        titleLabel.Location = new Point(30, 30);
        titleLabel.AutoSize = true;
        this.Controls.Add(titleLabel);

        // Панель параметров
        var paramsPanel = new Panel();
        paramsPanel.Size = new Size(920, 60);
        paramsPanel.Location = new Point(30, 80);
        paramsPanel.BackColor = Color.White;
        this.Controls.Add(paramsPanel);

        // Тип отчета
        var reportTypeLabel = new Label { Text = "Тип отчета:", Location = new Point(10, 20), AutoSize = true };
        reportTypeCombo = new System.Windows.Forms.ComboBox();
        reportTypeCombo.Size = new Size(200, 25);
        reportTypeCombo.Location = new Point(90, 17);
        reportTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        reportTypeCombo.Items.AddRange(new object[] {
            "Продажи по дням",
            "Топ товаров",
            "Продажи по менеджерам",
            "Обороты по месяцам",
            "Остатки на складе",
            "Заказы по статусам"
        });
        reportTypeCombo.SelectedIndex = 0;
        paramsPanel.Controls.AddRange(new Control[] { reportTypeLabel, reportTypeCombo });

        // Период
        var dateFromLabel = new Label { Text = "С:", Location = new Point(300, 20), AutoSize = true };
        dateFromPicker = new DateTimePicker();
        dateFromPicker.Size = new Size(120, 25);
        dateFromPicker.Location = new Point(320, 17);
        dateFromPicker.Value = DateTime.Now.AddDays(-30);
        paramsPanel.Controls.AddRange(new Control[] { dateFromLabel, dateFromPicker });

        var dateToLabel = new Label { Text = "По:", Location = new Point(450, 20), AutoSize = true };
        dateToPicker = new DateTimePicker();
        dateToPicker.Size = new Size(120, 25);
        dateToPicker.Location = new Point(480, 17);
        dateToPicker.Value = DateTime.Now;
        paramsPanel.Controls.AddRange(new Control[] { dateToLabel, dateToPicker });

        // Кнопки
        generateButton = new ModernButton();
        generateButton.Text = "📊 Сформировать";
        generateButton.Size = new Size(140, 30);
        generateButton.Location = new Point(610, 15);
        generateButton.Click += (s, e) => GenerateReport();
        paramsPanel.Controls.Add(generateButton);

        exportButton = new ModernButton();
        exportButton.Text = "📤 Экспорт";
        exportButton.Size = new Size(100, 30);
        exportButton.Location = new Point(760, 15);
        exportButton.BackColor = Color.FromArgb(40, 167, 69);
        exportButton.Click += (s, e) => ExportReport();
        paramsPanel.Controls.Add(exportButton);

        // Панель для графика
        chartPanel = new Panel();
        chartPanel.Size = new Size(920, 200);
        chartPanel.Location = new Point(30, 150);
        chartPanel.BackColor = Color.White;
        chartPanel.Paint += ChartPanel_Paint;
        this.Controls.Add(chartPanel);

        // Таблица отчета
        reportGrid = new DataGridView();
        reportGrid.Size = new Size(920, 350);
        reportGrid.Location = new Point(30, 360);
        reportGrid.BackgroundColor = Color.White;
        reportGrid.BorderStyle = BorderStyle.None;
        reportGrid.Font = new Font("Segoe UI", 9);
        reportGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        reportGrid.RowHeadersVisible = false;
        reportGrid.AllowUserToAddRows = false;
        reportGrid.ReadOnly = true;

        // Стилизация таблицы
        reportGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        reportGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        reportGrid.EnableHeadersVisualStyles = false;

        this.Controls.Add(reportGrid);
    }

    private void ChartPanel_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Простой график для демонстрации
        var rect = new Rectangle(20, 20, chartPanel.Width - 40, chartPanel.Height - 40);

        // Фон
        using (var brush = new SolidBrush(Color.FromArgb(248, 249, 250)))
            g.FillRectangle(brush, rect);

        // Оси
        using (var pen = new Pen(Color.LightGray, 1))
        {
            g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom); // X ось
            g.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom);    // Y ось
        }

        // Данные графика (заглушка)
        int[] data = { 12000, 18000, 15000, 22000, 19000, 25000, 28000 };
        int pointCount = data.Length;
        float xStep = (float)rect.Width / (pointCount - 1);
        float yMax = data.Max();

        // Линия графика
        using (var pen = new Pen(Color.FromArgb(0, 123, 255), 2))
        {
            for (int i = 0; i < pointCount - 1; i++)
            {
                float x1 = rect.Left + i * xStep;
                float y1 = rect.Bottom - (data[i] / yMax) * rect.Height;
                float x2 = rect.Left + (i + 1) * xStep;
                float y2 = rect.Bottom - (data[i + 1] / yMax) * rect.Height;

                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        // Точки данных
        using (var brush = new SolidBrush(Color.FromArgb(0, 123, 255)))
        {
            for (int i = 0; i < pointCount; i++)
            {
                float x = rect.Left + i * xStep;
                float y = rect.Bottom - (data[i] / yMax) * rect.Height;
                g.FillEllipse(brush, x - 3, y - 3, 6, 6);
            }
        }

        // Заголовок графика
        using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
        using (var brush = new SolidBrush(Color.FromArgb(60, 60, 60)))
        {
            g.DrawString("Динамика продаж за неделю", font, brush, rect.Left, rect.Top - 15);
        }
    }

    private void GenerateReport()
    {
        string reportType = reportTypeCombo.SelectedItem.ToString();
        DateTime fromDate = dateFromPicker.Value;
        DateTime toDate = dateToPicker.Value;

        // Очищаем предыдущие данные
        reportGrid.Columns.Clear();

        // Генерируем данные отчета в зависимости от типа
        switch (reportType)
        {
            case "Продажи по дням":
                GenerateSalesByDayReport(fromDate, toDate);
                break;
            case "Топ товаров":
                GenerateTopProductsReport(fromDate, toDate);
                break;
            case "Продажи по менеджерам":
                GenerateSalesByManagerReport(fromDate, toDate);
                break;
            case "Обороты по месяцам":
                GenerateMonthlyRevenueReport(fromDate, toDate);
                break;
            case "Остатки на складе":
                GenerateStockReport();
                break;
            case "Заказы по статусам":
                GenerateOrdersByStatusReport(fromDate, toDate);
                break;
        }

        ShowSuccess($"Отчет '{reportType}' сформирован за период с {fromDate:dd.MM.yyyy} по {toDate:dd.MM.yyyy}");
    }

    private void GenerateSalesByDayReport(DateTime fromDate, DateTime toDate)
    {
        reportGrid.Columns.Add("Date", "Дата");
        reportGrid.Columns.Add("OrdersCount", "Кол-во заказов");
        reportGrid.Columns.Add("TotalAmount", "Общая сумма");
        reportGrid.Columns.Add("AverageOrder", "Средний чек");

        // Пример данных
        reportGrid.Rows.Add("25.05.2024", "8", "45 200 ₽", "5 650 ₽");
        reportGrid.Rows.Add("24.05.2024", "12", "68 500 ₽", "5 708 ₽");
        reportGrid.Rows.Add("23.05.2024", "6", "32 100 ₽", "5 350 ₽");
        reportGrid.Rows.Add("22.05.2024", "10", "55 800 ₽", "5 580 ₽");
    }

    private void GenerateTopProductsReport(DateTime fromDate, DateTime toDate)
    {
        reportGrid.Columns.Add("Product", "Товар");
        reportGrid.Columns.Add("Category", "Категория");
        reportGrid.Columns.Add("Quantity", "Кол-во продаж");
        reportGrid.Columns.Add("Revenue", "Выручка");
        reportGrid.Columns.Add("AvgPrice", "Средняя цена");

        // Пример данных
        reportGrid.Rows.Add("Ноутбук Dell XPS 13", "Электроника", "15", "1 349 850 ₽", "89 990 ₽");
        reportGrid.Rows.Add("Монитор Samsung 27\"", "Электроника", "22", "549 780 ₽", "24 990 ₽");
        reportGrid.Rows.Add("Офисный стол", "Мебель", "8", "103 920 ₽", "12 990 ₽");
        reportGrid.Rows.Add("Клавиатура механическая", "Аксессуары", "35", "192 150 ₽", "5 490 ₽");
    }

    private void GenerateStockReport()
    {
        reportGrid.Columns.Add("Product", "Товар");
        reportGrid.Columns.Add("Category", "Категория");
        reportGrid.Columns.Add("CurrentStock", "Текущий запас");
        reportGrid.Columns.Add("MinStock", "Мин. запас");
        reportGrid.Columns.Add("Status", "Статус");
        reportGrid.Columns.Add("LastDelivery", "Последняя поставка");

        // Пример данных
        reportGrid.Rows.Add("Ноутбук Dell XPS 13", "Электроника", "5", "3", "Норма", "20.05.2024");
        reportGrid.Rows.Add("Монитор Samsung 27\"", "Электроника", "2", "5", "Низкий запас", "18.05.2024");
        reportGrid.Rows.Add("Компьютерная мышь", "Аксессуары", "0", "10", "Нет в наличии", "15.05.2024");
        reportGrid.Rows.Add("Офисный стол", "Мебель", "8", "2", "Норма", "22.05.2024");
    }

    private void ExportReport()
    {
        if (reportGrid.Rows.Count == 0)
        {
            ShowWarning("Нет данных для экспорта. Сформируйте отчет сначала.");
            return;
        }

        var saveDialog = new SaveFileDialog();
        saveDialog.Filter = "Excel Files|*.xlsx|CSV Files|*.csv|PDF Files|*.pdf";
        saveDialog.Title = "Экспорт отчета";

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            // Логика экспорта
            ShowSuccess($"Отчет успешно экспортирован в файл: {saveDialog.FileName}");
        }
    }

    // Заглушки для остальных методов генерации отчетов
    private void GenerateSalesByManagerReport(DateTime fromDate, DateTime toDate) { }
    private void GenerateMonthlyRevenueReport(DateTime fromDate, DateTime toDate) { }
    private void GenerateOrdersByStatusReport(DateTime fromDate, DateTime toDate) { }
}