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
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        // base.OnPaint(pevent); // We draw manually
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = this.ClientRectangle;
        rect.Width--; rect.Height--;

        using (var path = GetRoundedPath(rect, BorderRadius))
        using (var brush = new SolidBrush(this.BackColor))
        using (var pen = new Pen(this.BackColor, 1))
        {
            g.FillPath(brush, path);
            g.DrawPath(pen, path);
            
            // Text centering
            var strSize = g.MeasureString(this.Text, this.Font);
            g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), 
                (this.Width - strSize.Width) / 2, 
                (this.Height - strSize.Height) / 2);
        }
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
