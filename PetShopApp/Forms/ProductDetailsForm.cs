using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.IO;

namespace PetShopApp.Forms;

public class ProductDetailsForm : Form
{
    private Product _product;
    private PetShopContext _context;
    
    // UI
    private Panel _mainPanel;
    private PictureBox _photoBox;
    
    public ProductDetailsForm(Product product)
    {
        _product = product;
        _context = new PetShopContext();
        InitializeComponent();
        LoadReviews();
    }

    private void InitializeComponent()
    {
        this.Text = $"{_product.ProductName} — PetShop";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        // Split Container
        var split = new SplitContainer {
            Dock = DockStyle.Fill,
            SplitterDistance = 400,
            IsSplitterFixed = true,
            Orientation = Orientation.Vertical
        };

        // --- Left: Image ---
        _photoBox = new PictureBox {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Padding = new Padding(20)
        };
        LoadImage();
        split.Panel1.Controls.Add(_photoBox);

        // --- Right: Info ---
        var rightPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20),
            AutoScroll = true,
            WrapContents = false
        };

        // Brand
        var lblBrand = new Label {
            Text = _product.Manufacturer.ManufacturerName,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 12),
            AutoSize = true
        };

        // Name
        var lblName = new Label {
            Text = _product.ProductName,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            MaximumSize = new Size(500, 0)
        };

        // Price Block
        var pricePanel = new Panel { Size = new Size(500, 60), Margin = new Padding(0, 10, 0, 10) };
        decimal finalPrice = _product.ProductCost;
        if (_product.ProductDiscountAmount > 0)
        {
            finalPrice = _product.ProductCost * (1 - _product.ProductDiscountAmount.Value / 100m);
            var lblFinal = new Label { 
                Text = $"{finalPrice:N0} ₽", 
                Font = new Font("Segoe UI", 24, FontStyle.Bold), 
                ForeColor = Color.Red, 
                AutoSize = true, 
                Location = new Point(0, 0) 
            };
            var lblOld = new Label { 
                Text = $"{_product.ProductCost:N0} ₽", 
                Font = new Font("Segoe UI", 14, FontStyle.Strikeout), 
                ForeColor = Color.Gray, 
                AutoSize = true, 
                Location = new Point(lblFinal.Right + 100, 10) // Approx
            };
            pricePanel.Controls.Add(lblFinal);
            pricePanel.Controls.Add(lblOld);
            
            // Fix position after adding
             lblOld.Location = new Point(200, 10); 
        }
        else
        {
            var lblFinal = new Label { 
                Text = $"{_product.ProductCost:N0} ₽", 
                Font = new Font("Segoe UI", 24, FontStyle.Bold), 
                ForeColor = Color.FromArgb(46, 204, 113), 
                AutoSize = true 
            };
            pricePanel.Controls.Add(lblFinal);
        }

        // Buy Button
        var btnBuy = new Button {
            Text = "Добавить в корзину",
            Size = new Size(250, 50),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 10, 0, 20)
        };
        btnBuy.Click += (s, e) => {
            CartService.Instance.AddToCart(_product);
            MessageBox.Show("Товар добавлен в корзину");
        };

        // Description
        var lblDescTitle = new Label { Text = "Описание", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
        var lblDesc = new Label { 
            Text = _product.ProductDescription ?? "Нет описания", 
            Font = new Font("Segoe UI", 11), 
            AutoSize = true, 
            MaximumSize = new Size(500, 0) 
        };

        // Reviews Section Container
        var lblReviewsTitle = new Label { Text = "Отзывы", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 30, 0, 5) };
        
        rightPanel.Controls.Add(lblBrand);
        rightPanel.Controls.Add(lblName);
        rightPanel.Controls.Add(pricePanel);
        rightPanel.Controls.Add(btnBuy);
        rightPanel.Controls.Add(lblDescTitle);
        rightPanel.Controls.Add(lblDesc);
        rightPanel.Controls.Add(lblReviewsTitle);
        
        // Add container for reviews
        _reviewsPanel = new FlowLayoutPanel {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            MaximumSize = new Size(540, 0),
            WrapContents = false
        };
        rightPanel.Controls.Add(_reviewsPanel);

        // Add "Add Review" button
        var btnAddReview = new Button {
            Text = "Написать отзыв",
            Size = new Size(150, 30),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAddReview.Click += (s, e) => {
             new ReviewForm(_product).ShowDialog();
             LoadReviews(); // Refresh
        };
        rightPanel.Controls.Add(btnAddReview);

        split.Panel2.Controls.Add(rightPanel);
        this.Controls.Add(split);
    }
    
    private FlowLayoutPanel _reviewsPanel;

    private void LoadImage()
    {
        string photoPath = Path.Combine(Application.StartupPath, "Media", _product.ProductPhoto ?? "");
        if (File.Exists(photoPath))
            _photoBox.Image = Image.FromFile(photoPath);
        else 
        {
            // Placeholder
             Bitmap bmp = new Bitmap(400, 400);
             using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.WhiteSmoke); g.DrawString("Нет фото", this.Font, Brushes.Gray, 100, 100); }
             _photoBox.Image = bmp;
        }
    }

    private void LoadReviews()
    {
        _reviewsPanel.Controls.Clear();
        var reviews = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductID == _product.ProductID)
            .OrderByDescending(r => r.ReviewDate)
            .ToList();

        if (!reviews.Any())
        {
            _reviewsPanel.Controls.Add(new Label { Text = "Отзывов пока нет", AutoSize = true, ForeColor = Color.Gray });
            return;
        }

        foreach (var r in reviews)
        {
            var p = new Panel { Size = new Size(500, 80), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10) };
            var lName = new Label { Text = $"{r.User.UserSurname} {r.User.UserName}", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            var lRating = new Label { Text = $"Оценка: {r.Rating}", ForeColor = Color.Orange, Location = new Point(10, 30), AutoSize = true };
            var lText = new Label { Text = r.Comment, Location = new Point(10, 50), AutoSize = true, MaximumSize = new Size(480, 0) };
            
            p.Controls.Add(lName);
            p.Controls.Add(lRating);
            p.Controls.Add(lText);
            
            // Adjust height based on text
            if (lText.Height > 20) p.Height += lText.Height - 15;

            _reviewsPanel.Controls.Add(p);
        }
    }
}
