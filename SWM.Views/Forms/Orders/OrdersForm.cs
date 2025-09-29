using System.Windows.Forms;
using SWM.ViewModels;
using System;
using System.Drawing;
using SWM.Core.Models;

namespace SWM.Views.Forms.Orders
{
    public class OrdersForm : Form
    {
        private OrderViewModel _viewModel;
        private DataGridView dataGridViewOrders;
        private Button btnCreateOrder;
        private Button btnViewDetails;

        public OrdersForm(string connectionString)
        {
            _viewModel = new OrderViewModel(connectionString);
            InitializeComponent();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // DataGridView
            this.dataGridViewOrders = new DataGridView();
            this.dataGridViewOrders.Dock = DockStyle.Fill;
            this.dataGridViewOrders.Location = new Point(0, 50);
            this.dataGridViewOrders.Size = new Size(1000, 500);
            this.dataGridViewOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewOrders.ReadOnly = true;

            // Кнопки
            this.btnCreateOrder = new Button();
            this.btnCreateOrder.Text = "Создать заказ";
            this.btnCreateOrder.Location = new Point(12, 12);
            this.btnCreateOrder.Size = new Size(120, 30);
            this.btnCreateOrder.Click += BtnCreateOrder_Click;

            this.btnViewDetails = new Button();
            this.btnViewDetails.Text = "Детали заказа";
            this.btnViewDetails.Location = new Point(140, 12);
            this.btnViewDetails.Size = new Size(120, 30);
            this.btnViewDetails.Click += BtnViewDetails_Click;

            // Панель с кнопками
            var panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = 50;
            panel.Controls.AddRange(new Control[] { btnCreateOrder, btnViewDetails });

            // Форма
            this.Text = "Управление заказами";
            this.ClientSize = new Size(1000, 550);
            this.Controls.Add(dataGridViewOrders);
            this.Controls.Add(panel);

            this.ResumeLayout(false);
        }

        private void LoadOrders()
        {
            dataGridViewOrders.AutoGenerateColumns = false;
            dataGridViewOrders.Columns.Clear();

            // Настраиваем колонки
            dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "OrderNumber",
                HeaderText = "Номер заказа",
                Width = 120
            });

            dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "OrderDate",
                HeaderText = "Дата",
                Width = 100
            });

            dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "CustomerName",
                HeaderText = "Клиент",
                Width = 150
            });

            dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TotalAmount",
                HeaderText = "Сумма",
                Width = 80
            });

            dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "StatusName",
                HeaderText = "Статус",
                Width = 100
            });

            dataGridViewOrders.DataSource = _viewModel.Orders;
        }

        private void BtnCreateOrder_Click(object sender, EventArgs e)
        {
            ShowCreateOrderForm();
        }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrders.SelectedRows.Count > 0)
            {
                var order = dataGridViewOrders.SelectedRows[0].DataBoundItem as Order;
                if (order != null)
                {
                    ShowOrderDetailsForm(order);
                }
            }
        }

        private void ShowCreateOrderForm()
        {
            using (var createOrderForm = new CreateOrderForm(_viewModel))
            {
                if (createOrderForm.ShowDialog() == DialogResult.OK)
                {
                    LoadOrders(); // Обновляем список после создания
                }
            }
        }

        private void ShowOrderDetailsForm(Order order)
        {
            using (var detailsForm = new OrderDetailsForm(order, _viewModel))
            {
                detailsForm.ShowDialog();
                LoadOrders(); // Обновляем список после изменений
            }
        }
    }

}