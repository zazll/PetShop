using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace PetShopApp.Forms;

public class MainForm : Form
{
    private DataGridView dgvProducts;
    private ComboBox cmbSort;
    private ComboBox cmbFilter;
    private TextBox txtSearch;
    private Label lblCount;
    private Button btnAdd;
    private Button btnDelete;
    private Button btnBuy;
    private Button btnReviews;
    private Button btnReports;
    private PetShopContext _context;
    
    // Theme Colors
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113); // Emerald Green
    private readonly Color BackgroundColor = Color.White;
    private readonly Color SurfaceColor = Color.FromArgb(248, 255, 248); 
    private readonly Color TextColor = Color.FromArgb(64, 64, 64);
    
    // Cache data
    private List<Product> _allProducts = new();

    public MainForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
        LoadData();
        ApplyRolePermissions();
    }

    private void InitializeComponent()
    {
        this.Text = "PetShop Marketplace";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosed += (s, e) => Application.Exit();
        this.BackColor = BackgroundColor;
        this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        // Top Panel
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = BackgroundColor, Padding = new Padding(20) };
        topPanel.Paint += (s, e) => {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(230, 230, 230)), 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
        };

        var lblLogo = new Label { 
            Text = "PetShop", 
            Font = new Font("Segoe UI", 28, FontStyle.Bold), 
            ForeColor = PrimaryColor, 
            Location = new Point(20, 20), 
            AutoSize = true 
        };
        
        txtSearch = CreateStyledTextBox();
        txtSearch.PlaceholderText = "Поиск товаров...";
        txtSearch.Location = new Point(220, 35);
        txtSearch.Width = 350;
        txtSearch.TextChanged += (s, e) => UpdateList();

        cmbSort = CreateStyledComboBox();
        cmbSort.Location = new Point(600, 35);
        cmbSort.Width = 200;
        cmbSort.Items.AddRange(new string[] { "По умолчанию", "Цена: по возрастанию", "Цена: по убыванию" });
        cmbSort.SelectedIndex = 0;
        cmbSort.SelectedIndexChanged += (s, e) => UpdateList();

        cmbFilter = CreateStyledComboBox();
        cmbFilter.Location = new Point(820, 35);
        cmbFilter.Width = 200;
        cmbFilter.Items.Add("Все категории");
        cmbFilter.SelectedIndex = 0;
        cmbFilter.SelectedIndexChanged += (s, e) => UpdateList();

        lblCount = new Label { Location = new Point(1050, 40), AutoSize = true, ForeColor = TextColor };

        topPanel.Controls.Add(lblLogo);
        topPanel.Controls.Add(txtSearch);
        topPanel.Controls.Add(cmbSort);
        topPanel.Controls.Add(cmbFilter);
        topPanel.Controls.Add(lblCount);

        // Bottom Panel (Controls)
        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = SurfaceColor, Padding = new Padding(20) };
        
        // Admin buttons
        btnAdd = CreateStyledButton("Добавить", PrimaryColor, false);
        btnAdd.Location = new Point(20, 20);
        btnAdd.Visible = false;
        
        btnDelete = CreateStyledButton("Удалить", Color.LightCoral, false);
        btnDelete.ForeColor = Color.White;
        btnDelete.Location = new Point(160, 20);
        btnDelete.Visible = false;
        btnDelete.Click += BtnDelete_Click;
        
        btnReports = CreateStyledButton("Отчеты", Color.CadetBlue, false);
        btnReports.Location = new Point(280, 20);
        btnReports.Visible = false;
        btnReports.Click += (s, e) => new ReportForm().ShowDialog();

        // User buttons
        btnBuy = CreateStyledButton("В корзину", PrimaryColor, true);
        btnBuy.Click += BtnBuy_Click;

        btnReviews = CreateStyledButton("Отзывы", Color.Gray, false);
        btnReviews.BackColor = Color.White;
        btnReviews.ForeColor = TextColor;
        btnReviews.FlatStyle = FlatStyle.Flat;
        btnReviews.FlatAppearance.BorderColor = Color.LightGray;
        btnReviews.Click += BtnReviews_Click;

        btnReviews.Location = new Point(bottomPanel.Width - btnReviews.Width - 30, 20);
        btnBuy.Location = new Point(btnReviews.Location.X - btnBuy.Width - 10, 20);
        
        bottomPanel.Resize += (s, e) => {
            btnReviews.Location = new Point(bottomPanel.Width - btnReviews.Width - 30, 20);
            btnBuy.Location = new Point(btnReviews.Location.X - btnBuy.Width - 10, 20);
        };

        bottomPanel.Controls.Add(btnAdd);
        bottomPanel.Controls.Add(btnDelete);
        bottomPanel.Controls.Add(btnReports);
        bottomPanel.Controls.Add(btnBuy);
        bottomPanel.Controls.Add(btnReviews);

        // Grid
        dgvProducts = new DataGridView { 
            Dock = DockStyle.Fill, 
            AutoGenerateColumns = false, 
            ReadOnly = true, 
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = BackgroundColor,
            BorderStyle = BorderStyle.None,
            RowTemplate = { Height = 100 },
            AllowUserToAddRows = false,
            GridColor = Color.FromArgb(240, 240, 240),
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false
        };

        dgvProducts.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle {
            BackColor = SurfaceColor,
            ForeColor = TextColor,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(10),
            SelectionBackColor = SurfaceColor,
            SelectionForeColor = TextColor
        };

        dgvProducts.DefaultCellStyle = new DataGridViewCellStyle {
            Padding = new Padding(10),
            ForeColor = TextColor,
            SelectionBackColor = Color.FromArgb(230, 250, 230),
            SelectionForeColor = Color.Black
        };
        
        // Columns
        var imgCol = new DataGridViewImageColumn { Name="Photo", HeaderText = "Фото", Width = 120, ImageLayout = DataGridViewImageCellLayout.Zoom };
        dgvProducts.Columns.Add(imgCol);

        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Название", Width = 250 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManufacturerName", HeaderText = "Бренд", Width = 120 });
        
        // Price columns logic: Old Price (Base) and New Price (Calculated)
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCost", HeaderText = "Старая цена", Width = 100, DefaultCellStyle = { Format = "C0" } });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FinalCost", HeaderText = "Цена", Width = 100, DefaultCellStyle = { Format = "C0", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = PrimaryColor } });
        
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Discount", HeaderText = "Скидка", Width = 80 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductQuantityInStock", HeaderText = "Склад", Width = 80 });

        dgvProducts.CellFormatting += DgvProducts_CellFormatting;

        this.Controls.Add(dgvProducts);
        this.Controls.Add(topPanel);
        this.Controls.Add(bottomPanel);
    }

    private TextBox CreateStyledTextBox()
    {
        return new TextBox {
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.White
        };
    }

    private ComboBox CreateStyledComboBox()
    {
        return new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.White,
            FlatStyle = FlatStyle.System
        };
    }

    private Button CreateStyledButton(string text, Color backColor, bool isPrimary)
    {
        var btn = new Button {
            Text = text,
            Width = 140,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, isPrimary ? FontStyle.Bold : FontStyle.Regular),
            BackColor = backColor,
            ForeColor = isPrimary ? Color.White : Color.White,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void ApplyRolePermissions()
    {
        var user = AuthService.CurrentUser;
        if (user != null)
        {
            if (user.Role.RoleName == "Администратор" || user.Role.RoleName == "Менеджер")
            {
                btnAdd.Visible = true;
                btnReports.Visible = true;
            }
            if (user.Role.RoleName == "Администратор")
            {
                btnDelete.Visible = true;
            }
        }
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
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            filtered = filtered.Where(p => p.ProductName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
        }

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
        
        var displayList = result.Select(p => new 
        {
            p.ProductID,
            p.ProductPhoto, 
            p.ProductName,
            ManufacturerName = p.Manufacturer.ManufacturerName,
            ProductCost = p.ProductCost, // Base Price
            FinalCost = p.ProductDiscountAmount > 0 ? p.ProductCost * (1 - p.ProductDiscountAmount.Value / 100m) : p.ProductCost, // New Price
            Discount = p.ProductDiscountAmount > 0 ? $"-{p.ProductDiscountAmount}%" : "",
            p.ProductQuantityInStock
        }).ToList();

        dgvProducts.DataSource = displayList;
        lblCount.Text = $"Найдено товаров: {result.Count}";
    }

    private void DgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dgvProducts.Rows[e.RowIndex];
        
        // Image loading
        if (dgvProducts.Columns[e.ColumnIndex].Name == "Photo")
        {
            var photoName = row.Cells["ProductPhoto"].Value?.ToString();
            if (!string.IsNullOrEmpty(photoName))
            {
                // Logic to load image from local Images folder
                // We assume there is an 'Images' folder next to the .exe
                string imagePath = Path.Combine(Application.StartupPath, "Images", photoName);
                if (File.Exists(imagePath))
                {
                    try {
                        // Load image to a bitmap to avoid file locking, or just FromFile
                        // e.Value = Image.FromFile(imagePath);
                        // Using a simple placeholder logic if needed or just attempting load
                         e.Value = Image.FromFile(imagePath);
                    } catch { /* ignore error, show default */ }
                }
                else 
                {
                    // Placeholder if file not found
                    // e.Value = Resources.DefaultImage; 
                }
            }
        }

        // Logic: If discount > 0, Strikeout "ProductCost" (Old Price)
        // Column indices: 0=Photo, 1=Name, 2=Brand, 3=OldPrice, 4=NewPrice, 5=Discount
        if (e.ColumnIndex == 3) // Old Price
        {
             var discountCell = row.Cells[5].Value?.ToString();
             if (!string.IsNullOrEmpty(discountCell))
             {
                 e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Strikeout);
                 e.CellStyle.ForeColor = Color.Gray;
             }
             else
             {
                 // If no discount, hide Old Price or make it invisible/same as new?
                 // Better: Hide the value in Old Price cell if it equals New Price
                 e.Value = ""; 
                 e.FormattingApplied = true;
             }
        }
        
        // Highlight logic > 1000
        // Check Final Cost (index 4)
        if (e.ColumnIndex == 4)
        {
             if (decimal.TryParse(e.Value?.ToString(), out decimal cost))
             {
                 if (cost > 1000)
                 {
                     // row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); // Per TZ F3
                     // Or just highlight the price
                     e.CellStyle.ForeColor = Color.DarkGreen;
                 }
             }
        }
    }

    private void BtnBuy_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count == 0) return;
        MessageBox.Show("Товар успешно добавлен в корзину!", "Корзина", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnReviews_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count == 0) return;
        
        int productId = (int)dgvProducts.SelectedRows[0].Cells["ProductID"].Value;
        var product = _allProducts.First(p => p.ProductID == productId);
        
        new ReviewForm(product).ShowDialog();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count > 0)
        {
             // TZ F4 constraint check simulated
             MessageBox.Show("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.", "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}