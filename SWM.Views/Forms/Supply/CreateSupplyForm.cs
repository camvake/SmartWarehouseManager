using System;
using System.Drawing;
using System.Windows.Forms;
using SWM.ViewModels;

public class CreateSupplyForm : BaseForm
{
    private SupplyViewModel _viewModel;
    private System.Windows.Forms.ComboBox supplierComboBox;
    private DateTimePicker expectedDatePicker;
    private System.Windows.Forms.TextBox totalAmountTextBox;
    private System.Windows.Forms.TextBox notesTextBox;
    private ModernButton saveButton;

    public CreateSupplyForm(SupplyViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
    }

    public CreateSupplyForm() : this(new SupplyViewModel(""))
    {
    }

    private void InitializeComponent()
    {
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);
        this.Text = "Создание новой поставки";

        // Заголовок
        var titleLabel = new Label
        {
            Text = "Новая поставка",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(titleLabel);

        int y = 70;

        // Поставщик
        var supplierLabel = new Label { Text = "Поставщик*:", Location = new Point(20, y), AutoSize = true };
        supplierComboBox = new System.Windows.Forms.ComboBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(300, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Заполняем поставщиками
        foreach (var supplier in _viewModel.Suppliers)
        {
            supplierComboBox.Items.Add(new SupplierComboBoxItem
            {
                Text = supplier.SupplierName,
                Value = supplier.SupplierID
            });
        }
        supplierComboBox.DisplayMember = "Text";

        this.Controls.AddRange(new Control[] { supplierLabel, supplierComboBox });
        y += 40;

        // Ожидаемая дата
        var dateLabel = new Label { Text = "Ожидаемая дата*:", Location = new Point(20, y), AutoSize = true };
        expectedDatePicker = new DateTimePicker
        {
            Location = new Point(120, y - 3),
            Size = new Size(200, 25),
            Value = DateTime.Now.AddDays(3)
        };
        this.Controls.AddRange(new Control[] { dateLabel, expectedDatePicker });
        y += 40;

        // Сумма
        var amountLabel = new Label { Text = "Сумма:", Location = new Point(20, y), AutoSize = true };
        totalAmountTextBox = new System.Windows.Forms.TextBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(150, 25),
            Text = "0"
        };
        this.Controls.AddRange(new Control[] { amountLabel, totalAmountTextBox });
        y += 40;

        // Примечания
        var notesLabel = new Label { Text = "Примечания:", Location = new Point(20, y), AutoSize = true };
        notesTextBox = new System.Windows.Forms.TextBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(300, 80),
            Multiline = true
        };
        this.Controls.AddRange(new Control[] { notesLabel, notesTextBox });
        y += 100;

        // Кнопки
        saveButton = new ModernButton
        {
            Text = "💾 Создать поставку",
            Location = new Point(120, y),
            Size = new Size(150, 35)
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
            if (supplierComboBox.SelectedItem is SupplierComboBoxItem selectedSupplier)
            {
                _viewModel.EditingSupply.SupplierID = selectedSupplier.Value;
            }

            _viewModel.EditingSupply.ExpectedDate = expectedDatePicker.Value;
            _viewModel.EditingSupply.TotalAmount = decimal.Parse(totalAmountTextBox.Text);
            _viewModel.EditingSupply.Notes = notesTextBox.Text;

            _viewModel.CreateSupplyCommand.Execute(null);

            if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                ShowError(_viewModel.ErrorMessage);
            }
        }
    }

    private bool ValidateForm()
    {
        if (supplierComboBox.SelectedItem == null)
        {
            ShowError("Выберите поставщика");
            supplierComboBox.Focus();
            return false;
        }

        if (expectedDatePicker.Value < DateTime.Now.Date)
        {
            ShowError("Дата поставки не может быть в прошлом");
            expectedDatePicker.Focus();
            return false;
        }

        if (!decimal.TryParse(totalAmountTextBox.Text, out decimal amount) || amount < 0)
        {
            ShowError("Введите корректную сумму");
            totalAmountTextBox.Focus();
            return false;
        }

        return true;
    }
}