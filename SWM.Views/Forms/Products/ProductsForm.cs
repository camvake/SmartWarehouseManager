using System.Windows.Forms;
using SWM.ViewModels;
using System;
using System.Drawing;

namespace SWM.Views.Forms.Product
{
    public class ProductsForm : Form
    {
        private ProductViewModel _viewModel;
        private DataGridView dataGridViewProducts;
        private Button btnAdd;

        public ProductsForm(string connectionString)
        {
            // �������, ��� ��� ����� ProductsForm.Designer.cs
            InitializeComponent();
            _viewModel = new ProductViewModel(connectionString);
            SetupDataGrid();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // DataGridView
            this.dataGridViewProducts = new DataGridView();
            this.dataGridViewProducts.Dock = DockStyle.Fill;
            this.dataGridViewProducts.Location = new Point(0, 50);
            this.dataGridViewProducts.Size = new Size(800, 400);
            this.dataGridViewProducts.TabIndex = 0;

            // Button
            this.btnAdd = new Button();
            this.btnAdd.Text = "��������";
            this.btnAdd.Location = new Point(12, 12);
            this.btnAdd.Size = new Size(100, 30);
            this.btnAdd.Click += BtnAdd_Click;

            // Panel for button
            var panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = 50;
            panel.Controls.Add(this.btnAdd);

            // Form
            this.Text = "���������� ��������";
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(this.dataGridViewProducts);
            this.Controls.Add(panel);

            this.ResumeLayout(false);
        }

        private void SetupDataGrid()
        {
            dataGridViewProducts.AutoGenerateColumns = false;

            // ������� ������� ����� �����������
            dataGridViewProducts.Columns.Clear();

            // ����������� �������
            dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ArticleNumber",
                HeaderText = "�������",
                Width = 100
            });

            dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Name",
                HeaderText = "��������",
                Width = 200
            });

            dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Price",
                HeaderText = "����",
                Width = 80
            });

            dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "StockBalance",
                HeaderText = "�������",
                Width = 60
            });
        }

        private void LoadProducts()
        {
            dataGridViewProducts.DataSource = _viewModel.Products;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowAddProductForm();
        }

        private void ShowAddProductForm()
        {
            using (var form = new Form())
            {
                form.Text = "�������� �����";
                form.Size = new Size(350, 250);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                // ������� �������� �����
                var lblArticle = new Label() { Text = "�������:", Location = new Point(20, 20), Width = 80 };
                var txtArticle = new TextBox() { Location = new Point(100, 20), Width = 200 };

                var lblName = new Label() { Text = "��������:", Location = new Point(20, 50), Width = 80 };
                var txtName = new TextBox() { Location = new Point(100, 50), Width = 200 };

                var lblPrice = new Label() { Text = "����:", Location = new Point(20, 80), Width = 80 };
                var txtPrice = new TextBox() { Location = new Point(100, 80), Width = 200 };

                var lblStock = new Label() { Text = "����������:", Location = new Point(20, 110), Width = 80 };
                var txtStock = new TextBox() { Location = new Point(100, 110), Width = 200 };

                var lblCategory = new Label() { Text = "���������:", Location = new Point(20, 140), Width = 80 };
                var txtCategory = new TextBox() { Location = new Point(100, 140), Width = 200 };

                var btnOk = new Button() { Text = "��������", Location = new Point(100, 170), Size = new Size(80, 30), DialogResult = DialogResult.OK };
                var btnCancel = new Button() { Text = "������", Location = new Point(190, 170), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };

                form.Controls.AddRange(new Control[] {
                    lblArticle, txtArticle, lblName, txtName,
                    lblPrice, txtPrice, lblStock, txtStock,
                    lblCategory, txtCategory, btnOk, btnCancel
                });

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (ValidateProductForm(txtArticle.Text, txtName.Text, txtPrice.Text, txtStock.Text))
                    {
                        var product = new SWM.Core.Models.Product
                        {
                            ArticleNumber = txtArticle.Text,
                            Name = txtName.Text,
                            Price = decimal.Parse(txtPrice.Text),
                            StockBalance = int.Parse(txtStock.Text),
                            Category = txtCategory.Text,
                            Description = "����� �����",
                            Characteristics = "",
                            WarehouseID = 1
                        };

                        _viewModel.AddProduct(product);
                        LoadProducts();
                    }
                }
            }
        }

        private bool ValidateProductForm(string article, string name, string price, string stock)
        {
            if (string.IsNullOrWhiteSpace(article))
            {
                MessageBox.Show("������� �������");
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("������� ��������");
                return false;
            }

            if (!decimal.TryParse(price, out decimal priceValue) || priceValue <= 0)
            {
                MessageBox.Show("������� ���������� ����");
                return false;
            }

            if (!int.TryParse(stock, out int stockValue) || stockValue < 0)
            {
                MessageBox.Show("������� ���������� ����������");
                return false;
            }

            return true;
        }
    }
}