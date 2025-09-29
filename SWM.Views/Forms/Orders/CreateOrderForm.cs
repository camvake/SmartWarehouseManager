using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using SWM.ViewModels;
using ProductModel = SWM.Core.Models.Product;
using SWM.Core.Models;

namespace SWM.Views.Forms.Orders
{
    public class CreateOrderForm : Form
    {
        private OrderViewModel _viewModel;
        private Order _currentOrder;

        // Элементы формы
        private TextBox txtOrderNumber, txtCustomerName, txtDeliveryAddress, txtSearchProduct;
        private ComboBox cmbPaymentMethod, cmbStatus;
        private DataGridView gridAvailableProducts, gridOrderItems;
        private NumericUpDown numQuantity;
        private Button btnAddProduct, btnCreateOrder, btnCancel, btnClearSearch;
        private Label lblTotalAmount;

        // Константы для размеров
        private const int FormWidth = 1000;
        private const int FormHeight = 700;
        private const int LeftPanelWidth = 300;
        private const int RightPanelWidth = 680;
        private const int GridHeight = 200;

        public CreateOrderForm(OrderViewModel viewModel)
        {
            _viewModel = viewModel;
            _currentOrder = new Order
            {
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>(),
                StatusID = 1,
                UserID = 1
            };

            InitializeComponent();
            LoadPaymentMethods();
            LoadStatuses();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы
            this.Text = "Создание нового заказа";
            this.ClientSize = new Size(FormWidth, FormHeight);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimumSize = new Size(FormWidth, FormHeight);

            CreateControls();
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Создаем основные панели
            var leftPanel = new Panel()
            {
                Location = new Point(10, 10),
                Size = new Size(LeftPanelWidth, FormHeight - 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            var rightPanel = new Panel()
            {
                Location = new Point(LeftPanelWidth + 20, 10),
                Size = new Size(RightPanelWidth, FormHeight - 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Заполняем левую панель (информация о заказе)
            FillLeftPanel(leftPanel);

            // Заполняем правую панель (товары)
            FillRightPanel(rightPanel);

            // Добавляем панели на форму
            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);
        }

        private void FillLeftPanel(Panel panel)
        {
            int yPos = 20;
            int labelWidth = 120;
            int controlWidth = 150;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "Информация о заказе",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 30;

            // Номер заказа
            var lblOrderNumber = new Label() { Text = "Номер заказа:", Location = new Point(20, yPos), Width = labelWidth };
            txtOrderNumber = new TextBox()
            {
                Text = GenerateOrderNumber(),
                Location = new Point(140, yPos),
                Width = controlWidth,
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            yPos += 35;

            // Клиент
            var lblCustomerName = new Label() { Text = "Клиент:", Location = new Point(20, yPos), Width = labelWidth };
            txtCustomerName = new TextBox() { Location = new Point(140, yPos), Width = controlWidth };
            yPos += 35;

            // Адрес доставки
            var lblDeliveryAddress = new Label() { Text = "Адрес доставки:", Location = new Point(20, yPos), Width = labelWidth };
            txtDeliveryAddress = new TextBox()
            {
                Location = new Point(140, yPos),
                Width = controlWidth,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            yPos += 70;

            // Способ оплаты
            var lblPaymentMethod = new Label() { Text = "Способ оплаты:", Location = new Point(20, yPos), Width = labelWidth };
            cmbPaymentMethod = new ComboBox()
            {
                Location = new Point(140, yPos),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            yPos += 35;

            // Статус
            var lblStatus = new Label() { Text = "Статус заказа:", Location = new Point(20, yPos), Width = labelWidth };
            cmbStatus = new ComboBox()
            {
                Location = new Point(140, yPos),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            yPos += 50;

            // Общая сумма
            var lblTotal = new Label()
            {
                Text = "Общая сумма:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            lblTotalAmount = new Label()
            {
                Text = "0 руб",
                Location = new Point(140, yPos),
                Width = controlWidth,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };
            yPos += 40;

            // Кнопки действий
            btnCreateOrder = new Button()
            {
                Text = "✅ Создать заказ",
                Location = new Point(20, yPos),
                Size = new Size(120, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnCreateOrder.Click += BtnCreateOrder_Click;

            btnCancel = new Button()
            {
                Text = "❌ Отмена",
                Location = new Point(150, yPos),
                Size = new Size(120, 35),
                BackColor = Color.LightCoral
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Добавляем все на левую панель
            panel.Controls.AddRange(new Control[] {
                lblTitle, lblOrderNumber, txtOrderNumber,
                lblCustomerName, txtCustomerName,
                lblDeliveryAddress, txtDeliveryAddress,
                lblPaymentMethod, cmbPaymentMethod,
                lblStatus, cmbStatus,
                lblTotal, lblTotalAmount,
                btnCreateOrder, btnCancel
            });
        }

        private void FillRightPanel(Panel panel)
        {
            int yPos = 20;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "Товары в заказе",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 30;

            // Панель поиска
            var searchPanel = new Panel()
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 30)
            };

            var lblSearch = new Label() { Text = "Поиск:", Location = new Point(0, 5), Width = 50 };
            txtSearchProduct = new TextBox()
            {
                Location = new Point(50, 3),
                Width = 200,
                PlaceholderText = "Введите название или артикул..."
            };
            txtSearchProduct.TextChanged += TxtSearchProduct_TextChanged;

            btnClearSearch = new Button()
            {
                Text = "Очистить",
                Location = new Point(260, 3),
                Size = new Size(60, 23)
            };
            btnClearSearch.Click += (s, e) => {
                txtSearchProduct.Text = "";
                gridAvailableProducts.DataSource = _viewModel.AvailableProducts;
            };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearchProduct, btnClearSearch });
            yPos += 40;

            // Сетка доступных товаров
            var lblAvailableProducts = new Label()
            {
                Text = "Доступные товары:",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            yPos += 20;

            gridAvailableProducts = new DataGridView()
            {
                Location = new Point(20, yPos),
                Size = new Size(640, GridHeight),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            yPos += GridHeight + 10;

            // Панель добавления товара
            var addPanel = new Panel()
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 35)
            };

            var lblQuantity = new Label() { Text = "Количество:", Location = new Point(0, 8), Width = 80 };
            numQuantity = new NumericUpDown()
            {
                Location = new Point(85, 5),
                Width = 60,
                Minimum = 1,
                Maximum = 1000,
                Value = 1
            };

            btnAddProduct = new Button()
            {
                Text = "➕ Добавить в заказ",
                Location = new Point(155, 5),
                Size = new Size(120, 25),
                BackColor = Color.LightBlue
            };
            btnAddProduct.Click += BtnAddProduct_Click;

            addPanel.Controls.AddRange(new Control[] { lblQuantity, numQuantity, btnAddProduct });
            yPos += 45;

            // Сетка товаров в заказе
            var lblOrderItems = new Label()
            {
                Text = "Товары в заказе:",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            yPos += 20;

            gridOrderItems = new DataGridView()
            {
                Location = new Point(20, yPos),
                Size = new Size(640, GridHeight),
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            // Добавляем все на правую панель
            panel.Controls.AddRange(new Control[] {
                lblTitle, searchPanel, lblAvailableProducts, gridAvailableProducts,
                addPanel, lblOrderItems, gridOrderItems
            });

            SetupGrids();
        }

        private void SetupGrids()
        {
            // Настройка сетки доступных товаров
            gridAvailableProducts.AutoGenerateColumns = false;
            gridAvailableProducts.Columns.Clear();

            gridAvailableProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ArticleNumber",
                HeaderText = "Артикул",
                Width = 80
            });
            gridAvailableProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Name",
                HeaderText = "Название товара",
                Width = 200
            });
            gridAvailableProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Price",
                HeaderText = "Цена",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridAvailableProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "StockBalance",
                HeaderText = "В наличии",
                Width = 70
            });
            gridAvailableProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Category",
                HeaderText = "Категория",
                Width = 100
            });

            gridAvailableProducts.DataSource = _viewModel.AvailableProducts;

            // Настройка сетки товаров в заказе
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
                HeaderText = "Цена за ед.",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridOrderItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TotalPrice",
                HeaderText = "Сумма",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            // Кнопка удаления из заказа
            var deleteColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "Действие",
                Text = "🗑️ Удалить",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            gridOrderItems.Columns.Add(deleteColumn);
            gridOrderItems.CellClick += GridOrderItems_CellClick;
        }

        private void LoadPaymentMethods()
        {
            cmbPaymentMethod.Items.AddRange(new object[] {
                new { Text = "💵 Наличные", Value = 1 },
                new { Text = "💳 Банковская карта", Value = 2 },
                new { Text = "📲 Электронный перевод", Value = 3 },
                new { Text = "📋 Наложенный платеж", Value = 4 }
            });
            cmbPaymentMethod.DisplayMember = "Text";
            cmbPaymentMethod.ValueMember = "Value";
            cmbPaymentMethod.SelectedIndex = 0;
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.AddRange(new object[] {
                new { Text = "🆕 Новый", Value = 1 },
                new { Text = "⏳ В обработке", Value = 2 },
                new { Text = "✅ Подтвержден", Value = 3 },
                new { Text = "🚚 В доставке", Value = 4 }
            });
            cmbStatus.DisplayMember = "Text";
            cmbStatus.ValueMember = "Value";
            cmbStatus.SelectedIndex = 0;
        }

        // Остальные методы без изменений (GenerateOrderNumber, TxtSearchProduct_TextChanged, BtnAddProduct_Click, etc.)
        // ... [код методов остается таким же как в предыдущей версии]

        private string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        private void TxtSearchProduct_TextChanged(object sender, EventArgs e)
        {
            var searchText = txtSearchProduct.Text.ToLower();
            var filteredProducts = _viewModel.AvailableProducts
                .Where(p => p.Name.ToLower().Contains(searchText) ||
                           p.ArticleNumber.ToLower().Contains(searchText) ||
                           p.Category.ToLower().Contains(searchText))
                .ToList();

            gridAvailableProducts.DataSource = filteredProducts;
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            if (gridAvailableProducts.SelectedRows.Count > 0)
            {
                var selectedProduct = gridAvailableProducts.SelectedRows[0].DataBoundItem as ProductModel;
                if (selectedProduct != null)
                {
                    int quantity = (int)numQuantity.Value;

                    if (quantity > selectedProduct.StockBalance)
                    {
                        MessageBox.Show($"Недостаточно товара на складе. В наличии: {selectedProduct.StockBalance}");
                        return;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductID = selectedProduct.ProductID,
                        ProductName = selectedProduct.Name,
                        ArticleNumber = selectedProduct.ArticleNumber,
                        Quantity = quantity,
                        UnitPrice = selectedProduct.Price,
                        TotalPrice = selectedProduct.Price * quantity
                    };

                    _currentOrder.OrderItems.Add(orderItem);
                    RefreshOrderItemsGrid();
                    CalculateTotalAmount();

                    // Сбрасываем количество
                    numQuantity.Value = 1;
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для добавления в заказ");
            }
        }

        private void GridOrderItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 4) // Кнопка "Удалить"
            {
                _currentOrder.OrderItems.RemoveAt(e.RowIndex);
                RefreshOrderItemsGrid();
                CalculateTotalAmount();
            }
        }

        private void RefreshOrderItemsGrid()
        {
            gridOrderItems.DataSource = null;
            gridOrderItems.DataSource = _currentOrder.OrderItems.ToList();
        }

        private void CalculateTotalAmount()
        {
            decimal total = _currentOrder.OrderItems.Sum(item => item.TotalPrice);
            _currentOrder.TotalAmount = total;
            lblTotalAmount.Text = $"{total:N2} руб";
        }

        private void BtnCreateOrder_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    _currentOrder.OrderNumber = txtOrderNumber.Text;
                    _currentOrder.OrderContent = GenerateOrderContent();
                    _currentOrder.DeliveryAddress = txtDeliveryAddress.Text;
                    _currentOrder.PaymentMethodID = ((dynamic)cmbPaymentMethod.SelectedItem).Value;
                    _currentOrder.StatusID = ((dynamic)cmbStatus.SelectedItem).Value;

                    _viewModel.CreateOrder(_currentOrder);

                    MessageBox.Show("Заказ успешно создан!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GenerateOrderContent()
        {
            return string.Join(", ", _currentOrder.OrderItems.Select(item =>
                $"{item.ProductName} x{item.Quantity}"));
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Введите имя клиента");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDeliveryAddress.Text))
            {
                MessageBox.Show("Введите адрес доставки");
                return false;
            }

            if (_currentOrder.OrderItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ");
                return false;
            }

            return true;
        }
    }
}