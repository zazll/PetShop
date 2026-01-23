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
    private FlowLayoutPanel _mainContainer;
    
    public ProductDetailsForm(Product product)
    {
        _product = product;
        _context = new PetShopContext();
        InitializeComponent();
        LoadProductData(); 
    }

    private void InitializeComponent()
    {
        this.Text = $"{_product.ProductName} — PetShop";
        this.Size = new Size(1000, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
        this.AutoScroll = true;

        // Main Container (Vertical Scroll)
        _mainContainer = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            WrapContents = false,
            Padding = new Padding(40)
        };

        // 1. Gallery Section (Top)
        var galleryPanel = new FlowLayoutPanel { 
            Width = 900, 
            AutoSize = true, 
            FlowDirection = FlowDirection.TopDown, 
            WrapContents = false 
        };
        
        _mainPhoto = new PictureBox {
            Width = 800,
            Height = 450,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Margin = new Padding(50, 0, 0, 10)
        };
        
        _thumbsPanel = new FlowLayoutPanel {
            Width = 900,
            Height = 100,
            AutoScroll = true,
            Padding = new Padding(50, 0, 0, 0)
        };

        galleryPanel.Controls.Add(_mainPhoto);
        galleryPanel.Controls.Add(_thumbsPanel);
        _mainContainer.Controls.Add(galleryPanel);

        // 2. Info Section
        var infoPanel = new FlowLayoutPanel {
            Width = 900,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(50, 20, 0, 0)
        };

        // Brand
        var lblBrand = new Label {
            Text = _product.Manufacturer.ManufacturerName,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 12),
            AutoSize = true
        };
        infoPanel.Controls.Add(lblBrand);

        // Name
        var lblName = new Label {
            Text = _product.ProductName,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            AutoSize = true,
            MaximumSize = new Size(800, 0)
        };
        infoPanel.Controls.Add(lblName);

        // Rating
        var lblRating = new Label {
             Text = "Загрузка рейтинга...",
             Font = new Font("Segoe UI", 11),
             ForeColor = Color.Orange,
             AutoSize = true,
             Margin = new Padding(0, 5, 0, 15)
        };
        // Calc rating
        if (_product.Reviews != null && _product.Reviews.Any())
        {
            double avg = _product.Reviews.Average(r => r.Rating);
            lblRating.Text = $"★ {avg:N1} ({_product.Reviews.Count} отзывов)";
        }
        else
        {
             lblRating.Text = "Нет отзывов";
             lblRating.ForeColor = Color.Gray;
        }
        infoPanel.Controls.Add(lblRating);

        // Price Block
        var pricePanel = new Panel { Size = new Size(800, 60), Margin = new Padding(0, 10, 0, 10) };
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
                Font = new Font("Segoe UI", 16, FontStyle.Strikeout), 
                ForeColor = Color.Gray, 
                AutoSize = true, 
                Location = new Point(lblFinal.Right + 150, 12) 
            };
            // Need to add after layout to know width? No, AutoSize handles it, but location needs care.
            // Simplified positioning
            lblOld.Location = new Point(250, 12);

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
        infoPanel.Controls.Add(pricePanel);

        // Buy Button
        var btnBuy = new RoundedButton {
            Text = "Добавить в корзину",
            Size = new Size(300, 55),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Margin = new Padding(0, 10, 0, 30)
        };
        btnBuy.Click += (s, e) => {
            CartService.Instance.AddToCart(_product);
            MessageBox.Show("Товар добавлен в корзину");
        };
        infoPanel.Controls.Add(btnBuy);

        // Description
        var lblDescTitle = new Label { Text = "О товаре", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 10) };
        infoPanel.Controls.Add(lblDescTitle);
        
        var lblDesc = new Label { 
            Text = _product.ProductDescription ?? "Описание отсутствует", 
            Font = new Font("Segoe UI", 12), 
            AutoSize = true, 
            MaximumSize = new Size(800, 0),
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        infoPanel.Controls.Add(lblDesc);

        // Supplier
        var lblSupplier = new Label {
             Text = $"Поставщик: {_product.Supplier?.SupplierName ?? "Не указан"}",
             Font = new Font("Segoe UI", 10),
             ForeColor = Color.Gray,
             AutoSize = true,
             Margin = new Padding(0, 20, 0, 0)
        };
        infoPanel.Controls.Add(lblSupplier);

        _mainContainer.Controls.Add(infoPanel);

        // 3. Reviews Section
        var reviewsContainer = new FlowLayoutPanel {
             Width = 900,
             AutoSize = true,
             FlowDirection = FlowDirection.TopDown,
             WrapContents = false,
             Padding = new Padding(50, 40, 0, 20)
        };
        
        var lblReviewsTitle = new Label { Text = "Отзывы", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
        reviewsContainer.Controls.Add(lblReviewsTitle);

        var btnAddRev = new RoundedButton {
            Text = "Написать отзыв",
            Size = new Size(200, 40),
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        // Reset custom paint for white button border
        // Actually RoundedButton supports solid color.
        
        btnAddRev.Click += (s, e) => {
             new ReviewForm(_product).ShowDialog();
             LoadProductData(); 
        };
        reviewsContainer.Controls.Add(btnAddRev);
        
        _reviewsPanel = new FlowLayoutPanel {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Width = 800,
            Margin = new Padding(0, 20, 0, 0)
        };
        reviewsContainer.Controls.Add(_reviewsPanel);

        _mainContainer.Controls.Add(reviewsContainer);

        this.Controls.Add(_mainContainer);
    }

    private void LoadProductData()
    {
        // Reload product to ensure tracking
        _product = _context.Products
            .Include(p => p.Manufacturer)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Reviews).ThenInclude(r => r.User)
            .FirstOrDefault(p => p.ProductID == _product.ProductID) ?? _product;

        // Photos
        _context.Entry(_product).Collection(p => p.Photos).Load();

        // 1. Images
        _thumbsPanel.Controls.Clear();
        
        string mainPath = "";
        if (_product.Photos.Any()) 
            mainPath = _product.Photos.First().PhotoPath;
        else if (!string.IsNullOrEmpty(_product.ProductPhoto))
            mainPath = _product.ProductPhoto;
            
        SetMainPhoto(mainPath);

        var allPhotos = _product.Photos.Select(p => p.PhotoPath).ToList();
        if (allPhotos.Count == 0 && !string.IsNullOrEmpty(_product.ProductPhoto)) allPhotos.Add(_product.ProductPhoto);

        foreach (var p in allPhotos)
        {
            var pb = new PictureBox {
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            LoadImageIntoPb(p, pb);
            
            pb.Click += (s, e) => SetMainPhoto(p);
            _thumbsPanel.Controls.Add(pb);
        }

        // 2. Reviews
        _reviewsPanel.Controls.Clear();
        if (!_product.Reviews.Any())
        {
            _reviewsPanel.Controls.Add(new Label { Text = "Отзывов пока нет", AutoSize = true, ForeColor = Color.Gray });
        }
        else
        {
            foreach (var r in _product.Reviews.OrderByDescending(x => x.ReviewDate))
            {
                var p = new Panel { Size = new Size(600, 100), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10), BackColor = Color.FromArgb(250,250,250) };
                UIHelper.SetRoundedRegion(p, 10);
                
                var lName = new Label { Text = $"{r.User.UserSurname} {r.User.UserName}", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
                var lDate = new Label { Text = r.ReviewDate.ToShortDateString(), Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(500, 15), AutoSize = true };
                
                var lRating = new Label { Text = new string('★', r.Rating) + new string('☆', 5-r.Rating), ForeColor = Color.Orange, Location = new Point(15, 40), AutoSize = true, Font = new Font("Segoe UI", 12) };
                var lText = new Label { Text = r.Comment, Location = new Point(15, 65), AutoSize = true, MaximumSize = new Size(580, 0) };
                
                p.Controls.Add(lName);
                p.Controls.Add(lDate);
                p.Controls.Add(lRating);
                p.Controls.Add(lText);
                
                _reviewsPanel.Controls.Add(p);
            }
        }
    }

    private void SetMainPhoto(string path)
    {
        LoadImageIntoPb(path, _mainPhoto);
    }
    
    private async void LoadImageIntoPb(string path, PictureBox pb)
    {
        if (string.IsNullOrEmpty(path)) { LoadPlaceholder(pb); return; }

        // Local
        string fullPath = Path.Combine(Application.StartupPath, "Media", path);
        if (File.Exists(fullPath))
        {
            try {
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read)) { pb.Image = Image.FromStream(stream); }
                return;
            } catch {}
        }

        // MinIO
        try {
            string url = MinioService.Instance.GetFileUrl(path);
            pb.LoadAsync(url);
        } catch { LoadPlaceholder(pb); }
    }

    private void LoadPlaceholder(PictureBox pb)
    {
         Bitmap bmp = new Bitmap(200, 200);
         using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.WhiteSmoke); g.DrawString("Нет фото", new Font("Segoe UI", 10), Brushes.Gray, 60, 90); }
         pb.Image = bmp;
    }
}
