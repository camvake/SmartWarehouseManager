using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class DashboardForm : Form
{
    public DashboardForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Size = new Size(980, 740);
        this.BackColor = Color.FromArgb(250, 250, 250);

        // Заголовок
        var titleLabel = new Label();
        titleLabel.Text = "Дашборд";
        titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        titleLabel.ForeColor = Color.FromArgb(60, 60, 60);
        titleLabel.Location = new Point(30, 30);
        titleLabel.AutoSize = true;
        this.Controls.Add(titleLabel);

        // Создаем карточки статистики
        CreateStatCard("Общее количество заказов", "125", Color.FromArgb(0, 122, 204), new Point(30, 100));
        CreateStatCard("Новых заказов сегодня", "8", Color.FromArgb(40, 167, 69), new Point(260, 100));
        CreateStatCard("Товаров на складе", "542", Color.FromArgb(255, 193, 7), new Point(490, 100));
        CreateStatCard("Ожидают поставки", "15", Color.FromArgb(220, 53, 69), new Point(720, 100));
    }

    private void CreateStatCard(string title, string value, Color color, Point location)
    {
        var cardPanel = new Panel();
        cardPanel.Size = new Size(200, 120);
        cardPanel.Location = location;
        cardPanel.BackColor = Color.White;
        cardPanel.Paint += (s, e) => CardPanel_Paint(s, e, color);

        // Значение
        var valueLabel = new Label();
        valueLabel.Text = value;
        valueLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
        valueLabel.ForeColor = Color.FromArgb(60, 60, 60);
        valueLabel.Location = new Point(20, 20);
        valueLabel.AutoSize = true;
        cardPanel.Controls.Add(valueLabel);

        // Заголовок
        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.Font = new Font("Segoe UI", 9);
        titleLabel.ForeColor = Color.FromArgb(120, 120, 120);
        titleLabel.Location = new Point(20, 65);
        titleLabel.Size = new Size(160, 40);
        titleLabel.TextAlign = ContentAlignment.TopLeft;
        cardPanel.Controls.Add(titleLabel);

        this.Controls.Add(cardPanel);
    }

    private void CardPanel_Paint(object sender, PaintEventArgs e, Color color)
    {
        var panel = (Panel)sender;

        // Рисуем закругленные углы и верхнюю полосу цвета
        using (var path = GetRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8))
        using (var brush = new SolidBrush(Color.White))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(brush, path);
        }

        // Верхняя цветная полоса
        using (var brush = new SolidBrush(color))
        {
            e.Graphics.FillRectangle(brush, 0, 0, panel.Width, 4);
        }

        // Граница
        using (var pen = new Pen(Color.FromArgb(240, 240, 240), 1))
        using (var path = GetRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8))
        {
            e.Graphics.DrawPath(pen, path);
        }
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}