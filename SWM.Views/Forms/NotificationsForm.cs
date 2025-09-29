using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SWM.Views.Forms
{
    public class NotificationsForm : Form
    {
        private ListView _listViewNotifications;

        public NotificationsForm(string connectionString)
        {
            InitializeComponent();
            LoadSampleNotifications();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "🔔 Уведомления системы";
            this.ClientSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "Системные уведомления",
                Location = new Point(20, 20),
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true
            };

            // Список уведомлений
            _listViewNotifications = new ListView()
            {
                Location = new Point(20, 60),
                Size = new Size(750, 380),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            _listViewNotifications.Columns.Add("Дата", 150);
            _listViewNotifications.Columns.Add("Тип", 100);
            _listViewNotifications.Columns.Add("Сообщение", 500);

            // Кнопки
            var btnMarkAsRead = new Button()
            {
                Text = "Отметить как прочитанное",
                Location = new Point(20, 450),
                Size = new Size(180, 30)
            };
            btnMarkAsRead.Click += (s, e) => MarkAsRead();

            var btnClear = new Button()
            {
                Text = "Очистить все",
                Location = new Point(210, 450),
                Size = new Size(100, 30)
            };
            btnClear.Click += (s, e) => ClearNotifications();

            this.Controls.AddRange(new Control[] {
                lblTitle, _listViewNotifications, btnMarkAsRead, btnClear
            });

            this.ResumeLayout(false);
        }

        private void LoadSampleNotifications()
        {
            // Временные данные - замени на реальные из БД
            var notifications = new[]
            {
                new { Date = DateTime.Now.AddHours(-1), Type = "⚠️ Предупреждение", Message = "Заканчивается товар: Ноутбук Dell XPS 13" },
                new { Date = DateTime.Now.AddHours(-3), Type = "ℹ️ Информация", Message = "Новая поставка получена" },
                new { Date = DateTime.Now.AddDays(-1), Type = "📊 Отчет", Message = "Сгенерирован еженедельный отчет" },
                new { Date = DateTime.Now.AddDays(-2), Type = "⚠️ Предупреждение", Message = "Низкий остаток: Мышь беспроводная" }
            };

            foreach (var notification in notifications)
            {
                var item = new ListViewItem(notification.Date.ToString("dd.MM.yyyy HH:mm"));
                item.SubItems.Add(notification.Type);
                item.SubItems.Add(notification.Message);

                // Цвет в зависимости от типа
                if (notification.Type.Contains("⚠️")) item.BackColor = Color.LightYellow;
                if (notification.Type.Contains("ℹ️")) item.BackColor = Color.LightBlue;

                _listViewNotifications.Items.Add(item);
            }
        }

        private void MarkAsRead()
        {
            if (_listViewNotifications.SelectedItems.Count > 0)
            {
                _listViewNotifications.SelectedItems[0].Remove();
                MessageBox.Show("Уведомление отмечено как прочитанное");
            }
        }

        private void ClearNotifications()
        {
            var result = MessageBox.Show("Очистить все уведомления?", "Очистка",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _listViewNotifications.Items.Clear();
            }
        }
    }
}