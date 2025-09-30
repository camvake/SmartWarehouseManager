using System;
using System.Drawing;
using System.Windows.Forms;

public class BaseForm : Form
{
    protected void ShowError(string message)
    {
        MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected void ShowWarning(string message)
    {
        MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    protected DialogResult ShowQuestion(string message)
    {
        return MessageBox.Show(message, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }
}