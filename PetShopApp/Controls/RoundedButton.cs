using System.Drawing.Drawing2D;

namespace PetShopApp.Controls;

public class RoundedButton : Button
{
    public int BorderRadius { get; set; } = 20;

    public RoundedButton()
    {
        this.FlatStyle = FlatStyle.Flat;
        this.FlatAppearance.BorderSize = 0;
        this.Cursor = Cursors.Hand;
        this.BackColor = Color.FromArgb(46, 204, 113);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.Size = new Size(150, 40);
        this.Paint += RoundedButton_Paint;
    }

    private void RoundedButton_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Clear background with parent color to avoid artifacts
        if (this.Parent != null)
        {
            using (var brush = new SolidBrush(this.Parent.BackColor))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }
        }

        var rect = this.ClientRectangle;
        rect.Width--; rect.Height--;

        using (var path = GetRoundedPath(rect, BorderRadius))
        using (var brush = new SolidBrush(this.BackColor))
        // using (var pen = new Pen(this.BackColor, 1)) // No border needed if same color
        {
            g.FillPath(brush, path);
            
            // Text centering
            var strSize = g.MeasureString(this.Text, this.Font);
            g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), 
                (this.Width - strSize.Width) / 2, 
                (this.Height - strSize.Height) / 2);
        }
    }

    // Override OnPaintBackground to prevent system drawing the square background
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Do nothing
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        float curveSize = radius * 2F;
        path.StartFigure();
        path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
        path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
        path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
        path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
        path.CloseFigure();
        return path;
    }
}