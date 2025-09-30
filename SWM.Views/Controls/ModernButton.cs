using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class ModernButton : Button
{
    private bool _isHovered;

    public ModernButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.FromArgb(0, 122, 204);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        Cursor = Cursors.Hand;
        Size = new Size(120, 36);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var brush = new SolidBrush(BackColor);

        // Рисуем закругленный прямоугольник
        var path = GetRoundedPath(rect, 6);
        graphics.FillPath(brush, path);

        // Эффект при наведении
        if (_isHovered)
        {
            using (var hoverBrush = new SolidBrush(Color.FromArgb(30, Color.White)))
            {
                graphics.FillPath(hoverBrush, path);
            }
        }

        // Текст
        var stringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(Text, Font, new SolidBrush(ForeColor), rect, stringFormat);

        brush.Dispose();
        path.Dispose();
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

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }
}