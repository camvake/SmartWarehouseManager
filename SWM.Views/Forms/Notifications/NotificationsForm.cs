using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SWM.ViewModels;
using SWM.Core.Models;

namespace SWM.Views.Forms.Notifications
{
    public class NotificationsForm : Form
    {
        private NotificationViewModel _viewModel;
        private DataGridView gridNotifications;
        private Button btnRefresh, btnMarkAllRead, btnCheckAll;
        private Label lblStats;

        public NotificationsForm(string connectionString)
        {
            _viewModel = new NotificationViewModel(connectionString);
            InitializeComponent();
            LoadNotifications();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "🔔 Уведомления системы";
            this.ClientSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            int yPos = 20;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "Система уведомлений",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 40;

            // Статистика
            lblStats = new Label()
            {
                Text = "Загрузка...",
                Location = new Point(20, yPos),
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.Blue
            };
            yPos += 30;

            // Панель кнопок
            var buttonsPanel = new Panel()
            {
                Location = new Point(20, yPos),
                Size = new Size(850, 40)
            };

            btnCheckAll = new Button()
            {
                Text = "🔍 Проверить уведомления",
                Location = new Point(0, 5),
                Size = new Size(180, 30),
                BackColor = Color.LightBlue
            };
            btnCheckAll.Click += BtnCheckAll_Click;

            btnRefresh = new Button()
            {
                Text = "🔄 Обновить",
                Location = new Point(190, 5),
                Size = new Size(100, 30)
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnMarkAllRead = new Button()
            {
                Text = "✅ Прочитать все",
                Location = new Point(300, 5),
                Size = new Size(120, 30),
                BackColor = Color.LightGreen
            };
            btnMarkAllRead.Click += BtnMarkAllRead_Click;

            buttonsPanel.Controls.AddRange(new Control[] {
                btnCheckAll, btnRefresh, btnMarkAllRead
            });
            yPos += 50;

            // Сетка уведомлений
            gridNotifications = new DataGridView()
            {
                Location = new Point(20, yPos),
                Size = new Size(850, 400),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblStats, buttonsPanel, gridNotifications
            });

            SetupNotificationsGrid();
            UpdateStatsDisplay();
        }

        private void SetupNotificationsGrid()
        {
            gridNotifications.AutoGenerateColumns = false;
            gridNotifications.Columns.Clear();

            // Иконка приоритета
            gridNotifications.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "PriorityIcon",
                HeaderText = "",
                Width = 30
            });

            // Иконка типа
            gridNotifications.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TypeIcon",
                HeaderText = "",
                Width = 30
            });

            gridNotifications.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Title",
                HeaderText = "Заголовок",
                Width = 200
            });

            gridNotifications.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Message",
                HeaderText = "Сообщение",
                Width = 300
            });

            gridNotifications.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Дата",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" }
            });

            // Кнопка отметки прочитанным
            var readColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "Действие",
                Text = "✅ Прочитать",
                UseColumnTextForButtonValue = true,
                Width = 100
            };
            gridNotifications.Columns.Add(readColumn);

            gridNotifications.CellClick += GridNotifications_CellClick;
            gridNotifications.CellFormatting += GridNotifications_CellFormatting;
        }

        private void GridNotifications_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 5) // Кнопка "Прочитать"
            {
                var notification = gridNotifications.Rows[e.RowIndex].DataBoundItem as Notification;
                if (notification != null)
                {
                    _viewModel.MarkAsRead(notification.NotificationID);
                    LoadNotifications();
                }
            }
        }

        private void GridNotifications_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridNotifications.Rows[e.RowIndex].DataBoundItem is Notification notification)
            {
                // Раскрашиваем строки по приоритету
                switch (notification.Priority)
                {
                    case NotificationPriority.High:
                        gridNotifications.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                        break;
                    case NotificationPriority.Critical:
                        gridNotifications.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                        break;
                }
            }
        }

        private void LoadNotifications()
        {
            gridNotifications.DataSource = _viewModel.Notifications.ToList();
            UpdateStatsDisplay();
        }

        private void UpdateStatsDisplay()
        {
            if (_viewModel.DashboardStats != null)
            {
                lblStats.Text =
                    $"📊 Статистика: {_viewModel.DashboardStats.PendingOrders} новых заказов | " +
                    $"{_viewModel.DashboardStats.LowStockProducts} товаров с низким запасом | " +
                    $"{_viewModel.DashboardStats.UnreadNotifications} непрочитанных уведомлений";
            }
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            _viewModel.CheckAllNotifications();
            LoadNotifications();
            MessageBox.Show("Проверка уведомлений завершена!", "Уведомления",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadNotifications();
        }

        private void BtnMarkAllRead_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Отметить все уведомления как прочитанные?",
                "Подтверждение", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                _viewModel.MarkAllAsRead();
                LoadNotifications();
            }
        }
    }
}