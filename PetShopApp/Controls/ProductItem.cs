using PetShopApp.Models;
using PetShopApp.Helpers;
using PetShopApp.Services;
using System.Drawing.Drawing2D;
using System.Data;

namespace PetShopApp.Controls;

public class ProductItem : UserControl
{
    private Product _product;
    private PictureBox _photo;
    private Label _lblName;
    private Label _lblPrice;
    private Label _lblOldPrice;
    private Label _lblRating;
    private RoundedButton _btnBuy;
    private Panel _imagePanel;

    public event EventHandler OnBuyClick;
    public event EventHandler OnCardClick;

    public ProductItem(Product product)
    {
        _product = product;
        InitializeComponent();
        LoadData();
        
        this.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(240, 248, 240);
        this.MouseLeave += (s, e) => this.BackColor = Color.White;
        
        foreach (Control c in this.Controls)
        {
            if (c is not Button) 
                c.Click += (s, e) => OnCardClick?.Invoke(this, EventArgs.Empty);
        }
        this.Click += (s, e) => OnCardClick?.Invoke(this, EventArgs.Empty);
    }

    private void InitializeComponent()
    {
        this.Size = new Size(240, 380);
        this.BackColor = Color.White;
        this.Margin = new Padding(10);
        this.Padding = new Padding(5);
        
        var fontPrice = new Font("Segoe UI", 14, FontStyle.Bold);
        var fontOldPrice = new Font("Segoe UI", 9, FontStyle.Strikeout);
        var fontName = new Font("Segoe UI", 10);
        var fontRating = new Font("Segoe UI", 9);

        _imagePanel = new Panel {
            Dock = DockStyle.Top,
            Height = 200,
            BackColor = Color.White,
        };

        _photo = new PictureBox {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        _imagePanel.Controls.Add(_photo);

        _lblPrice = new Label {
            AutoSize = true,
            ForeColor = Color.FromArgb(46, 204, 113), 
            Font = fontPrice,
            Location = new Point(10, 210)
        };

        _lblOldPrice = new Label {
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = fontOldPrice,
            Location = new Point(10, 235) 
        };

        _lblName = new Label {
            Location = new Point(10, 240),
            Size = new Size(220, 45), 
            Font = fontName,
            ForeColor = Color.FromArgb(64, 64, 64),
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };

        _lblRating = new Label {
            Location = new Point(10, 290),
            AutoSize = true,
            Font = fontRating,
            ForeColor = Color.Orange,
            Text = "..."
        };

        _btnBuy = new RoundedButton {
            Text = "В корзину",
            Width = 200,
            Height = 35,
            Location = new Point(20, 330),
            BorderRadius = 15
        };
        _btnBuy.Click += (s, e) => OnBuyClick?.Invoke(this, EventArgs.Empty);

        this.Controls.Add(_btnBuy);
        this.Controls.Add(_lblRating);
        this.Controls.Add(_lblName);
        this.Controls.Add(_lblOldPrice);
        this.Controls.Add(_lblPrice);
        this.Controls.Add(_imagePanel);
    }

    private void LoadData()
    {
        _lblName.Text = _product.ProductName;
        
        decimal finalPrice = _product.ProductCost;
        if (_product.ProductDiscountAmount > 0)
        {
            finalPrice = _product.ProductCost * (1 - _product.ProductDiscountAmount.Value / 100m);
            _lblPrice.Text = $"{finalPrice:N0} ₽";
            _lblOldPrice.Text = $"{_product.ProductCost:N0} ₽";
            _lblOldPrice.Visible = true;
            _lblPrice.ForeColor = Color.FromArgb(231, 76, 60); 
            
            int priceWidth = TextRenderer.MeasureText(_lblPrice.Text, _lblPrice.Font).Width;
            _lblOldPrice.Location = new Point(10 + priceWidth + 10, 215);
        }
        else
        {
            _lblPrice.Text = $"{_product.ProductCost:N0} ₽";
            _lblOldPrice.Visible = false;
            _lblPrice.ForeColor = Color.FromArgb(46, 204, 113); 
        }

        if (_product.Reviews != null && _product.Reviews.Any())
        {
            double avg = _product.Reviews.Average(r => r.Rating);
            int count = _product.Reviews.Count;
            _lblRating.Text = $"★ {avg:N1} ({count})";
            _lblRating.ForeColor = Color.Orange;
        }
        else
        {
            _lblRating.Text = "Нет отзывов";
            _lblRating.ForeColor = Color.Gray;
        }

        string photoPath = "";
        if (_product.Photos != null && _product.Photos.Any())
            photoPath = _product.Photos.First().PhotoPath;
        else if (!string.IsNullOrEmpty(_product.ProductPhoto))
            photoPath = _product.ProductPhoto;
            
        LoadImage(photoPath);
    }

    private void LoadImage(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            LoadPlaceholder();
            return;
        }

        // 1. Try Local
        string fullPath = Path.Combine(Application.StartupPath, "Media", path);
        if (File.Exists(fullPath))
        {
            try {
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    _photo.Image = Image.FromStream(stream);
                }
                return;
            } catch { }
        }

        // 2. Try MinIO (Remote)
        try
        {
            string url = MinioService.Instance.GetFileUrl(path);
            _photo.LoadAsync(url);
        }
        catch
        {
            LoadPlaceholder();
        }
    }

    private void LoadPlaceholder()
    {
        Bitmap bmp = new Bitmap(200, 200);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.WhiteSmoke);
            g.DrawString("Нет фото", new Font("Segoe UI", 10), Brushes.Gray, 60, 90);
        }
        _photo.Image = bmp;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var pen = new Pen(Color.LightGray, 1))
        using (var path = GetRoundedPath(this.ClientRectangle, 20))
        {
            e.Graphics.DrawPath(pen, path);
        }
        
        using (var path = GetRoundedPath(this.ClientRectangle, 20))
        {
            this.Region = new Region(path);
        }
    }
    
    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        float curveSize = radius * 2F;
        rect.Width--; rect.Height--; 
        path.StartFigure();
        path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
        path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
        path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
        path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
        path.CloseFigure();
        return path;
    }
}