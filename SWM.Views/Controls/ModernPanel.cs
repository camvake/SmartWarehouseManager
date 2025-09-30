using System.Drawing.Drawing2D;

public class ModernPanel : Panel
{
    public Color BorderColor { get; set; } = Color.FromArgb(225, 225, 225);
    public int BorderRadius { get; set; } = 6;

    public ModernPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
        BorderStyle = BorderStyle.None;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // Фон
        using (var backgroundBrush = new SolidBrush(BackColor))
        using (var path = GetRoundedPath(rect, BorderRadius))
        {
            graphics.FillPath(backgroundBrush, path);
        }

        // Граница
        using (var borderPen = new Pen(BorderColor, 1))
        using (var path = GetRoundedPath(rect, BorderRadius))
        {
            graphics.DrawPath(borderPen, path);
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