using System;
using System.Drawing;
using System.Windows.Forms;
using SWM.ViewModels;
using ProductModel = SWM.Core.Models.Product;

namespace SmartWarehouseManager.SWM.Views.Forms.Products
{
    public partial class AddProductForm : Form  // Добавляем наследование от Form
    {
        private ProductViewModel _viewModel;
        private TextBox txtArticleNumber;
        private TextBox txtName;
        private TextBox txtPrice;
        private TextBox txtStockBalance;
        private TextBox txtCategory;
        private Button btnSave;
        private Button btnCancel;

        public AddProductForm(ProductViewModel viewModel)
        {
            InitializeComponent();  // Должно быть первым!
            _viewModel = viewModel;
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.txtArticleNumber = new TextBox();
            this.txtName = new TextBox();
            this.txtPrice = new TextBox();
            this.txtStockBalance = new TextBox();
            this.txtCategory = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            // Настройка контролов
            this.SuspendLayout();

            // ArticleNumber
            this.txtArticleNumber.Location = new Point(120, 20);
            this.txtArticleNumber.Size = new Size(200, 20);
            Label lblArticle = new Label() { Text = "Артикул:", Location = new Point(20, 23) };

            // Name
            this.txtName.Location = new Point(120, 50);
            this.txtName.Size = new Size(200, 20);
            Label lblName = new Label() { Text = "Название:", Location = new Point(20, 53) };

            // Price
            this.txtPrice.Location = new Point(120, 80);
            this.txtPrice.Size = new Size(200, 20);
            Label lblPrice = new Label() { Text = "Цена:", Location = new Point(20, 83) };

            // StockBalance
            this.txtStockBalance.Location = new Point(120, 110);
            this.txtStockBalance.Size = new Size(200, 20);
            Label lblStock = new Label() { Text = "Количество:", Location = new Point(20, 113) };

            // Category
            this.txtCategory.Location = new Point(120, 140);
            this.txtCategory.Size = new Size(200, 20);
            Label lblCategory = new Label() { Text = "Категория:", Location = new Point(20, 143) };

            // Buttons
            this.btnSave.Text = "Сохранить";
            this.btnSave.Location = new Point(120, 180);
            this.btnSave.Size = new Size(90, 30);
            this.btnSave.Click += BtnSave_Click;

            this.btnCancel.Text = "Отмена";
            this.btnCancel.Location = new Point(220, 180);
            this.btnCancel.Size = new Size(90, 30);
            this.btnCancel.Click += (s, e) => this.Close();

            // Добавляем контролы на форму
            this.Controls.AddRange(new Control[] {
                lblArticle, this.txtArticleNumber,
                lblName, this.txtName,
                lblPrice, this.txtPrice,
                lblStock, this.txtStockBalance,
                lblCategory, this.txtCategory,
                this.btnSave, this.btnCancel
            });

            this.ResumeLayout(false);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                var product = new ProductModel
                {
                    ArticleNumber = txtArticleNumber.Text,
                    Name = txtName.Text,
                    Price = decimal.Parse(txtPrice.Text),
                    StockBalance = int.Parse(txtStockBalance.Text),
                    Category = txtCategory.Text,
                    Description = "Описание товара",
                    Characteristics = "Характеристики",
                    WarehouseID = 1
                };

                _viewModel.AddProduct(product);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtArticleNumber.Text))
            {
                MessageBox.Show("Введите артикул");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название");
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену");
                return false;
            }

            if (!int.TryParse(txtStockBalance.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество");
                return false;
            }

            return true;
        }
    }
}