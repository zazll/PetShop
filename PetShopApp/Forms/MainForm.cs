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
    private Label _lblDidYouMean; // New: "Did you mean" suggestion label
    
    // Header controls
    private PictureBox _logoBox;
    private Panel _headerPanel;
    private Button btnProfile;
    private Button btnCart;
    private Label _lblCartCount; // New: Cart item count label
    
    private PetShopContext _context;
    private List<Product> _allProducts = new();
    
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113); 
    private readonly Color BackgroundColor = Color.FromArgb(249, 249, 249); 

    public MainForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
        LoadData();
        UpdateCartIndicator(); // Initialize cart indicator
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
             using (var path = UIHelper.GetRoundedPath(searchPanel.ClientRectangle, 20))
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
        btnCart.Click += (s, e) => {
             var form = new CartForm();
             form.FormClosed += (sender, args) => UpdateCartIndicator(); // Update indicator after CartForm closes
             form.ShowDialog();
        };

        _lblCartCount = new Label {
            Text = "0", // Initial count
            Location = new Point(btnCart.Location.X + btnCart.Width - 15, btnCart.Location.Y + 5), // Position as a badge
            BackColor = Color.Red,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Size = new Size(20, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0),
            Visible = false // Hide if cart is empty
        };
        UIHelper.SetRoundedRegion(_lblCartCount, 10); // Make it round

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
        _headerPanel.Controls.Add(_lblCartCount); // Add the new label to the header
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

        _lblDidYouMean = new Label {
            Text = "",
            Location = new Point(20, 50), // Position below filters bar
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            ForeColor = Color.Blue,
            Cursor = Cursors.Hand,
            Visible = false // Initially hidden
        };
        _lblDidYouMean.Click += (s, e) => {
            if (!string.IsNullOrEmpty(_lblDidYouMean.Tag as string))
            {
                txtSearch.Text = _lblDidYouMean.Tag as string;
            }
        };

        filterPanel.Controls.Add(cmbSort);
        filterPanel.Controls.Add(cmbFilter);
        filterPanel.Controls.Add(lblCount);
        filterPanel.Controls.Add(_lblDidYouMean); // Add to filter panel

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

        // "Did you mean" logic
        _lblDidYouMean.Visible = false; // Hide by default
        if (result.Count == 0 && !string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            string searchTerm = txtSearch.Text.ToLower();
            int minDistance = int.MaxValue;
            string? closestMatch = null;
            
            // Search through all product names for a suggestion
            foreach (var p in _allProducts)
            {
                int distance = LevenshteinDistance(searchTerm, p.ProductName.ToLower());
                // Only suggest if the distance is within a reasonable threshold (e.g., up to 30% of search term length)
                if (distance < minDistance && distance <= searchTerm.Length / 3) 
                {
                    minDistance = distance;
                    closestMatch = p.ProductName;
                }
            }

            if (closestMatch != null)
            {
                _lblDidYouMean.Text = $"Возможно, вы имели в виду: {closestMatch}?";
                _lblDidYouMean.Tag = closestMatch; // Store the actual suggested term
                _lblDidYouMean.Visible = true;
            }
        }

        foreach (var p in result)
        {
            var card = new ProductItem(p);
            card.OnBuyClick += (s, e) => {
                CartService.Instance.AddToCart(p);
                MessageBox.Show($"Товар '{p.ProductName}' добавлен в корзину");
                UpdateCartIndicator(); // Update indicator when item added
            };
            card.OnCardClick += (s, e) => {
                new ProductDetailsForm(p).ShowDialog();
            };
            
            // Context Menu for right click
            var cm = new ContextMenuStrip();
            
            // Analytics (For everyone or just staff? Let's allow staff)
            if (AuthService.CurrentUser?.Role.RoleName == "Администратор" || AuthService.CurrentUser?.Role.RoleName == "Менеджер")
            {
                cm.Items.Add("Аналитика товара", null, (s, e) => {
                    // Show simple analytics for this product
                    MessageBox.Show($"Продано: {_context.OrderProducts.Where(op => op.ProductID == p.ProductID).Sum(op => op.Quantity)} шт.", $"Аналитика: {p.ProductName}");
                });
                
                cm.Items.Add("-"); // Separator
                
                cm.Items.Add("Редактировать", null, (s, e) => {
                    var editForm = new ProductEditForm(p);
                    editForm.FormClosed += (sender, args) => LoadData();
                    editForm.ShowDialog();
                });
            }

            if (AuthService.CurrentUser?.Role.RoleName == "Администратор")
            {
                cm.Items.Add("Удалить", null, (s, e) => {
                     // Check F4
                     if (_context.OrderProducts.Any(op => op.ProductID == p.ProductID)) {
                         MessageBox.Show("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                         return;
                     }
                     if (MessageBox.Show($"Вы уверены, что хотите удалить '{p.ProductName}'?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                         _context.Products.Remove(p);
                         _context.SaveChanges();
                         LoadData();
                     }
                });
            }
            
            if (cm.Items.Count > 0) card.ContextMenuStrip = cm;

            _flowPanel.Controls.Add(card);
        }

        _flowPanel.ResumeLayout();
        UpdateCartIndicator(); // Call after list updates
    }

    // Levenshtein Distance implementation for "Did you mean"
    private int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
        {
            if (string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and left of the table
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }
        }
        return d[n, m];
    }

    private void UpdateCartIndicator()
    {
        int count = CartService.Instance.Items.Sum(x => x.Quantity);
        _lblCartCount.Text = count.ToString();
        _lblCartCount.Visible = count > 0;
    }
}

