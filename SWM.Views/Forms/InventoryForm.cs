using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using SWM.Core.Models;
using SWM.Data.Repositories;

namespace SWM.Views.Forms
{
    public class InventoryForm : Form
    {
        private string _connectionString;
        private InventoryRepository _inventoryRepo;
        private ProductRepository _productRepo;

        private ComboBox cmbWarehouse;
        private DateTimePicker dtpInventoryDate;
        private DataGridView gridInventoryItems;
        private TextBox txtInventoryNumber, txtNotes;
        private Button btnStartInventory, btnAddProduct, btnComplete, btnCancel;
        private Label lblStatus;

        private Inventory _currentInventory;
        private List<InventoryItem> _inventoryItems;
        private List<InventoryItemDisplayDto> _inventoryDisplayItems;

        public InventoryForm(string connectionString)
        {
            _connectionString = connectionString;
            _inventoryRepo = new InventoryRepository(connectionString);
            _productRepo = new ProductRepository(connectionString);

            _currentInventory = new Inventory
            {
                InventoryNumber = GenerateInventoryNumber(),
                InventoryDate = DateTime.Today,
                Status = InventoryStatus.Draft,
                CreatedDate = DateTime.Now
            };

            _inventoryItems = new List<InventoryItem>();
            _inventoryDisplayItems = new List<InventoryItemDisplayDto>();

            InitializeComponent();
            LoadWarehouses();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "📋 Проведение инвентаризации";
            this.ClientSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Заголовок
            var lblTitle = new Label()
            {
                Text = "📦 ИНВЕНТАРИЗАЦИЯ СКЛАДА",
                Location = new Point(20, 15),
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };

            // Панель информации
            var infoPanel = new Panel()
            {
                Location = new Point(20, 50),
                Size = new Size(960, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            // Номер инвентаризации
            var lblNumber = new Label()
            {
                Text = "Номер:",
                Location = new Point(15, 15),
                Width = 80,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            txtInventoryNumber = new TextBox()
            {
                Location = new Point(100, 12),
                Width = 180,
                Text = _currentInventory.InventoryNumber,
                ReadOnly = true,
                BackColor = Color.LightGray,
                Font = new Font("Arial", 9)
            };

            // Склад
            var lblWarehouse = new Label()
            {
                Text = "Склад:",
                Location = new Point(300, 15),
                Width = 50,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            cmbWarehouse = new ComboBox()
            {
                Location = new Point(355, 12),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 9)
            };
            cmbWarehouse.SelectedIndexChanged += CmbWarehouse_SelectedIndexChanged;

            // Дата инвентаризации
            var lblDate = new Label()
            {
                Text = "Дата:",
                Location = new Point(625, 15),
                Width = 40,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            dtpInventoryDate = new DateTimePicker()
            {
                Location = new Point(670, 12),
                Width = 130,
                Font = new Font("Arial", 9),
                Value = DateTime.Today
            };

            // Примечания
            var lblNotes = new Label()
            {
                Text = "Примечания:",
                Location = new Point(15, 55),
                Width = 80,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            txtNotes = new TextBox()
            {
                Location = new Point(100, 52),
                Width = 700,
                Height = 25,
                Font = new Font("Arial", 9)
            };

            infoPanel.Controls.AddRange(new Control[] {
                lblNumber, txtInventoryNumber, lblWarehouse, cmbWarehouse,
                lblDate, dtpInventoryDate, lblNotes, txtNotes
            });

            // Статус
            lblStatus = new Label()
            {
                Text = "🟦 Статус: ЧЕРНОВИК",
                Location = new Point(20, 160),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Blue
            };

            // Заголовок таблицы товаров
            var lblItems = new Label()
            {
                Text = "📋 ТОВАРЫ ДЛЯ ИНВЕНТАРИЗАЦИИ",
                Location = new Point(20, 190),
                Font = new Font("Arial", 11, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.DarkGreen
            };

            // Таблица товаров
            gridInventoryItems = new DataGridView()
            {
                Location = new Point(20, 220),
                Size = new Size(960, 300),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Arial", 9)
            };

            // Панель кнопок
            var buttonsPanel = new Panel()
            {
                Location = new Point(20, 535),
                Size = new Size(960, 50),
                BackColor = Color.WhiteSmoke
            };

            btnStartInventory = new Button()
            {
                Text = "▶️ НАЧАТЬ ИНВЕНТАРИЗАЦИЮ",
                Location = new Point(10, 10),
                Size = new Size(200, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnStartInventory.Click += BtnStartInventory_Click;

            btnAddProduct = new Button()
            {
                Text = "➕ ДОБАВИТЬ ТОВАР",
                Location = new Point(220, 10),
                Size = new Size(150, 35),
                BackColor = Color.LightYellow,
                Font = new Font("Arial", 9),
                FlatStyle = FlatStyle.Flat
            };
            btnAddProduct.Click += BtnAddProduct_Click;

            btnComplete = new Button()
            {
                Text = "✅ ЗАВЕРШИТЬ",
                Location = new Point(380, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnComplete.Click += BtnComplete_Click;

            btnCancel = new Button()
            {
                Text = "❌ ОТМЕНА",
                Location = new Point(510, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 9),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Кнопка обновления
            var btnRefresh = new Button()
            {
                Text = "🔄 ОБНОВИТЬ",
                Location = new Point(640, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightGray,
                Font = new Font("Arial", 9),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadProductsForInventory();

            buttonsPanel.Controls.AddRange(new Control[] {
                btnStartInventory, btnAddProduct, btnComplete, btnCancel, btnRefresh
            });

            // Добавляем все контролы на форму
            this.Controls.AddRange(new Control[] {
                lblTitle, infoPanel, lblStatus, lblItems, gridInventoryItems, buttonsPanel
            });

            SetupInventoryGrid();
        }

        private void LoadWarehouses()
        {
            try
            {
                cmbWarehouse.Items.Clear();

                // Временные данные - замените на реальные из вашего репозитория
                cmbWarehouse.Items.Add(new { Name = "Основной склад", WarehouseID = 1 });
                cmbWarehouse.Items.Add(new { Name = "Резервный склад", WarehouseID = 2 });
                cmbWarehouse.Items.Add(new { Name = "Склад готовой продукции", WarehouseID = 3 });

                cmbWarehouse.DisplayMember = "Name";
                cmbWarehouse.ValueMember = "WarehouseID";

                if (cmbWarehouse.Items.Count > 0)
                    cmbWarehouse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки складов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbWarehouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbWarehouse.SelectedItem != null && _currentInventory.Status == InventoryStatus.Draft)
            {
                LoadProductsForInventory();
            }
        }

        private void LoadProductsForInventory()
        {
            try
            {
                if (cmbWarehouse.SelectedItem == null) return;

                var products = _productRepo.GetAllProducts();
                var warehouseId = ((dynamic)cmbWarehouse.SelectedItem).WarehouseID;

                var warehouseProducts = products.Where(p => p.WarehouseID == warehouseId && p.IsActive).ToList();

                _inventoryDisplayItems.Clear();
                _inventoryItems.Clear();

                foreach (var product in warehouseProducts)
                {
                    // Для отображения в grid
                    _inventoryDisplayItems.Add(new InventoryItemDisplayDto
                    {
                        ProductID = product.ProductID,
                        ProductName = product.Name,
                        ArticleNumber = product.ArticleNumber,
                        ExpectedQuantity = product.StockBalance,
                        ActualQuantity = product.StockBalance,
                        Notes = ""
                    });

                    // Для сохранения в БД
                    _inventoryItems.Add(new InventoryItem
                    {
                        ProductID = product.ProductID,
                        ExpectedQuantity = product.StockBalance,
                        ActualQuantity = product.StockBalance,
                        Notes = ""
                    });
                }

                gridInventoryItems.DataSource = _inventoryDisplayItems.ToList();
                UpdateStatusInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupInventoryGrid()
        {
            gridInventoryItems.AutoGenerateColumns = false;
            gridInventoryItems.Columns.Clear();

            // Товар
            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductName",
                HeaderText = "НАИМЕНОВАНИЕ ТОВАРА",
                Width = 250,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.WhiteSmoke }
            });

            // Артикул
            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ArticleNumber",
                HeaderText = "АРТИКУЛ",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.WhiteSmoke }
            });

            // Ожидается
            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ExpectedQuantity",
                HeaderText = "ОЖИДАЕТСЯ",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.WhiteSmoke,
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "N0"
                }
            });

            // Фактически
            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ActualQuantity",
                HeaderText = "ФАКТИЧЕСКИ",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "N0"
                }
            });

            // Разница
            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Difference",
                HeaderText = "РАЗНИЦА",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "N0"
                }
            });

            // Примечание
            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Notes",
                HeaderText = "ПРИМЕЧАНИЕ",
                Width = 250
            });

            gridInventoryItems.CellValueChanged += GridInventoryItems_CellValueChanged;
            gridInventoryItems.CellFormatting += GridInventoryItems_CellFormatting;
        }

        private void GridInventoryItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 4) // Колонка "Разница"
            {
                if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal difference))
                {
                    if (difference < 0)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(gridInventoryItems.Font, FontStyle.Bold);
                    }
                    else if (difference > 0)
                    {
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(gridInventoryItems.Font, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }

        private void GridInventoryItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 3) // Изменение ActualQuantity
            {
                var row = gridInventoryItems.Rows[e.RowIndex];
                var expected = Convert.ToDecimal(row.Cells[2].Value);
                var actual = Convert.ToDecimal(row.Cells[3].Value);
                row.Cells[4].Value = actual - expected;

                // Обновляем соответствующий элемент в списке для БД
                if (e.RowIndex < _inventoryItems.Count)
                {
                    _inventoryItems[e.RowIndex].ActualQuantity = actual;
                    _inventoryItems[e.RowIndex].Notes = row.Cells[5].Value?.ToString() ?? "";
                }

                // Подсветка строки
                if (actual != expected)
                {
                    row.DefaultCellStyle.BackColor = actual < expected ? Color.LightCoral : Color.LightGreen;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }

                UpdateStatusInfo();
            }
        }

        private void BtnStartInventory_Click(object sender, EventArgs e)
        {
            if (cmbWarehouse.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад для инвентаризации!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_inventoryDisplayItems.Count == 0)
            {
                MessageBox.Show("Нет товаров для инвентаризации на выбранном складе!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentInventory.Status = InventoryStatus.InProgress;
            _currentInventory.InventoryDate = dtpInventoryDate.Value;
            _currentInventory.WarehouseID = ((dynamic)cmbWarehouse.SelectedItem).WarehouseID;
            _currentInventory.Notes = txtNotes.Text;

            UpdateStatusInfo();

            btnStartInventory.Enabled = false;
            btnComplete.Enabled = true;
            cmbWarehouse.Enabled = false;
            dtpInventoryDate.Enabled = false;
            txtNotes.Enabled = false;

            MessageBox.Show("Инвентаризация начата! Теперь можно вносить фактические данные.", "Инвентаризация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            if (_currentInventory.Status != InventoryStatus.InProgress)
            {
                MessageBox.Show("Сначала начните инвентаризацию!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new Form())
            {
                form.Text = "➕ ДОБАВИТЬ ТОВАР В ИНВЕНТАРИЗАЦИЮ";
                form.Size = new Size(450, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var lblProduct = new Label()
                {
                    Text = "Товар:",
                    Location = new Point(20, 25),
                    Width = 80,
                    Font = new Font("Arial", 9, FontStyle.Bold)
                };
                var cmbProduct = new ComboBox()
                {
                    Location = new Point(100, 22),
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Arial", 9)
                };

                var lblQuantity = new Label()
                {
                    Text = "Количество:",
                    Location = new Point(20, 65),
                    Width = 80,
                    Font = new Font("Arial", 9, FontStyle.Bold)
                };
                var numQuantity = new NumericUpDown()
                {
                    Location = new Point(100, 62),
                    Width = 120,
                    Minimum = 0,
                    Maximum = 100000,
                    DecimalPlaces = 0,
                    Font = new Font("Arial", 9)
                };

                var btnOk = new Button()
                {
                    Text = "ДОБАВИТЬ",
                    Location = new Point(100, 110),
                    Size = new Size(100, 30),
                    DialogResult = DialogResult.OK,
                    BackColor = Color.LightGreen,
                    Font = new Font("Arial", 9, FontStyle.Bold)
                };
                var btnCancel = new Button()
                {
                    Text = "ОТМЕНА",
                    Location = new Point(210, 110),
                    Size = new Size(100, 30),
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.LightCoral,
                    Font = new Font("Arial", 9)
                };

                // Загружаем все товары
                var allProducts = _productRepo.GetAllProducts().Where(p => p.IsActive).ToList();
                var existingProductIds = _inventoryDisplayItems.Select(i => i.ProductID).ToHashSet();
                var availableProducts = allProducts.Where(p => !existingProductIds.Contains(p.ProductID)).ToList();

                cmbProduct.DataSource = availableProducts;
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";

                if (availableProducts.Count == 0)
                {
                    MessageBox.Show("Нет доступных товаров для добавления!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                form.Controls.AddRange(new Control[] {
                    lblProduct, cmbProduct, lblQuantity, numQuantity, btnOk, btnCancel
                });

                if (form.ShowDialog() == DialogResult.OK && cmbProduct.SelectedItem != null)
                {
                    // Используем dynamic чтобы обойти проблему с пространством имен
                    dynamic product = cmbProduct.SelectedItem;

                    // Для отображения
                    _inventoryDisplayItems.Add(new InventoryItemDisplayDto
                    {
                        ProductID = product.ProductID,
                        ProductName = product.Name,
                        ArticleNumber = product.ArticleNumber,
                        ExpectedQuantity = 0,
                        ActualQuantity = (decimal)numQuantity.Value,
                        Notes = "Добавлен вручную"
                    });

                    // Для сохранения в БД
                    _inventoryItems.Add(new InventoryItem
                    {
                        ProductID = product.ProductID,
                        ExpectedQuantity = 0,
                        ActualQuantity = (decimal)numQuantity.Value,
                        Notes = "Добавлен вручную"
                    });

                    gridInventoryItems.DataSource = _inventoryDisplayItems.ToList();
                    UpdateStatusInfo();
                }
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Завершить инвентаризацию?\n\nПосле завершения все данные будут сохранены в базу данных.",
                "ЗАВЕРШЕНИЕ ИНВЕНТАРИЗАЦИИ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Сохраняем инвентаризацию
                    _currentInventory.InventoryItems = _inventoryItems;
                    var inventoryId = _inventoryRepo.CreateInventory(_currentInventory);

                    // Завершаем инвентаризацию
                    _inventoryRepo.CompleteInventory(inventoryId);

                    _currentInventory.Status = InventoryStatus.Completed;
                    _currentInventory.CompletedDate = DateTime.Now;

                    UpdateStatusInfo();

                    MessageBox.Show($"Инвентаризация №{_currentInventory.InventoryNumber} успешно завершена!\n\n" +
                                  $"Обработано товаров: {_inventoryItems.Count}",
                                  "✅ ИНВЕНТАРИЗАЦИЯ ЗАВЕРШЕНА",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при завершении инвентаризации:\n{ex.Message}", "❌ ОШИБКА",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateStatusInfo()
        {
            var totalItems = _inventoryDisplayItems.Count;
            var withDifferences = _inventoryDisplayItems.Count(i => i.Difference != 0);
            var totalDifference = _inventoryDisplayItems.Sum(i => i.Difference);

            string statusText = _currentInventory.Status switch
            {
                InventoryStatus.Draft => "🟦 Статус: ЧЕРНОВИК",
                InventoryStatus.InProgress => "🟧 Статус: В ПРОЦЕССЕ",
                InventoryStatus.Completed => "🟩 Статус: ЗАВЕРШЕНА",
                InventoryStatus.Cancelled => "🟥 Статус: ОТМЕНЕНА",
                _ => "Статус: НЕИЗВЕСТЕН"
            };

            string details = $"\nТоваров: {totalItems} | С расхождениями: {withDifferences} | Общая разница: {totalDifference}";

            lblStatus.Text = statusText + details;
            lblStatus.ForeColor = _currentInventory.Status switch
            {
                InventoryStatus.Draft => Color.Blue,
                InventoryStatus.InProgress => Color.Orange,
                InventoryStatus.Completed => Color.Green,
                InventoryStatus.Cancelled => Color.Red,
                _ => Color.Black
            };
        }

        private string GenerateInventoryNumber()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_currentInventory.Status == InventoryStatus.InProgress &&
                this.DialogResult != DialogResult.OK)
            {
                var result = MessageBox.Show(
                    "Инвентаризация еще не завершена. Все внесенные данные будут потеряны.\n\nПродолжить?",
                    "ПРЕДУПРЕЖДЕНИЕ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }
    }

    public class InventoryItemDisplayDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public string Notes { get; set; }
        public decimal Difference => ActualQuantity - ExpectedQuantity;
    }
}