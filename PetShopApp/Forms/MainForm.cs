using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

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
    private readonly Color SecondaryColor = Color.FromArgb(39, 174, 96); // Darker Green
    private readonly Color BackgroundColor = Color.White;
    private readonly Color SurfaceColor = Color.FromArgb(248, 255, 248); // Very light green tint
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
        // Add a subtle border at the bottom of top panel
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
        // Load categories later
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
        // btnAdd.Click += BtnAdd_Click; 
        
        btnDelete = CreateStyledButton("Удалить", Color.LightCoral, false); // Keep red for danger, but softer
        btnDelete.ForeColor = Color.White;
        btnDelete.Location = new Point(160, 20);
        btnDelete.Visible = false;
        btnDelete.Click += BtnDelete_Click;
        
        btnReports = CreateStyledButton("Отчеты", Color.CadetBlue, false);
        btnReports.Location = new Point(280, 20);
        btnReports.Visible = false;
        btnReports.Click += (s, e) => new ReportForm().ShowDialog();

        // User buttons
        // Right align logic
        int btnWidth = 140;
        int rightMargin = 20;
        int btnSpacing = 10;
        int startX = this.ClientSize.Width - rightMargin - btnWidth;

        btnBuy = CreateStyledButton("В корзину", PrimaryColor, true);
        btnBuy.Location = new Point(950, 20); // Will adjust on resize if needed, but fixed for now
        btnBuy.Click += BtnBuy_Click;

        btnReviews = CreateStyledButton("Отзывы", Color.Gray, false); // Neutral color
        btnReviews.Location = new Point(1060, 20); // Will adjust
        btnReviews.BackColor = Color.White;
        btnReviews.ForeColor = TextColor;
        btnReviews.FlatStyle = FlatStyle.Flat;
        btnReviews.FlatAppearance.BorderColor = Color.LightGray;
        btnReviews.Click += BtnReviews_Click;

        // Position user buttons correctly
        btnReviews.Location = new Point(bottomPanel.Width - btnReviews.Width - 30, 20);
        btnBuy.Location = new Point(btnReviews.Location.X - btnBuy.Width - 10, 20);
        
        // Handle Resize for buttons
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
            SelectionBackColor = Color.FromArgb(230, 250, 230), // Very light green selection
            SelectionForeColor = Color.Black
        };
        
        // Image Column
        var imgCol = new DataGridViewImageColumn { Name="Photo", HeaderText = "Фото", Width = 120, ImageLayout = DataGridViewImageCellLayout.Zoom };
        dgvProducts.Columns.Add(imgCol);

        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Название", Width = 300 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManufacturerName", HeaderText = "Бренд", Width = 150 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCost", HeaderText = "Цена", Width = 120, DefaultCellStyle = { Format = "C2", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = PrimaryColor } });
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

        // Search
        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            filtered = filtered.Where(p => p.ProductName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
        }

        // Filter
        if (cmbFilter.SelectedIndex > 0)
        {
            string cat = cmbFilter.SelectedItem.ToString()!;
            filtered = filtered.Where(p => p.Category.CategoryName == cat);
        }

        // Sort
        switch (cmbSort.SelectedIndex)
        {
            case 1: filtered = filtered.OrderBy(p => p.ProductCost); break;
            case 2: filtered = filtered.OrderByDescending(p => p.ProductCost); break;
        }

        var result = filtered.ToList();
        
        // Custom binding
        var displayList = result.Select(p => new 
        {
            p.ProductID,
            p.ProductPhoto, // filename
            p.ProductName,
            ManufacturerName = p.Manufacturer.ManufacturerName,
            p.ProductCost,
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
        
        // Highlight expensive
        if (decimal.TryParse(row.Cells[3].Value?.ToString(), out decimal cost))
        {
             // Cost formatting is handled by DefaultCellStyle
        }
        
        // Discount color
        if (dgvProducts.Columns[e.ColumnIndex].HeaderText == "Скидка" && e.Value != null && e.Value.ToString() != "")
        {
            e.CellStyle.ForeColor = Color.Red;
            e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        // Image loading placeholder
        if (dgvProducts.Columns[e.ColumnIndex].Name == "Photo")
        {
             // Placeholder logic
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
             MessageBox.Show("Функция удаления недоступна в демо-режиме.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
