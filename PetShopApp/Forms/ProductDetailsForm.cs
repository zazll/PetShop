using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using PetShopApp.Controls;
using PetShopApp.Helpers;
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
    private PictureBox _mainPhoto;
    private FlowLayoutPanel _thumbsPanel;
    private FlowLayoutPanel _reviewsPanel;
    
    public ProductDetailsForm(Product product)
    {
        _product = product;
        _context = new PetShopContext();
        InitializeComponent();
        LoadProductData(); // Load photos and reviews
    }

    private void InitializeComponent()
    {
        this.Text = $"{_product.ProductName} — PetShop";
        this.Size = new Size(1100, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        // Split Container
        var split = new SplitContainer {
            Dock = DockStyle.Fill,
            SplitterDistance = 500,
            IsSplitterFixed = true,
            Orientation = Orientation.Vertical,
            Panel1 = { Padding = new Padding(20) },
            Panel2 = { Padding = new Padding(20) }
        };

        // --- Left: Gallery ---
        var galleryPanel = new Panel { Dock = DockStyle.Fill };
        
        _mainPhoto = new PictureBox {
            Dock = DockStyle.Top,
            Height = 400,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };
        
        _thumbsPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            AutoScroll = true
        };

        galleryPanel.Controls.Add(_thumbsPanel);
        galleryPanel.Controls.Add(_mainPhoto);
        split.Panel1.Controls.Add(galleryPanel);

        // --- Right: Info ---
        var rightPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
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
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            AutoSize = true,
            MaximumSize = new Size(500, 0)
        };
        
        // Stock Status
        var lblStock = new Label {
            Text = _product.ProductQuantityInStock > 0 ? "В наличии" : "Нет в наличии",
            ForeColor = _product.ProductQuantityInStock > 0 ? Color.Green : Color.Red,
            Font = new Font("Segoe UI", 10),
            Margin = new Padding(0, 5, 0, 10),
            AutoSize = true
        };

        // Price Block
        var pricePanel = new Panel { Size = new Size(500, 60), Margin = new Padding(0, 10, 0, 10) };
        decimal finalPrice = _product.ProductCost;
        if (_product.ProductDiscountAmount > 0)
        {
            finalPrice = _product.ProductCost * (1 - _product.ProductDiscountAmount.Value / 100m);
            var lblFinal = new Label { 
                Text = $"{finalPrice:N0} ₽", 
                Font = new Font("Segoe UI", 26, FontStyle.Bold), 
                ForeColor = Color.FromArgb(231, 76, 60), 
                AutoSize = true, 
                Location = new Point(0, 0) 
            };
            var lblOld = new Label { 
                Text = $"{_product.ProductCost:N0} ₽", 
                Font = new Font("Segoe UI", 14, FontStyle.Strikeout), 
                ForeColor = Color.Gray, 
                AutoSize = true, 
                Location = new Point(200, 15)
            };
            pricePanel.Controls.Add(lblFinal);
            pricePanel.Controls.Add(lblOld);
        }
        else
        {
            var lblFinal = new Label { 
                Text = $"{_product.ProductCost:N0} ₽", 
                Font = new Font("Segoe UI", 26, FontStyle.Bold), 
                ForeColor = Color.FromArgb(46, 204, 113), 
                AutoSize = true 
            };
            pricePanel.Controls.Add(lblFinal);
        }

        // Buy Button
        var btnBuy = new RoundedButton {
            Text = "Добавить в корзину",
            Size = new Size(250, 50),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Margin = new Padding(0, 10, 0, 20)
        };
        btnBuy.Click += (s, e) => {
            CartService.Instance.AddToCart(_product);
            MessageBox.Show("Товар добавлен в корзину");
        };

        // Description
        var lblDescTitle = new Label { Text = "О товаре", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
        var lblDesc = new Label { 
            Text = _product.ProductDescription ?? "Описание отсутствует", 
            Font = new Font("Segoe UI", 11), 
            AutoSize = true, 
            MaximumSize = new Size(520, 0),
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        
        // Supplier Info (Requirement: Full DB usage)
        var lblSupplier = new Label {
             Text = $"Поставщик: {_product.Supplier?.SupplierName ?? "Не указан"}",
             Font = new Font("Segoe UI", 9),
             ForeColor = Color.Gray,
             AutoSize = true,
             Margin = new Padding(0, 10, 0, 0)
        };

        // Reviews Section
        var lblReviewsTitle = new Label { Text = "Отзывы покупателей", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 30, 0, 5) };
        
        _reviewsPanel = new FlowLayoutPanel {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            MaximumSize = new Size(540, 0),
            WrapContents = false
        };

        var btnAddReview = new RoundedButton {
            Text = "Написать отзыв",
            Size = new Size(160, 35),
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        // Hack: Reset custom button style for secondary button
        // Or just use standard button for secondary
        var btnAddRevSimple = new Button {
            Text = "Написать отзыв",
            Size = new Size(160, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnAddRevSimple.Click += (s, e) => {
             new ReviewForm(_product).ShowDialog();
             LoadProductData(); // Refresh reviews
        };

        rightPanel.Controls.Add(lblBrand);
        rightPanel.Controls.Add(lblName);
        rightPanel.Controls.Add(lblStock);
        rightPanel.Controls.Add(pricePanel);
        rightPanel.Controls.Add(btnBuy);
        rightPanel.Controls.Add(lblDescTitle);
        rightPanel.Controls.Add(lblDesc);
        rightPanel.Controls.Add(lblSupplier);
        rightPanel.Controls.Add(lblReviewsTitle);
        rightPanel.Controls.Add(btnAddRevSimple);
        rightPanel.Controls.Add(_reviewsPanel);

        split.Panel2.Controls.Add(rightPanel);
        this.Controls.Add(split);
    }

    private void LoadProductData()
    {
        // Reload product to ensure it's tracked by THIS context
        _product = _context.Products
            .Include(p => p.Manufacturer)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefault(p => p.ProductID == _product.ProductID) ?? _product;

        // 1. Photos
        _context.Entry(_product).Collection(p => p.Photos).Load();

        _thumbsPanel.Controls.Clear();
        
        // Main photo logic
        string mainPath = "";
        if (_product.Photos.Any()) 
            mainPath = Path.Combine(Application.StartupPath, "Media", _product.Photos.First().PhotoPath);
        else if (!string.IsNullOrEmpty(_product.ProductPhoto))
            mainPath = Path.Combine(Application.StartupPath, "Media", _product.ProductPhoto);
            
        SetMainPhoto(mainPath);

        // Thumbnails
        var allPhotos = _product.Photos.Select(p => p.PhotoPath).ToList();
        if (allPhotos.Count == 0 && !string.IsNullOrEmpty(_product.ProductPhoto)) allPhotos.Add(_product.ProductPhoto);

        foreach (var p in allPhotos)
        {
            string fullPath = Path.Combine(Application.StartupPath, "Media", p);
            if (!File.Exists(fullPath)) continue;

            var pb = new PictureBox {
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(fullPath),
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };
            pb.Click += (s, e) => SetMainPhoto(fullPath);
            _thumbsPanel.Controls.Add(pb);
        }

        // 2. Reviews
        _reviewsPanel.Controls.Clear();
        var reviews = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductID == _product.ProductID)
            .OrderByDescending(r => r.ReviewDate)
            .ToList();

        if (!reviews.Any())
        {
            _reviewsPanel.Controls.Add(new Label { Text = "Отзывов пока нет", AutoSize = true, ForeColor = Color.Gray });
        }
        else
        {
            foreach (var r in reviews)
            {
                var p = new Panel { Size = new Size(500, 90), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10), BackColor = Color.FromArgb(252,252,252) };
                UIHelper.SetRoundedRegion(p, 10); // Rounded corners for review card
                
                var lName = new Label { Text = $"{r.User.UserSurname} {r.User.UserName}", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
                var lRating = new Label { Text = new string('★', r.Rating) + new string('☆', 5-r.Rating), ForeColor = Color.Orange, Location = new Point(10, 30), AutoSize = true, Font = new Font("Segoe UI", 12) };
                var lText = new Label { Text = r.Comment, Location = new Point(10, 55), AutoSize = true, MaximumSize = new Size(480, 0) };
                
                p.Controls.Add(lName);
                p.Controls.Add(lRating);
                p.Controls.Add(lText);
                
                _reviewsPanel.Controls.Add(p);
            }
        }
    }

    private void SetMainPhoto(string path)
    {
        if (File.Exists(path))
            _mainPhoto.Image = Image.FromFile(path);
        else 
        {
             Bitmap bmp = new Bitmap(400, 400);
             using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.WhiteSmoke); g.DrawString("Нет фото", this.Font, Brushes.Gray, 150, 180); }
             _mainPhoto.Image = bmp;
        }
    }
}