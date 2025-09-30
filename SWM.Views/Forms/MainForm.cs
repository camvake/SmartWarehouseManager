using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SWM.Core.Services;
using SWM.Views.Forms.Orders;
using SWM.Views.Forms.Product;
using SWM.Views.Forms.Reports;
using SWM.Views.Forms.Supplies;
using SWM.Views.Forms.Users;
using SWM.Views.Forms;

public class MainForm : Form
{
    private Panel sidebarPanel;
    private Panel headerPanel;
    private Panel workspacePanel;
    private Label appTitleLabel;
    private Label userNameLabel;
    private Dictionary<string, Form> openForms = new Dictionary<string, Form>();
    private Button currentActiveButton;

    // Цветовая схема
    private readonly Color primaryColor = Color.FromArgb(0, 122, 204);
    private readonly Color sidebarColor = Color.FromArgb(45, 45, 48);
    private readonly Color headerColor = Color.White;
    private readonly Color workspaceColor = Color.FromArgb(250, 250, 250);
    private readonly Color hoverColor = Color.FromArgb(62, 62, 64);
    private readonly Color activeColor = Color.FromArgb(0, 122, 204);

    public MainForm()
    {
        InitializeForm();
        InitializeHeader();
        InitializeSidebar();
        InitializeWorkspace();
        LoadDashboard();
    }

    private void InitializeForm()
    {
        this.Text = "Business Management System";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = workspaceColor;
        this.Padding = new Padding(0);
        this.DoubleBuffered = true;
    }

    private void InitializeHeader()
    {
        headerPanel = new Panel();
        headerPanel.Size = new Size(1200, 60);
        headerPanel.Location = new Point(0, 0);
        headerPanel.BackColor = headerColor;
        headerPanel.Paint += HeaderPanel_Paint;

        // Заголовок приложения
        appTitleLabel = new Label();
        appTitleLabel.Text = "Business Management System";
        appTitleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        appTitleLabel.ForeColor = primaryColor;
        appTitleLabel.Location = new Point(20, 15);
        appTitleLabel.AutoSize = true;

        // Информация пользователя
        userNameLabel = new Label();
        userNameLabel.Text = "Администратор";
        userNameLabel.Font = new Font("Segoe UI", 9);
        userNameLabel.ForeColor = Color.FromArgb(100, 100, 100);
        userNameLabel.Location = new Point(1000, 20);
        userNameLabel.AutoSize = true;

        // Кнопка закрытия
        var closeButton = CreateHeaderButton("X", Color.FromArgb(255, 80, 80));
        closeButton.Location = new Point(1150, 15);
        closeButton.Click += (s, e) => Application.Exit();

        // Кнопка свернуть
        var minimizeButton = CreateHeaderButton("_", Color.FromArgb(100, 100, 100));
        minimizeButton.Location = new Point(1110, 15);
        minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

        headerPanel.Controls.AddRange(new Control[] {
            appTitleLabel, userNameLabel, closeButton, minimizeButton
        });
        this.Controls.Add(headerPanel);
    }

    private void HeaderPanel_Paint(object sender, PaintEventArgs e)
    {
        // Рисуем нижнюю границу
        using (var pen = new Pen(Color.FromArgb(240, 240, 240), 1))
        {
            e.Graphics.DrawLine(pen, 0, 59, headerPanel.Width, 59);
        }
    }

    private Button CreateHeaderButton(string text, Color backColor)
    {
        var button = new Button();
        button.Text = text;
        button.Size = new Size(30, 30);
        button.BackColor = backColor;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        button.Cursor = Cursors.Hand;

        // Закругленные углы
        button.Paint += (s, e) =>
        {
            using (var path = GetRoundedPath(new Rectangle(0, 0, button.Width - 1, button.Height - 1), 5))
            using (var brush = new SolidBrush(button.BackColor))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(e.Graphics, button.Text, button.Font,
                new Rectangle(0, 0, button.Width, button.Height),
                button.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        return button;
    }

    private void InitializeSidebar()
    {
        sidebarPanel = new Panel();
        sidebarPanel.Size = new Size(220, 740);
        sidebarPanel.Location = new Point(0, 60);
        sidebarPanel.BackColor = sidebarColor;

        CreateMenuItems();
        this.Controls.Add(sidebarPanel);
    }

    private void CreateMenuItems()
    {
        var menuItems = new[]
        {
            new { Text = "📊 Дашборд", FormName = "Dashboard" },
            new { Text = "📦 Заказы", FormName = "Orders" },
            new { Text = "📁 Товары", FormName = "Products" },
            new { Text = "🚚 Поставки", FormName = "Supply" },
            new { Text = "📈 Отчеты", FormName = "Reports" },
            new { Text = "👥 Пользователи", FormName = "Users" },
            new { Text = "📦 Инвентарь", FormName = "Inventory" },
            new { Text = "🔔 Уведомления", FormName = "Notifications" }
        };

        int y = 20;
        foreach (var item in menuItems)
        {
            var menuButton = CreateMenuButton(item.Text, item.FormName, y);
            sidebarPanel.Controls.Add(menuButton);
            y += 45;
        }
    }

    private Button CreateMenuButton(string text, string formName, int y)
    {
        var button = new Button();
        button.Text = text;
        button.Tag = formName;
        button.Size = new Size(220, 40);
        button.Location = new Point(0, y);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.Transparent;
        button.ForeColor = Color.White;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(20, 0, 0, 0);
        button.Font = new Font("Segoe UI", 10);
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleLeft;

        // События
        button.Click += (s, e) =>
        {
            SetActiveButton(button);
            OpenForm(formName);
        };

        button.MouseEnter += (s, e) =>
        {
            if (button != currentActiveButton)
            {
                button.BackColor = hoverColor;
            }
        };

        button.MouseLeave += (s, e) =>
        {
            if (button != currentActiveButton)
            {
                button.BackColor = Color.Transparent;
            }
        };

        return button;
    }

    private void SetActiveButton(Button button)
    {
        // Сбрасываем предыдущую активную кнопку
        if (currentActiveButton != null)
        {
            currentActiveButton.BackColor = Color.Transparent;
            currentActiveButton.Font = new Font("Segoe UI", 10);
        }

        // Устанавливаем новую активную кнопку
        currentActiveButton = button;
        currentActiveButton.BackColor = activeColor;
        currentActiveButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
    }

    private void InitializeWorkspace()
    {
        workspacePanel = new Panel();
        workspacePanel.Size = new Size(980, 740);
        workspacePanel.Location = new Point(220, 60);
        workspacePanel.BackColor = workspaceColor;
        this.Controls.Add(workspacePanel);
    }

    private void OpenForm(string formName)
{
    foreach (var form in openForms.Values)
    {
        form.Hide();
    }

    if (!openForms.ContainsKey(formName))
    {
        string connectionString = "Your_Connection_String_Here"; // Получите из конфига
        
        Form newForm = formName switch
        {
            "Dashboard" => new DashboardForm(),
            "Orders" => new OrdersForm(connectionString), // Передаем connection string
            "Products" => new ProductsForm(connectionString), // Передаем connection string
            "Supply" => new SuppliesForm(connectionString),
            "Reports" => new ReportsForm(connectionString),
            "Users" => new UsersForm(connectionString),
            "Inventory" => new InventoryForm(connectionString),
            "Notifications" => new NotificationsForm(),
            _ => new DashboardForm()
        };

        ConfigureForm(newForm);
        workspacePanel.Controls.Add(newForm);
        openForms[formName] = newForm;
    }

    openForms[formName].Show();
    openForms[formName].BringToFront();
}

    // Методы для создания форм (заглушки)
    private Form CreateDashboardForm() => new DashboardForm();
    private Form CreateOrdersForm() => new OrdersForm();
    private Form CreateProductsForm() => new ProductsForm();
    private Form CreateSuppliesForm() => new SuppliesForm();
    private Form CreateReportsForm() => new ReportsForm();
    private Form CreateUsersForm() => new UsersForm();
    private Form CreateInventoryForm() => new InventoryForm();
    private Form CreateNotificationsForm() => new NotificationsForm();

    private void ConfigureForm(Form form)
    {
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        form.Visible = false;
    }

    private void LoadDashboard()
    {
        // Активируем первую кнопку меню
        if (sidebarPanel.Controls.Count > 0 && sidebarPanel.Controls[0] is Button firstButton)
        {
            SetActiveButton(firstButton);
            OpenForm("Dashboard");
        }
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }

    // Добавляем возможность перемещения формы
    private bool dragging = false;
    private Point dragCursorPoint;
    private Point dragFormPoint;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && e.Y <= 60) // Только в области хедера
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (dragging)
        {
            Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            this.Location = Point.Add(dragFormPoint, new Size(dif));
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        dragging = false;
        base.OnMouseUp(e);
    }
}