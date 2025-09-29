using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SWM.Core.Models;
using SWM.ViewModels;

namespace SWM.Views.Forms.Orders
{
    public class OrderDetailsForm : Form
    {
        private Order _order;
        private OrderViewModel _viewModel;

        private DataGridView gridOrderItems;
        private ComboBox cmbStatus;
        private Button btnUpdateStatus, btnClose;

        public OrderDetailsForm(Order order, OrderViewModel viewModel)
        {
            _order = order;
            _viewModel = viewModel;
            InitializeComponent();
            LoadOrderDetails();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = $"Детали заказа {_order.OrderNumber}";
            this.ClientSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            CreateControls();
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            int yPos = 10;

            // Информация о заказе
            var lblInfo = new Label()
            {
                Text = $"Заказ №{_order.OrderNumber} от {_order.OrderDate:dd.MM.yyyy}",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 30;

            // Клиент и адрес
            var lblCustomer = new Label() { Text = $"Клиент: {_order.CustomerName}", Location = new Point(20, yPos), AutoSize = true };
            yPos += 20;
            var lblAddress = new Label() { Text = $"Адрес: {_order.DeliveryAddress}", Location = new Point(20, yPos), AutoSize = true };
            yPos += 20;
            var lblTotal = new Label() { Text = $"Общая сумма: {_order.TotalAmount:N2} руб", Location = new Point(20, yPos), AutoSize = true };
            yPos += 30;

            // Товары в заказе
            var lblItems = new Label() { Text = "Товары в заказе:", Location = new Point(20, yPos), AutoSize = true };
            yPos += 20;

            gridOrderItems = new DataGridView()
            {
                Location = new Point(20, yPos),
                Size = new Size(550, 150),
                ReadOnly = true
            };
            yPos += 160;

            // Смена статуса
            var lblStatus = new Label() { Text = "Статус заказа:", Location = new Point(20, yPos), AutoSize = true };
            cmbStatus = new ComboBox()
            {
                Location = new Point(120, yPos),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            yPos += 30;

            btnUpdateStatus = new Button()
            {
                Text = "Обновить статус",
                Location = new Point(280, yPos - 30),
                Size = new Size(100, 23)
            };
            btnUpdateStatus.Click += BtnUpdateStatus_Click;

            // Кнопка закрытия
            btnClose = new Button()
            {
                Text = "Закрыть",
                Location = new Point(250, yPos + 10),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblInfo, lblCustomer, lblAddress, lblTotal,
                lblItems, gridOrderItems,
                lblStatus, cmbStatus, btnUpdateStatus,
                btnClose
            });

            SetupOrderItemsGrid();
            LoadStatuses();
        }

        private void SetupOrderItemsGrid()
        {
            gridOrderItems.AutoGenerateColumns = false;
            gridOrderItems.Columns.Clear();

            gridOrderItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductName",
                HeaderText = "Товар",
                Width = 200
            });
            gridOrderItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Quantity",
                HeaderText = "Кол-во",
                Width = 60
            });
            gridOrderItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "UnitPrice",
                HeaderText = "Цена",
                Width = 80
            });
            gridOrderItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TotalPrice",
                HeaderText = "Сумма",
                Width = 80
            });

            // Загружаем товары заказа (пока заглушка)
            gridOrderItems.DataSource = _order.OrderItems.Any() ?
                _order.OrderItems :
                new List<OrderItem> { new OrderItem { ProductName = "Товар 1", Quantity = 1, UnitPrice = 100, TotalPrice = 100 } };
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.AddRange(new object[] {
                new { Text = "Новый", Value = 1 },
                new { Text = "В обработке", Value = 2 },
                new { Text = "Подтвержден", Value = 3 },
                new { Text = "Отправлен", Value = 4 },
                new { Text = "Доставлен", Value = 5 },
                new { Text = "Отменен", Value = 6 }
            });
            cmbStatus.DisplayMember = "Text";
            cmbStatus.ValueMember = "Value";

            // Устанавливаем текущий статус
            var currentStatus = cmbStatus.Items.Cast<dynamic>()
                .FirstOrDefault(item => item.Value == _order.StatusID);
            if (currentStatus != null)
                cmbStatus.SelectedItem = currentStatus;
        }

        private void LoadOrderDetails()
        {
            // Здесь можно загрузить детальную информацию о заказе из БД
        }

        private void BtnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedItem != null)
            {
                int newStatusId = ((dynamic)cmbStatus.SelectedItem).Value;
                _viewModel.UpdateOrderStatus(_order.OrderID, newStatusId);
                MessageBox.Show("Статус заказа обновлен");
                this.Close();
            }
        }
    }
}