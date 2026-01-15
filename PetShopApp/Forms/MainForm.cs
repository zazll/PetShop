using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        this.Text = "PetShop - Каталог товаров";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosed += (s, e) => Application.Exit();

        // Top Panel
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        
        txtSearch = new TextBox { Location = new Point(10, 15), Width = 200, PlaceholderText = "Поиск..." };
        txtSearch.TextChanged += (s, e) => UpdateList();

        cmbSort = new ComboBox { Location = new Point(220, 15), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbSort.Items.AddRange(new string[] { "Без сортировки", "Цена: по возрастанию", "Цена: по убыванию" });
        cmbSort.SelectedIndex = 0;
        cmbSort.SelectedIndexChanged += (s, e) => UpdateList();

        cmbFilter = new ComboBox { Location = new Point(380, 15), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbFilter.Items.Add("Все категории");
        // Load categories later
        cmbFilter.SelectedIndex = 0;
        cmbFilter.SelectedIndexChanged += (s, e) => UpdateList();

        lblCount = new Label { Location = new Point(800, 15), AutoSize = true };

        topPanel.Controls.Add(txtSearch);
        topPanel.Controls.Add(cmbSort);
        topPanel.Controls.Add(cmbFilter);
        topPanel.Controls.Add(lblCount);

        // Bottom Panel (Admin/Manager controls)
        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        
        btnAdd = new Button { Text = "Добавить товар", Location = new Point(10, 10), Width = 120, Visible = false };
        // btnAdd.Click += BtnAdd_Click; 
        
        btnDelete = new Button { Text = "Удалить товар", Location = new Point(140, 10), Width = 120, Visible = false };
        btnDelete.Click += BtnDelete_Click;

        bottomPanel.Controls.Add(btnAdd);
        bottomPanel.Controls.Add(btnDelete);

        // Grid
        dgvProducts = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductPhoto", HeaderText = "Фото", Width = 100 }); // Placeholder for image
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Название", Width = 200 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductDescription", HeaderText = "Описание", Width = 250 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManufacturerName", HeaderText = "Производитель", Width = 150 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCost", HeaderText = "Цена", Width = 100 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductDiscountAmount", HeaderText = "Скидка %", Width = 80 });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductQuantityInStock", HeaderText = "На складе", Width = 80 });

        dgvProducts.CellFormatting += DgvProducts_CellFormatting;

        this.Controls.Add(dgvProducts);
        this.Controls.Add(topPanel);
        this.Controls.Add(bottomPanel);
    }

    private void ApplyRolePermissions()
    {
        var user = AuthService.CurrentUser;
        if (user != null && (user.Role.RoleName == "Администратор" || user.Role.RoleName == "Менеджер"))
        {
            btnAdd.Visible = true;
        }
        if (user != null && user.Role.RoleName == "Администратор")
        {
            btnDelete.Visible = true;
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
        
        // Custom binding to show Manufacturer Name flat
        var displayList = result.Select(p => new 
        {
            p.ProductID,
            p.ProductPhoto,
            p.ProductName,
            p.ProductDescription,
            ManufacturerName = p.Manufacturer.ManufacturerName,
            p.ProductCost,
            p.ProductDiscountAmount,
            p.ProductQuantityInStock
        }).ToList();

        dgvProducts.DataSource = displayList;
        lblCount.Text = $"Показано: {result.Count} из {_allProducts.Count}";
    }

    private void DgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dgvProducts.Rows[e.RowIndex];
        // Cost highlight
        if (decimal.TryParse(row.Cells[4].Value?.ToString(), out decimal cost))
        {
            if (cost > 1000)
            {
                row.DefaultCellStyle.BackColor = Color.LightGreen; // Highlight expensive items
            }
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count > 0)
        {
            // Logic for delete with constraints check
            int id = (int)dgvProducts.SelectedRows[0].Cells["ProductID"].Value; // Need to ensure ID is in DataSource
             // DataSource is anonymous object, access via reflection or dynamic if grid autogen.
             // But simpler: get from _allProducts by index? No, list is filtered.
             // I included ProductID in anonymous object.
             
             // ... Deletion logic implementation ...
             MessageBox.Show("Функция удаления (демо)");
        }
    }
}
