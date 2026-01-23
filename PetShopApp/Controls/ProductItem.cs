using System.Diagnostics;
using PetShopApp.Models;
using System.Drawing.Drawing2D;

namespace PetShopApp.Controls;

public class ProductItem : UserControl
{
    private Product _product;
    private PictureBox _photo;
    private Label _lblName;
    private Label _lblPrice;
    private Label _lblOldPrice;
    private Label _lblRating;
    private Button _btnBuy;
    private Panel _imagePanel;

    public event EventHandler OnBuyClick;
    public event EventHandler OnCardClick;

    public ProductItem(Product product)
    {
        _product = product;
        InitializeComponent();
        LoadData();
        
        // Hover effects
        this.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(245, 245, 245);
        this.MouseLeave += (s, e) => this.BackColor = Color.White;
        
        // Propagate clicks
        foreach (Control c in this.Controls)
        {
            if (c is not Button) 
                c.Click += (s, e) => OnCardClick?.Invoke(this, EventArgs.Empty);
        }
        this.Click += (s, e) => OnCardClick?.Invoke(this, EventArgs.Empty);
    }

    private void InitializeComponent()
    {
        this.Size = new Size(220, 360);
        this.BackColor = Color.White;
        this.Margin = new Padding(10);
        this.Padding = new Padding(10);
        
        // Styles
        var fontPrice = new Font("Segoe UI", 14, FontStyle.Bold);
        var fontOldPrice = new Font("Segoe UI", 9, FontStyle.Strikeout);
        var fontName = new Font("Segoe UI", 10);
        var fontRating = new Font("Segoe UI", 9);

        // Image Container
        _imagePanel = new Panel {
            Dock = DockStyle.Top,
            Height = 180,
            BackColor = Color.White,
        };

        _photo = new PictureBox {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        _imagePanel.Controls.Add(_photo);

        // Price Section
        _lblPrice = new Label {
            AutoSize = true,
            ForeColor = Color.FromArgb(46, 204, 113), // Green
            Font = fontPrice,
            Location = new Point(10, 190)
        };

        _lblOldPrice = new Label {
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = fontOldPrice,
            Location = new Point(10, 215) // Will adjust dynamically
        };

        // Name
        _lblName = new Label {
            Location = new Point(10, 235),
            Size = new Size(200, 45), // 2 lines
            Font = fontName,
            ForeColor = Color.FromArgb(64, 64, 64),
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };

        // Rating
        _lblRating = new Label {
            Location = new Point(10, 280),
            AutoSize = true,
            Font = fontRating,
            ForeColor = Color.Orange,
            Text = "★ 4.8 (120 отзывов)" // Mock data or fetch later
        };

        // Button
        _btnBuy = new Button {
            Text = "В корзину",
            Dock = DockStyle.Bottom,
            Height = 40,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnBuy.FlatAppearance.BorderSize = 0;
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
        
        // Price Calculation
        decimal finalPrice = _product.ProductCost;
        if (_product.ProductDiscountAmount > 0)
        {
            finalPrice = _product.ProductCost * (1 - _product.ProductDiscountAmount.Value / 100m);
            _lblPrice.Text = $"{finalPrice:N0} ₽";
            _lblOldPrice.Text = $"{_product.ProductCost:N0} ₽";
            _lblOldPrice.Visible = true;
            _lblPrice.ForeColor = Color.FromArgb(231, 76, 60); // Red for discount
            
            // Adjust layout
            int priceWidth = TextRenderer.MeasureText(_lblPrice.Text, _lblPrice.Font).Width;
            _lblOldPrice.Location = new Point(10 + priceWidth + 5, 195);
        }
        else
        {
            _lblPrice.Text = $"{_product.ProductCost:N0} ₽";
            _lblOldPrice.Visible = false;
            _lblPrice.ForeColor = Color.FromArgb(46, 204, 113); // Green normal
        }

        // Image Load
        string photoPath = Path.Combine(Application.StartupPath, "Media", _product.ProductPhoto ?? "");
        if (File.Exists(photoPath))
        {
            try {
                using (var stream = new FileStream(photoPath, FileMode.Open, FileAccess.Read))
                {
                    _photo.Image = Image.FromStream(stream);
                }
            } catch { LoadPlaceholder(); }
        }
        else
        {
            LoadPlaceholder();
        }
    }

    private void LoadPlaceholder()
    {
        // Draw a simple placeholder
        Bitmap bmp = new Bitmap(200, 200);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.WhiteSmoke);
            g.DrawString("Нет фото", new Font("Segoe UI", 10), Brushes.Gray, 60, 90);
            g.DrawRectangle(Pens.LightGray, 0, 0, 199, 199);
        }
        _photo.Image = bmp;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Draw border
        ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.FromArgb(230,230,230), ButtonBorderStyle.Solid);
    }
}
