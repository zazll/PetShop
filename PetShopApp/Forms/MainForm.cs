using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;

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
        this.Text = "PetShop Marketplace - Главная";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosed += (s, e) => Application.Exit();
        this.BackColor = Color.WhiteSmoke;

        // Top Panel
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(10) };
        
        var lblLogo = new Label { Text = "PetShop", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.Purple, Location = new Point(10, 15), AutoSize = true };
        
        txtSearch = new TextBox { Location = new Point(180, 25), Width = 300, PlaceholderText = "Поиск товаров...", Font = new Font("Segoe UI", 12) };
        txtSearch.TextChanged += (s, e) => UpdateList();

        cmbSort = new ComboBox { Location = new Point(500, 25), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
        cmbSort.Items.AddRange(new string[] { "По умолчанию", "Цена: по возрастанию", "Цена: по убыванию" });
        cmbSort.SelectedIndex = 0;
        cmbSort.SelectedIndexChanged += (s, e) => UpdateList();

        cmbFilter = new ComboBox { Location = new Point(720, 25), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
        cmbFilter.Items.Add("Все категории");
        // Load categories later
        cmbFilter.SelectedIndex = 0;
        cmbFilter.SelectedIndexChanged += (s, e) => UpdateList();

        lblCount = new Label { Location = new Point(950, 30), AutoSize = true, Font = new Font("Segoe UI", 10) };

        topPanel.Controls.Add(lblLogo);
        topPanel.Controls.Add(txtSearch);
        topPanel.Controls.Add(cmbSort);
        topPanel.Controls.Add(cmbFilter);
        topPanel.Controls.Add(lblCount);

        // Bottom Panel (Controls)
        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.White, Padding = new Padding(10) };
        
        // Admin buttons
        btnAdd = new Button { Text = "Добавить товар", Location = new Point(10, 15), Width = 150, Height=40, Visible = false, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
        // btnAdd.Click += BtnAdd_Click; 
        
        btnDelete = new Button { Text = "Удалить", Location = new Point(170, 15), Width = 100, Height=40, Visible = false, BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat };
        btnDelete.Click += BtnDelete_Click;
        
        btnReports = new Button { Text = "Отчеты", Location = new Point(280, 15), Width = 100, Height=40, Visible = false, BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat };
        btnReports.Click += (s, e) => new ReportForm().ShowDialog();

        // User buttons
        btnBuy = new Button { Text = "В корзину", Location = new Point(950, 15), Width = 100, Height=40, BackColor = Color.Orange, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btnBuy.Click += BtnBuy_Click;

        btnReviews = new Button { Text = "Отзывы", Location = new Point(1060, 15), Width = 100, Height=40, BackColor = Color.CornflowerBlue, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
        btnReviews.Click += BtnReviews_Click;

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
            BackgroundColor = Color.WhiteSmoke,
            BorderStyle = BorderStyle.None,
            RowTemplate = { Height = 80 },
            AllowUserToAddRows = false
        };
        
        // Image Column
        var imgCol = new DataGridViewImageColumn { Name="Photo", HeaderText = "Фото", Width = 100, ImageLayout = DataGridViewImageCellLayout.Zoom };
        dgvProducts.Columns.Add(imgCol);

        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Название", Width = 250 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManufacturerName", HeaderText = "Бренд", Width = 150 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCost", HeaderText = "Цена", Width = 100, DefaultCellStyle = { Format = "C2", Font = new Font("Segoe UI", 10, FontStyle.Bold) } });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Discount", HeaderText = "Скидка", Width = 80 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductQuantityInStock", HeaderText = "Склад", Width = 80 });

        dgvProducts.CellFormatting += DgvProducts_CellFormatting;

        this.Controls.Add(dgvProducts);
        this.Controls.Add(topPanel);
        this.Controls.Add(bottomPanel);
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
            Discount = p.ProductDiscountAmount > 0 ? $"{p.ProductDiscountAmount}%" : "",
            p.ProductQuantityInStock
        }).ToList();

        dgvProducts.DataSource = displayList;
        lblCount.Text = $"Найдено: {result.Count}";
    }

    private void DgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dgvProducts.Rows[e.RowIndex];
        
        // Highlight expensive
        if (decimal.TryParse(row.Cells[3].Value?.ToString(), out decimal cost))
        {
             // Index 3 is Cost based on columns added above? 
             // Columns: 0=Photo, 1=Name, 2=Brand, 3=Price, 4=Discount, 5=Stock
             // Wait, Cost is index 3.
             if (cost > 1000)
             {
                 // row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); // Subtle green
                 // Highlighting specific cell is better
                 row.Cells[3].Style.ForeColor = Color.DarkGreen;
             }
        }

        // Image loading
        if (dgvProducts.Columns[e.ColumnIndex].Name == "Photo")
        {
            // Here we would load image
            // e.Value = Image.FromFile(...)
            // For now, let's leave default or null
        }
    }

    private void BtnBuy_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count == 0) return;
        MessageBox.Show("Товар добавлен в корзину! (Имитация)", "Корзина", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
             MessageBox.Show("Удаление запрещено в демо-режиме.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}