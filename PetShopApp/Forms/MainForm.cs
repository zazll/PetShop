using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using PetShopApp.Controls;
using PetShopApp.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace PetShopApp.Forms;

public class MainForm : Form
{
    private FlowLayoutPanel _flowPanel;
    private ComboBox cmbSort;
    private ComboBox cmbFilter;
    private TextBox txtSearch;
    private Label lblCount;
    private Button btnAdd;
    
    // Header controls
    private PictureBox _logoBox;
    private Panel _headerPanel;
    private Button btnProfile;
    private Button btnCart;
    
    private PetShopContext _context;
    private List<Product> _allProducts = new();
    
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113); 
    private readonly Color BackgroundColor = Color.FromArgb(249, 249, 249); 

    public MainForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "PetShop Marketplace";
        this.Size = new Size(1280, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosed += (s, e) => Application.Exit();
        this.BackColor = BackgroundColor;
        this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        // --- Header ---
        _headerPanel = new Panel { 
            Dock = DockStyle.Top, 
            Height = 80, 
            BackColor = Color.White, 
            Padding = new Padding(20, 10, 20, 10) 
        };
        _headerPanel.Paint += (s, e) => {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(230, 230, 230)), 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
        };

        // Logo
        _logoBox = new PictureBox {
            Size = new Size(180, 60),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(20, 10),
            Cursor = Cursors.Hand
        };
        LoadLogo();
        
        // Search Bar (Center) - Rounded
        var searchPanel = new Panel {
            Size = new Size(400, 45),
            Location = new Point(250, 18),
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        // Manual draw for rounded border
        searchPanel.Paint += (s, e) => {
             e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
             using (var pen = new Pen(PrimaryColor, 2))
             using (var path = GetRoundedPath(searchPanel.ClientRectangle, 20))
             {
                 e.Graphics.DrawPath(pen, path);
             }
        };

        txtSearch = new TextBox {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 12),
            Location = new Point(15, 12),
            Width = 330,
            PlaceholderText = "Искать на PetShop..."
        };
        txtSearch.TextChanged += (s, e) => UpdateList();
        
        var btnSearch = new Label { // Use label as icon
            Text = "🔍",
            Location = new Point(360, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 12),
            Cursor = Cursors.Hand,
            ForeColor = PrimaryColor
        };

        searchPanel.Controls.Add(txtSearch);
        searchPanel.Controls.Add(btnSearch);

        // Header Buttons (Right)
        int btnX = 700;
        
        var btnReports = new RoundedButton {
            Text = "Отчеты",
            Location = new Point(btnX, 20),
            Width = 100,
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        btnReports.Click += (s, e) => new ReportForm().ShowDialog();
        
        btnAdd = new RoundedButton {
            Text = "+ Товар",
            Location = new Point(btnX + 110, 20),
            Width = 100,
            BackColor = Color.White,
            ForeColor = Color.Green
        };
        btnAdd.Click += (s, e) => {
             var form = new ProductEditForm();
             form.FormClosed += (sender, args) => LoadData();
             form.ShowDialog();
        };

        if (AuthService.CurrentUser?.Role.RoleName != "Администратор" && AuthService.CurrentUser?.Role.RoleName != "Менеджер")
        {
            btnReports.Visible = false;
            btnAdd.Visible = false;
        }

        btnCart = new RoundedButton {
            Text = "Корзина",
            Location = new Point(1050, 20),
            Width = 100,
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        btnCart.Click += (s, e) => new CartForm().ShowDialog();

        btnProfile = new RoundedButton {
            Text = "Профиль",
            Location = new Point(1160, 20),
            Width = 100,
            BackColor = PrimaryColor,
            ForeColor = Color.White
        };
        btnProfile.Click += (s, e) => new UserProfileForm().ShowDialog();

        _headerPanel.Controls.Add(_logoBox);
        _headerPanel.Controls.Add(searchPanel);
        _headerPanel.Controls.Add(btnReports);
        _headerPanel.Controls.Add(btnAdd);
        _headerPanel.Controls.Add(btnCart);
        _headerPanel.Controls.Add(btnProfile);
        
        // --- Filters Bar ---
        var filterPanel = new Panel {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White
        };
        
        cmbSort = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(20, 12),
            Width = 200,
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat
        };
        cmbSort.Items.AddRange(new string[] { "По популярности", "Сначала дешевле", "Сначала дороже" });
        cmbSort.SelectedIndex = 0;
        cmbSort.SelectedIndexChanged += (s, e) => UpdateList();

        cmbFilter = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(240, 12),
            Width = 200,
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat
        };
        cmbFilter.Items.Add("Все категории");
        cmbFilter.SelectedIndex = 0;
        cmbFilter.SelectedIndexChanged += (s, e) => UpdateList();

        lblCount = new Label {
            AutoSize = true,
            Location = new Point(500, 15),
            ForeColor = Color.Gray
        };

        filterPanel.Controls.Add(cmbSort);
        filterPanel.Controls.Add(cmbFilter);
        filterPanel.Controls.Add(lblCount);

        // --- Main Content (Grid) ---
        _flowPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
            BackColor = BackgroundColor
        };

        this.Controls.Add(_flowPanel);
        this.Controls.Add(filterPanel);
        this.Controls.Add(_headerPanel);
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

    private void LoadLogo()
    {
        try {
            string path = Path.Combine(Application.StartupPath, "Media", "logo_company.png");
            if (File.Exists(path)) 
                _logoBox.Image = Image.FromFile(path);
            else {
                Bitmap bmp = new Bitmap(180, 60);
                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.Clear(Color.White);
                    g.DrawString("PetShop", new Font("Segoe UI", 24, FontStyle.Bold), new SolidBrush(PrimaryColor), 10, 10);
                }
                _logoBox.Image = bmp;
            }
        } catch {}
    }

    private void LoadData()
    {
        // Reload context to get fresh data
        _context = new PetShopContext();
        
        _allProducts = _context.Products
            .Include(p => p.Manufacturer)
            .Include(p => p.Category)
            .Include(p => p.Photos) // Load photos
            .Include(p => p.Reviews) // Load reviews for rating
            .ToList();

        var categories = _context.ProductCategories.ToList();
        cmbFilter.Items.Clear();
        cmbFilter.Items.Add("Все категории");
        foreach (var c in categories)
        {
            cmbFilter.Items.Add(c.CategoryName);
        }
        cmbFilter.SelectedIndex = 0;

        UpdateList();
    }

    private void UpdateList()
    {
        _flowPanel.SuspendLayout();
        _flowPanel.Controls.Clear();

        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            filtered = filtered.Where(p => p.ProductName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));

        if (cmbFilter.SelectedIndex > 0)
        {
            string cat = cmbFilter.SelectedItem.ToString()!;
            filtered = filtered.Where(p => p.Category.CategoryName == cat);
        }

        switch (cmbSort.SelectedIndex)
        {
            case 1: filtered = filtered.OrderBy(p => p.ProductCost); break;
            case 2: filtered = filtered.OrderByDescending(p => p.ProductCost); break;
        }

        var result = filtered.ToList();
        lblCount.Text = $"{result.Count} товаров";

        foreach (var p in result)
        {
            var card = new ProductItem(p);
            card.OnBuyClick += (s, e) => {
                CartService.Instance.AddToCart(p);
                MessageBox.Show($"Товар '{p.ProductName}' добавлен в корзину");
            };
            card.OnCardClick += (s, e) => {
                new ProductDetailsForm(p).ShowDialog();
            };
            
            if (AuthService.CurrentUser?.Role.RoleName == "Администратор")
            {
                var cm = new ContextMenuStrip();
                cm.Items.Add("Редактировать", null, (s, e) => {
                    var editForm = new ProductEditForm(p);
                    editForm.FormClosed += (sender, args) => LoadData();
                    editForm.ShowDialog();
                });
                cm.Items.Add("Удалить", null, (s, e) => {
                     if (_context.OrderProducts.Any(op => op.ProductID == p.ProductID)) {
                         MessageBox.Show("Невозможно удалить товар (есть в заказах)");
                         return;
                     }
                     if (MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                         _context.Products.Remove(p);
                         _context.SaveChanges();
                         LoadData();
                     }
                });
                card.ContextMenuStrip = cm;
            }

            _flowPanel.Controls.Add(card);
        }

        _flowPanel.ResumeLayout();
    }
}
