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

        public InventoryForm(string connectionString)
        {
            _connectionString = connectionString;
            _inventoryRepo = new InventoryRepository(connectionString);
            _productRepo = new ProductRepository(connectionString);

            _currentInventory = new Inventory
            {
                InventoryDate = DateTime.Today,
                Status = InventoryStatus.Draft
            };

            _inventoryItems = new List<InventoryItem>();

            InitializeComponent();
            LoadWarehouses();
            LoadProductsForInventory();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "📋 Проведение инвентаризации";
            this.ClientSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            CreateControls();
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            int yPos = 20;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "Инвентаризация склада",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true
            };
            yPos += 40;

            // Информация об инвентаризации
            var infoPanel = new Panel()
            {
                Location = new Point(20, yPos),
                Size = new Size(850, 80),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblNumber = new Label() { Text = "Номер:", Location = new Point(10, 15), Width = 80 };
            txtInventoryNumber = new TextBox()
            {
                Location = new Point(95, 12),
                Width = 150,
                Text = GenerateInventoryNumber(),
                ReadOnly = true,
                BackColor = Color.LightGray
            };

            var lblWarehouse = new Label() { Text = "Склад:", Location = new Point(260, 15), Width = 50 };
            cmbWarehouse = new ComboBox()
            {
                Location = new Point(315, 12),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblDate = new Label() { Text = "Дата:", Location = new Point(530, 15), Width = 40 };
            dtpInventoryDate = new DateTimePicker()
            {
                Location = new Point(575, 12),
                Width = 120
            };

            var lblNotes = new Label() { Text = "Примечания:", Location = new Point(10, 45), Width = 80 };
            txtNotes = new TextBox()
            {
                Location = new Point(95, 42),
                Width = 600,
                Height = 25
            };

            infoPanel.Controls.AddRange(new Control[] {
                lblNumber, txtInventoryNumber, lblWarehouse, cmbWarehouse,
                lblDate, dtpInventoryDate, lblNotes, txtNotes
            });
            yPos += 90;

            // Статус
            lblStatus = new Label()
            {
                Text = "Статус: Черновик",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Blue
            };
            yPos += 30;

            // Товары для инвентаризации
            var lblItems = new Label()
            {
                Text = "Товары для инвентаризации:",
                Location = new Point(20, yPos),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            yPos += 25;

            gridInventoryItems = new DataGridView()
            {
                Location = new Point(20, yPos),
                Size = new Size(850, 300),
                AllowUserToAddRows = false
            };
            yPos += 310;

            // Кнопки
            var buttonsPanel = new Panel()
            {
                Location = new Point(20, yPos),
                Size = new Size(850, 40)
            };

            btnStartInventory = new Button()
            {
                Text = "▶️ Начать инвентаризацию",
                Location = new Point(0, 5),
                Size = new Size(160, 30),
                BackColor = Color.LightGreen
            };
            btnStartInventory.Click += BtnStartInventory_Click;

            btnAddProduct = new Button()
            {
                Text = "➕ Добавить товар",
                Location = new Point(170, 5),
                Size = new Size(140, 30)
            };
            btnAddProduct.Click += BtnAddProduct_Click;

            btnComplete = new Button()
            {
                Text = "✅ Завершить",
                Location = new Point(320, 5),
                Size = new Size(100, 30),
                BackColor = Color.LightBlue,
                Enabled = false
            };
            btnComplete.Click += BtnComplete_Click;

            btnCancel = new Button()
            {
                Text = "❌ Отмена",
                Location = new Point(430, 5),
                Size = new Size(100, 30)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonsPanel.Controls.AddRange(new Control[] {
                btnStartInventory, btnAddProduct, btnComplete, btnCancel
            });

            this.Controls.AddRange(new Control[] {
                lblTitle, infoPanel, lblStatus, lblItems, gridInventoryItems, buttonsPanel
            });

            SetupInventoryGrid();
        }

        private void LoadWarehouses()
        {
            // Временные данные - замени на реальные из БД
            cmbWarehouse.Items.Add(new { Name = "Основной склад", WarehouseID = 1 });
            cmbWarehouse.Items.Add(new { Name = "Резервный склад", WarehouseID = 2 });
            cmbWarehouse.DisplayMember = "Name";
            cmbWarehouse.ValueMember = "WarehouseID";
            cmbWarehouse.SelectedIndex = 0;
        }

        private void LoadProductsForInventory()
        {
            var products = _productRepo.GetAllProducts();
            var warehouseId = ((dynamic)cmbWarehouse.SelectedItem).WarehouseID;

            var warehouseProducts = products.Where(p => p.WarehouseID == warehouseId).ToList();

            _inventoryItems.Clear();
            foreach (var product in warehouseProducts)
            {
                _inventoryItems.Add(new InventoryItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.Name,
                    ArticleNumber = product.ArticleNumber,
                    ExpectedQuantity = product.StockBalance,
                    ActualQuantity = product.StockBalance,
                    Notes = ""
                });
            }

            gridInventoryItems.DataSource = _inventoryItems.ToList();
        }

        private void SetupInventoryGrid()
        {
            gridInventoryItems.AutoGenerateColumns = false;
            gridInventoryItems.Columns.Clear();

            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductName",
                HeaderText = "Товар",
                Width = 200,
                ReadOnly = true
            });

            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ArticleNumber",
                HeaderText = "Артикул",
                Width = 100,
                ReadOnly = true
            });

            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ExpectedQuantity",
                HeaderText = "Ожидается",
                Width = 80,
                ReadOnly = true
            });

            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ActualQuantity",
                HeaderText = "Фактически",
                Width = 80
            });

            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Difference",
                HeaderText = "Разница",
                Width = 80,
                ReadOnly = true
            });

            gridInventoryItems.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Notes",
                HeaderText = "Примечание",
                Width = 200
            });

            gridInventoryItems.CellValueChanged += GridInventoryItems_CellValueChanged;
        }

        private void GridInventoryItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 3) // Изменение ActualQuantity
            {
                var row = gridInventoryItems.Rows[e.RowIndex];
                var expected = Convert.ToDecimal(row.Cells[2].Value);
                var actual = Convert.ToDecimal(row.Cells[3].Value);
                row.Cells[4].Value = actual - expected;

                // Подсветка расхождений
                if (actual != expected)
                {
                    row.DefaultCellStyle.BackColor = actual < expected ? Color.LightCoral : Color.LightGreen;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private void BtnStartInventory_Click(object sender, EventArgs e)
        {
            _currentInventory.Status = InventoryStatus.InProgress;
            _currentInventory.InventoryDate = dtpInventoryDate.Value;
            _currentInventory.WarehouseID = ((dynamic)cmbWarehouse.SelectedItem).WarehouseID;
            _currentInventory.Notes = txtNotes.Text;

            lblStatus.Text = "Статус: В процессе";
            lblStatus.ForeColor = Color.Orange;

            btnStartInventory.Enabled = false;
            btnComplete.Enabled = true;
            cmbWarehouse.Enabled = false;
            dtpInventoryDate.Enabled = false;

            MessageBox.Show("Инвентаризация начата! Теперь можно вносить фактические данные.", "Инвентаризация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            // Диалог добавления товара
            using (var form = new Form())
            {
                form.Text = "Добавить товар в инвентаризацию";
                form.Size = new Size(400, 200);
                form.StartPosition = FormStartPosition.CenterParent;

                var lblProduct = new Label() { Text = "Товар:", Location = new Point(20, 30), Width = 80 };
                var cmbProduct = new ComboBox()
                {
                    Location = new Point(100, 27),
                    Width = 250,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                var products = _productRepo.GetAllProducts();
                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";

                var lblQuantity = new Label() { Text = "Количество:", Location = new Point(20, 70), Width = 80 };
                var numQuantity = new NumericUpDown()
                {
                    Location = new Point(100, 67),
                    Width = 100,
                    Minimum = 0,
                    Maximum = 10000
                };

                var btnOk = new Button() { Text = "Добавить", Location = new Point(100, 110), DialogResult = DialogResult.OK };
                var btnCancel = new Button() { Text = "Отмена", Location = new Point(200, 110), DialogResult = DialogResult.Cancel };

                form.Controls.AddRange(new Control[] {
                    lblProduct, cmbProduct, lblQuantity, numQuantity, btnOk, btnCancel
                });

                if (form.ShowDialog() == DialogResult.OK && cmbProduct.SelectedItem is Product product)
                {
                    _inventoryItems.Add(new InventoryItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.Name,
                        ArticleNumber = product.ArticleNumber,
                        ExpectedQuantity = 0,
                        ActualQuantity = (int)numQuantity.Value,
                        Notes = "Добавлен вручную"
                    });

                    gridInventoryItems.DataSource = _inventoryItems.ToList();
                }
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Завершить инвентаризацию? После завершения изменения будут сохранены.",
                "Завершение инвентаризации", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Сохраняем инвентаризацию
                    _currentInventory.InventoryItems = _inventoryItems;
                    var inventoryId = _inventoryRepo.CreateInventory(_currentInventory);

                    // Завершаем инвентаризацию (обновляем остатки)
                    _inventoryRepo.CompleteInventory(inventoryId);

                    lblStatus.Text = "Статус: Завершена ✅";
                    lblStatus.ForeColor = Color.Green;

                    MessageBox.Show("Инвентаризация успешно завершена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при завершении инвентаризации: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GenerateInventoryNumber()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }
    }
}