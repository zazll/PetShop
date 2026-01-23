using System.Drawing.Drawing2D;

namespace PetShopApp.Helpers;

public static class UIHelper
{
    public static void SetRoundedRegion(Control c, int radius)
    {
        Rectangle bounds = new Rectangle(0, 0, c.Width, c.Height);
        float d = radius * 2.0f;
        GraphicsPath gp = new GraphicsPath();
        gp.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        gp.AddArc(bounds.X + bounds.Width - d, bounds.Y, d, d, 270, 90);
        gp.AddArc(bounds.X + bounds.Width - d, bounds.Y + bounds.Height - d, d, d, 0, 90);
        gp.AddArc(bounds.X, bounds.Y + bounds.Height - d, d, d, 90, 90);
        c.Region = new Region(gp);
    }
    
    public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
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

