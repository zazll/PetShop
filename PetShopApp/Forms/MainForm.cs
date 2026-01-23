using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using PetShopApp.Controls;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PetShopApp.Forms;

public class MainForm : Form
{
    private FlowLayoutPanel _flowPanel;
    private ComboBox cmbSort;
    private ComboBox cmbFilter;
    private TextBox txtSearch;
    private Label lblCount;
    
    // Header controls
    private PictureBox _logoBox;
    private Panel _headerPanel;
    
    private PetShopContext _context;
    private List<Product> _allProducts = new();
    
    // Theme
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113); 
    private readonly Color BackgroundColor = Color.FromArgb(249, 249, 249); 

    public MainForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
        LoadData();
        // CheckUserRole();
    }

    private void InitializeComponent()
    {
        this.Text = "PetShop Marketplace";
        this.Size = new Size(1280, 800); // Widescreen
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
        // Bottom border for header
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
        // Try load logo
        LoadLogo();
        
        // Search Bar (Center)
        var searchPanel = new Panel {
            Size = new Size(500, 45),
            Location = new Point(250, 18),
            BackColor = Color.FromArgb(46, 204, 113), // Green border
            Padding = new Padding(2)
        };
        
        var searchInner = new Panel {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        txtSearch = new TextBox {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 12),
            Location = new Point(10, 10),
            Width = 400,
            PlaceholderText = "Искать на PetShop..."
        };
        txtSearch.TextChanged += (s, e) => UpdateList();
        
        var btnSearch = new Button {
            Dock = DockStyle.Right,
            Width = 60,
            FlatStyle = FlatStyle.Flat,
            BackColor = PrimaryColor,
            Image = null, // Could add icon
            Text = "🔍",
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnSearch.FlatAppearance.BorderSize = 0;

        searchInner.Controls.Add(txtSearch);
        searchInner.Controls.Add(btnSearch);
        searchPanel.Controls.Add(searchInner);

        // Header Buttons (Reports, Login info, etc)
        var btnReports = new Button {
            Text = "Отчеты",
            Location = new Point(800, 20),
            Height = 40,
            Width = 100,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        btnReports.Click += (s, e) => new ReportForm().ShowDialog();
        if (AuthService.CurrentUser?.Role.RoleName != "Администратор" && AuthService.CurrentUser?.Role.RoleName != "Менеджер")
            btnReports.Visible = false;


        _headerPanel.Controls.Add(_logoBox);
        _headerPanel.Controls.Add(searchPanel);
        _headerPanel.Controls.Add(btnReports);
        
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
        _flowPanel.SizeChanged += (s, e) => AdjustGrid();

        this.Controls.Add(_flowPanel);
        this.Controls.Add(filterPanel);
        this.Controls.Add(_headerPanel);
    }

    private void LoadLogo()
    {
        // Logic to try loading "logo.png" or similar from Media
        try {
            string path = Path.Combine(Application.StartupPath, "Media", "logo_company.png"); // The "cutout" one likely
            if (File.Exists(path)) 
                _logoBox.Image = Image.FromFile(path);
            else {
                // Draw text logo if missing
                Bitmap bmp = new Bitmap(180, 60);
                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.Clear(Color.White);
                    g.DrawString("PetShop", new Font("Segoe UI", 24, FontStyle.Bold), new SolidBrush(PrimaryColor), 10, 10);
                }
                _logoBox.Image = bmp;
            }
        } catch {}
    }

    private void AdjustGrid()
    {
        // Optional: Dynamically center content or adjust margins
    }

    private void LoadData()
    {
        _allProducts = _context.Products
            .Include(p => p.Manufacturer)
            .Include(p => p.Category)
            .ToList();

        var categories = _context.ProductCategories.ToList();
        foreach (var c in categories)
        {
            cmbFilter.Items.Add(c.CategoryName);
        }

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
                MessageBox.Show($"Товар '{p.ProductName}' добавлен в корзину", "Корзина", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            card.OnCardClick += (s, e) => {
                // Open Details/Reviews
                new ReviewForm(p).ShowDialog();
            };
            _flowPanel.Controls.Add(card);
        }

        _flowPanel.ResumeLayout();
    }
}
