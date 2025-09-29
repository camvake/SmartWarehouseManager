using SWM.Core.Models;
using SWM.Views.Forms;
using SWM.Views.Forms.Notifications;
using SWM.Views.Forms.Orders;
using SWM.Views.Forms.Product;
using SWM.Views.Forms.Reports;
using SWM.Views.Forms.Supplies;
using SWM.Views.Forms.Users;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SWM.Views
{
    public class MainForm : Form
    {
        private string _connectionString;
        private User _currentUser;
        private ToolStripStatusLabel _lblUserInfo;

        public MainForm(string connectionString, User currentUser)
        {
            _connectionString = connectionString;
            _currentUser = currentUser;

            // ВАЖНО: Устанавливаем это свойство ДО InitializeComponent()
            this.IsMdiContainer = true;

            InitializeComponent();
            SetupUI();
            ApplyUserPermissions();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = $"Smart Warehouse Manager - {_currentUser.FullName} ({_currentUser.RoleDisplay})";
            this.ClientSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true; // ДОБАВЬ ЭТУ СТРОКУ

            // Создаем статус бар
            var statusStrip = new StatusStrip();
            _lblUserInfo = new ToolStripStatusLabel()
            {
                Text = $"Пользователь: {_currentUser.FullName} | Роль: {_currentUser.RoleDisplay}",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTime = new ToolStripStatusLabel()
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                TextAlign = ContentAlignment.MiddleRight
            };

            statusStrip.Items.AddRange(new ToolStripItem[] { _lblUserInfo, lblTime });
            this.Controls.Add(statusStrip);

            var timer = new System.Windows.Forms.Timer() { Interval = 60000 };
            timer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            timer.Start();

            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            int xPos = 20;
            int yPos = 60; // Увеличиваем отступ сверху для статусбара
            int buttonWidth = 150;
            int buttonHeight = 40;
            int spacing = 10;

            // Кнопка товаров
            var btnProducts = CreateMenuButton("📦 Товары", xPos, yPos, buttonWidth, buttonHeight);
            btnProducts.Click += (s, e) => OpenProductsForm();
            this.Controls.Add(btnProducts);
            xPos += buttonWidth + spacing;

            // Кнопка заказов
            var btnOrders = CreateMenuButton("📋 Заказы", xPos, yPos, buttonWidth, buttonHeight);
            btnOrders.Click += (s, e) => OpenOrdersForm();
            this.Controls.Add(btnOrders);
            xPos += buttonWidth + spacing;

            // Кнопка поставок
            var btnSupplies = CreateMenuButton("🚚 Поставки", xPos, yPos, buttonWidth, buttonHeight);
            btnSupplies.Click += (s, e) => OpenSuppliesForm();
            this.Controls.Add(btnSupplies);
            xPos += buttonWidth + spacing;

            // Кнопка инвентаризации (ДОБАВЬ ЭТУ КНОПКУ)
            var btnInventory = CreateMenuButton("📊 Инвентаризация", xPos, yPos, buttonWidth, buttonHeight);
            btnInventory.Click += (s, e) => OpenInventoryForm();
            this.Controls.Add(btnInventory);

            // Следующий ряд
            xPos = 20;
            yPos += buttonHeight + spacing;

            // Кнопка отчетов
            var btnReports = CreateMenuButton("📈 Отчеты", xPos, yPos, buttonWidth, buttonHeight);
            btnReports.Click += (s, e) => OpenReportsForm();
            this.Controls.Add(btnReports);
            xPos += buttonWidth + spacing;

            // Кнопка аналитики
            var btnAnalytics = CreateMenuButton("📊 Аналитика", xPos, yPos, buttonWidth, buttonHeight);
            btnAnalytics.Click += (s, e) => OpenAdvancedReportsForm();
            this.Controls.Add(btnAnalytics);
            xPos += buttonWidth + spacing;

            // Кнопка уведомлений
            var btnNotifications = CreateMenuButton("🔔 Уведомления", xPos, yPos, buttonWidth, buttonHeight);
            btnNotifications.Click += (s, e) => OpenNotificationsForm();
            this.Controls.Add(btnNotifications);
            xPos += buttonWidth + spacing;

            // Кнопка пользователей (только для админов)
            if (_currentUser.Role == UserRole.Admin)
            {
                var btnUsers = CreateMenuButton("👥 Пользователи", xPos, yPos, buttonWidth, buttonHeight);
                btnUsers.Click += (s, e) => OpenUsersForm();
                this.Controls.Add(btnUsers);
            }

            // Кнопка выхода
            var btnLogout = new Button()
            {
                Text = "🚪 Выход",
                Location = new Point(1000, 60),
                Size = new Size(100, 30),
                BackColor = Color.LightCoral
            };
            btnLogout.Click += (s, e) =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    this.Close();
            };
            this.Controls.Add(btnLogout);
        }

        private Button CreateMenuButton(string text, int x, int y, int width, int height)
        {
            return new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Arial", 10),
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void ApplyUserPermissions()
        {
            switch (_currentUser.Role)
            {
                case UserRole.WarehouseWorker:
                    DisableButton("👥 Пользователи");
                    DisableButton("📊 Аналитика");
                    break;

                case UserRole.Viewer:
                    DisableButton("📦 Товары");
                    DisableButton("📋 Заказы");
                    DisableButton("🚚 Поставки");
                    DisableButton("📊 Инвентаризация");
                    DisableButton("👥 Пользователи");
                    break;

                case UserRole.Manager:
                    DisableButton("👥 Пользователи");
                    break;

                case UserRole.Admin:
                    // Админ - полный доступ
                    break;
            }
        }

        private void DisableButton(string buttonText)
        {
            foreach (Control control in this.Controls)
            {
                if (control is Button button && button.Text == buttonText)
                {
                    button.Enabled = false;
                    button.BackColor = Color.LightGray;
                    button.Text = button.Text + " 🔒";
                    break;
                }
            }
        }

        // Методы открытия форм (MDI)
        private void OpenProductsForm()
        {
            try
            {
                var productsForm = new ProductsForm(_connectionString);
                productsForm.MdiParent = this;
                productsForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы товаров: {ex.Message}");
            }
        }

        private void OpenOrdersForm()
        {
            try
            {
                var ordersForm = new OrdersForm(_connectionString);
                ordersForm.MdiParent = this;
                ordersForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы заказов: {ex.Message}");
            }
        }

        private void OpenSuppliesForm()
        {
            try
            {
                var suppliesForm = new SuppliesForm(_connectionString);
                suppliesForm.MdiParent = this;
                suppliesForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы поставок: {ex.Message}");
            }
        }

        private void OpenReportsForm()
        {
            try
            {
                var reportsForm = new DashboardForm(_connectionString);
                reportsForm.MdiParent = this;
                reportsForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы отчетов: {ex.Message}");
            }
        }

        private void OpenAdvancedReportsForm()
        {
            try
            {
                var reportsForm = new AdvancedReportsForm(_connectionString);
                reportsForm.MdiParent = this;
                reportsForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы аналитики: {ex.Message}");
            }
        }

        private void OpenNotificationsForm()
        {
            try
            {
                // Временно закомментируй, если форма не готова
                MessageBox.Show("Форма уведомлений в разработке", "Информация");
                // var notificationsForm = new NotificationsForm(_connectionString);
                // notificationsForm.MdiParent = this;
                // notificationsForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы уведомлений: {ex.Message}");
            }
        }

        private void OpenUsersForm()
        {
            try
            {
                var usersForm = new UsersForm(_connectionString);
                usersForm.MdiParent = this;
                usersForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы пользователей: {ex.Message}");
            }
        }

        // ДОБАВЬ МЕТОД ДЛЯ ИНВЕНТАРИЗАЦИИ
        private void OpenInventoryForm()
        {
            try
            {
                var inventoryForm = new InventoryForm(_connectionString);
                inventoryForm.MdiParent = this;
                inventoryForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы инвентаризации: {ex.Message}");
            }
        }
    }
}