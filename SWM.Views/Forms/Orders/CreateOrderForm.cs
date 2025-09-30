using System;
using System.Drawing;
using System.Windows.Forms;

public class CreateOrderForm : BaseForm
{
    private TextBox customerNameTextBox;
    private TextBox customerPhoneTextBox;
    private TextBox deliveryAddressTextBox;
    private ModernButton saveButton;

    public CreateOrderForm()
    {
        InitializeComponent();
        this.Text = "Создание нового заказа";
    }

    private void InitializeComponent()
    {
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);

        // Заголовок
        var titleLabel = new Label
        {
            Text = "Создание заказа",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(titleLabel);

        int y = 70;

        // ФИО клиента
        var nameLabel = new Label { Text = "ФИО клиента*:", Location = new Point(20, y), AutoSize = true };
        customerNameTextBox = new TextBox { Location = new Point(150, y - 3), Size = new Size(300, 25) };
        this.Controls.AddRange(new Control[] { nameLabel, customerNameTextBox });
        y += 40;

        // Телефон
        var phoneLabel = new Label { Text = "Телефон*:", Location = new Point(20, y), AutoSize = true };
        customerPhoneTextBox = new TextBox { Location = new Point(150, y - 3), Size = new Size(200, 25) };
        this.Controls.AddRange(new Control[] { phoneLabel, customerPhoneTextBox });
        y += 40;

        // Адрес доставки
        var addressLabel = new Label { Text = "Адрес доставки:", Location = new Point(20, y), AutoSize = true };
        deliveryAddressTextBox = new TextBox { Location = new Point(150, y - 3), Size = new Size(300, 25) };
        this.Controls.AddRange(new Control[] { addressLabel, deliveryAddressTextBox });
        y += 60;

        // Кнопки
        saveButton = new ModernButton
        {
            Text = "💾 Сохранить",
            Location = new Point(150, y),
            Size = new Size(120, 35)
        };
        saveButton.Click += SaveButton_Click;

        var cancelButton = new ModernButton
        {
            Text = "❌ Отмена",
            Location = new Point(280, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(108, 117, 125)
        };
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { saveButton, cancelButton });
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        if (ValidateForm())
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(customerNameTextBox.Text))
        {
            ShowError("Введите ФИО клиента");
            customerNameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(customerPhoneTextBox.Text))
        {
            ShowError("Введите телефон клиента");
            customerPhoneTextBox.Focus();
            return false;
        }

        return true;
    }
}