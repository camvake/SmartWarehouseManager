using System;
using System.Drawing;
using System.Windows.Forms;

public class ReceiveSupplyForm : BaseForm
{
    private string _supplyId;
    private string _supplier;
    private System.Windows.Forms.TextBox receivedQuantityTextBox;
    private System.Windows.Forms.TextBox damageNotesTextBox;
    private ModernButton receiveButton;

    public ReceiveSupplyForm(string supplyId, string supplier)
    {
        _supplyId = supplyId;
        _supplier = supplier;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(500, 350);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);
        this.Text = "Прием поставки";

        // Заголовок
        var titleLabel = new Label
        {
            Text = $"Прием поставки #{_supplyId}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(titleLabel);

        var supplierLabel = new Label
        {
            Text = $"Поставщик: {_supplier}",
            Font = new Font("Segoe UI", 10),
            Location = new Point(20, 50),
            AutoSize = true
        };
        this.Controls.Add(supplierLabel);

        int y = 90;

        // Полученное количество
        var quantityLabel = new Label { Text = "Полученное количество:", Location = new Point(20, y), AutoSize = true };
        receivedQuantityTextBox = new System.Windows.Forms.TextBox
        {
            Location = new Point(180, y - 3),
            Size = new Size(100, 25),
            Text = "0"
        };
        this.Controls.AddRange(new Control[] { quantityLabel, receivedQuantityTextBox });
        y += 40;

        // Примечания о повреждениях
        var damageLabel = new Label { Text = "Примечания о повреждениях:", Location = new Point(20, y), AutoSize = true };
        damageNotesTextBox = new System.Windows.Forms.TextBox
        {
            Location = new Point(180, y - 3),
            Size = new Size(280, 80),
            Multiline = true,
            PlaceholderText = "Укажите поврежденные товары, если есть..."
        };
        this.Controls.AddRange(new Control[] { damageLabel, damageNotesTextBox });
        y += 100;

        // Кнопки
        receiveButton = new ModernButton
        {
            Text = "✅ Принять поставку",
            Location = new Point(180, y),
            Size = new Size(150, 35),
            BackColor = Color.FromArgb(40, 167, 69)
        };
        receiveButton.Click += ReceiveButton_Click;

        var cancelButton = new ModernButton
        {
            Text = "❌ Отмена",
            Location = new Point(340, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(108, 117, 125)
        };
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { receiveButton, cancelButton });
    }

    private void ReceiveButton_Click(object sender, EventArgs e)
    {
        if (ValidateForm())
        {
            // Логика приема поставки
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    private bool ValidateForm()
    {
        if (!int.TryParse(receivedQuantityTextBox.Text, out int quantity) || quantity < 0)
        {
            ShowError("Введите корректное количество");
            receivedQuantityTextBox.Focus();
            return false;
        }

        return true;
    }
}