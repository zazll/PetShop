using PetShopApp.Data;
using PetShopApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.IO;

namespace PetShopApp.Forms;

public class ProductEditForm : Form
{
    private Product? _product;
    private PetShopContext _context;
    
    // Controls
    private TextBox txtArticle;
    private TextBox txtName;
    private NumericUpDown numCost;
    private NumericUpDown numDiscount;
    private NumericUpDown numStock;
    private ComboBox cmbCategory;
    private ComboBox cmbManufacturer;
    private ComboBox cmbSupplier;
    private TextBox txtDesc;
    private PictureBox pbxPhoto;
    private string? _selectedPhotoPath;

    public ProductEditForm(Product? product = null)
    {
        _product = product;
        _context = new PetShopContext();
        InitializeComponent();
        LoadDictionaries();
        if (_product != null) LoadProductData();
    }

    private void InitializeComponent()
    {
        this.Text = _product == null ? "Добавление товара" : "Редактирование товара";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowStyles = { new RowStyle(SizeType.AutoSize) } };
        
        // --- Helper to create labeled inputs ---
        Control CreateInput(string label, Control input, int y)
        {
            var l = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            input.Location = new Point(20, y + 25);
            input.Width = 300;
            input.Font = new Font("Segoe UI", 10);
            this.Controls.Add(l);
            this.Controls.Add(input);
            return input; // return for ref
        }

        int curY = 20;
        
        // Article
        txtArticle = new TextBox();
        CreateInput("Артикул:", txtArticle, curY);
        
        // Name
        txtName = new TextBox(); 
        var lName = new Label { Text = "Название:", Location = new Point(350, curY), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        txtName.Location = new Point(350, curY + 25);
        txtName.Width = 400;
        txtName.Font = new Font("Segoe UI", 10);
        this.Controls.Add(lName);
        this.Controls.Add(txtName);

        curY += 70;

        // Category & Manufacturer
        cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        CreateInput("Категория:", cmbCategory, curY);

        cmbManufacturer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        var lMan = new Label { Text = "Производитель:", Location = new Point(350, curY), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        cmbManufacturer.Location = new Point(350, curY + 25);
        cmbManufacturer.Width = 300;
        this.Controls.Add(lMan);
        this.Controls.Add(cmbManufacturer);

        curY += 70;

        // Cost, Discount, Stock
        numCost = new NumericUpDown { Maximum = 1000000, DecimalPlaces = 2 };
        CreateInput("Цена (руб):", numCost, curY);

        numDiscount = new NumericUpDown { Maximum = 100 };
        var lDisc = new Label { Text = "Скидка (%):", Location = new Point(350, curY), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        numDiscount.Location = new Point(350, curY + 25);
        numDiscount.Width = 100;
        this.Controls.Add(lDisc);
        this.Controls.Add(numDiscount);

        numStock = new NumericUpDown { Maximum = 10000 };
        var lStock = new Label { Text = "На складе:", Location = new Point(500, curY), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        numStock.Location = new Point(500, curY + 25);
        numStock.Width = 100;
        this.Controls.Add(lStock);
        this.Controls.Add(numStock);

        curY += 70;

        // Image
        var btnPhoto = new Button { Text = "Выбрать фото...", Location = new Point(20, curY), Width = 150 };
        btnPhoto.Click += BtnPhoto_Click;
        this.Controls.Add(btnPhoto);

        pbxPhoto = new PictureBox { Location = new Point(20, curY + 40), Size = new Size(150, 150), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        this.Controls.Add(pbxPhoto);

        // Description
        var lDesc = new Label { Text = "Описание:", Location = new Point(350, curY), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        txtDesc = new TextBox { Location = new Point(350, curY + 25), Width = 400, Height = 150, Multiline = true, ScrollBars = ScrollBars.Vertical };
        this.Controls.Add(lDesc);
        this.Controls.Add(txtDesc);

        // Save Button
        var btnSave = new Button { 
            Text = "Сохранить", 
            Location = new Point(600, 500), 
            Size = new Size(150, 40), 
            BackColor = Color.FromArgb(46, 204, 113), 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);
    }

    private void LoadDictionaries()
    {
        cmbCategory.DataSource = _context.ProductCategories.ToList();
        cmbCategory.DisplayMember = "CategoryName";
        cmbCategory.ValueMember = "CategoryID";

        cmbManufacturer.DataSource = _context.Manufacturers.ToList();
        cmbManufacturer.DisplayMember = "ManufacturerName";
        cmbManufacturer.ValueMember = "ManufacturerID";
        
        // Default Supplier (hidden for simplicity in this demo)
        // In real app, add ComboBox for Supplier too
    }

    private void LoadProductData()
    {
        if (_product == null) return;
        
        txtArticle.Text = _product.ProductArticleNumber;
        txtName.Text = _product.ProductName;
        txtDesc.Text = _product.ProductDescription;
        numCost.Value = _product.ProductCost;
        numDiscount.Value = _product.ProductDiscountAmount ?? 0;
        numStock.Value = _product.ProductQuantityInStock;
        
        cmbCategory.SelectedValue = _product.CategoryID;
        cmbManufacturer.SelectedValue = _product.ManufacturerID;

        if (!string.IsNullOrEmpty(_product.ProductPhoto))
        {
            string path = Path.Combine(Application.StartupPath, "Media", _product.ProductPhoto);
            if (File.Exists(path)) pbxPhoto.Image = Image.FromFile(path);
        }
    }

    private void BtnPhoto_Click(object? sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog())
        {
            ofd.Filter = "Images|*.jpg;*.jpeg;*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbxPhoto.Image = Image.FromFile(ofd.FileName);
                _selectedPhotoPath = ofd.FileName;
            }
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtArticle.Text))
        {
            MessageBox.Show("Заполните обязательные поля (Артикул, Название)");
            return;
        }

        if (_product == null)
        {
            _product = new Product();
            _context.Products.Add(_product);
            
            // Default fields required by DB
            _product.UnitDescription = "шт";
            _product.SupplierID = _context.Suppliers.First().SupplierID; // Take first available
            _product.AnimalTypeID = _context.AnimalTypes.First().AnimalTypeID; // Take first available
        }
        else
        {
            _context.Attach(_product); // Re-attach if lost context (though we keep _context alive)
        }

        _product.ProductArticleNumber = txtArticle.Text;
        _product.ProductName = txtName.Text;
        _product.ProductDescription = txtDesc.Text;
        _product.ProductCost = numCost.Value;
        _product.ProductDiscountAmount = (byte)numDiscount.Value;
        _product.ProductQuantityInStock = (int)numStock.Value;
        
        _product.CategoryID = (int)cmbCategory.SelectedValue!;
        _product.ManufacturerID = (int)cmbManufacturer.SelectedValue!;

        // Save Image
        if (_selectedPhotoPath != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(_selectedPhotoPath);
            string destPath = Path.Combine(Application.StartupPath, "Media", fileName);
            try {
                if (!Directory.Exists(Path.GetDirectoryName(destPath))) Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                File.Copy(_selectedPhotoPath, destPath, true);
                _product.ProductPhoto = fileName;
            } catch (Exception ex) {
                MessageBox.Show("Ошибка сохранения фото: " + ex.Message);
            }
        }

        try {
            _context.SaveChanges();
            MessageBox.Show("Товар сохранен!");
            this.Close();
        } catch (Exception ex) {
            MessageBox.Show("Ошибка БД: " + ex.Message);
        }
    }
}
