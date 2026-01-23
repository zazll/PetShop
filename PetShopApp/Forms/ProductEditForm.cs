using PetShopApp.Data;
using PetShopApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.IO;
using PetShopApp.Controls;
using PetShopApp.Helpers;
using PetShopApp.Services;

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
    
    // Images
    private FlowLayoutPanel _pnlPhotos;
    private List<string> _newPhotos = new();
    private List<ProductPhoto> _existingPhotosToDelete = new();

    public ProductEditForm(Product? product = null)
    {
        _product = product;
        _context = new PetShopContext();
        if (_product != null)
        {
            _context.Products.Attach(_product); // Attach the existing product to the new context
        }
        InitializeComponent();
        LoadDictionaries();
        if (_product != null) LoadProductData();
    }

    private void InitializeComponent()
    {
        this.Text = _product == null ? "Добавление товара" : "Редактирование товара";
        this.Size = new Size(900, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        // --- Helper to create labeled inputs ---
        Control CreateInput(string label, Control input, int x, int y, int w = 300)
        {
            var l = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            input.Location = new Point(x, y + 25);
            input.Width = w;
            input.Font = new Font("Segoe UI", 10);
            this.Controls.Add(l);
            this.Controls.Add(input);
            return input; 
        }

        int leftX = 20;
        int rightX = 350;
        int curY = 20;
        
        // Article
        txtArticle = new TextBox();
        CreateInput("Артикул:", txtArticle, leftX, curY);
        
        // Name
        txtName = new TextBox(); 
        CreateInput("Название:", txtName, rightX, curY, 400);

        curY += 70;

        // Category & Manufacturer
        cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        CreateInput("Категория:", cmbCategory, leftX, curY);

        cmbManufacturer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        CreateInput("Производитель:", cmbManufacturer, rightX, curY);
        
        // Supplier (Added as per requirement to use full DB)
        cmbSupplier = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        CreateInput("Поставщик:", cmbSupplier, rightX + 320, curY, 150);

        curY += 70;

        // Cost, Discount, Stock
        numCost = new NumericUpDown { Maximum = 1000000, DecimalPlaces = 2 };
        CreateInput("Цена (руб):", numCost, leftX, curY);

        numDiscount = new NumericUpDown { Maximum = 100 };
        CreateInput("Скидка (%):", numDiscount, rightX, curY, 100);

        numStock = new NumericUpDown { Maximum = 10000 };
        CreateInput("На складе:", numStock, rightX + 150, curY, 100);

        curY += 70;

        // Description
        txtDesc = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 100 };
        CreateInput("Описание:", txtDesc, leftX, curY, 730);

        curY += 140;

        // Photos Section
        var lblPhotos = new Label { Text = "Фотографии (до 6):", Location = new Point(leftX, curY), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        this.Controls.Add(lblPhotos);

        var btnAddPhoto = new RoundedButton { Text = "+ Фото", Width = 100, Height = 30, Location = new Point(leftX + 150, curY - 5) };
        btnAddPhoto.Click += BtnAddPhoto_Click;
        this.Controls.Add(btnAddPhoto);

        _pnlPhotos = new FlowLayoutPanel { 
            Location = new Point(leftX, curY + 30), 
            Size = new Size(840, 160), 
            AutoScroll = true, 
            BackColor = Color.WhiteSmoke,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(_pnlPhotos);

        // Save Button
        var btnSave = new RoundedButton { 
            Text = "Сохранить", 
            Location = new Point(700, 600), 
            Size = new Size(150, 45), 
            BackColor = Color.FromArgb(46, 204, 113)
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
        
        cmbSupplier.DataSource = _context.Suppliers.ToList();
        cmbSupplier.DisplayMember = "SupplierName";
        cmbSupplier.ValueMember = "SupplierID";
    }

    private void LoadProductData()
    {
        if (_product == null) return;
        
        // Eager load photos
        _context.Entry(_product).Collection(p => p.Photos).Load();

        txtArticle.Text = _product.ProductArticleNumber;
        txtName.Text = _product.ProductName;
        txtDesc.Text = _product.ProductDescription;
        numCost.Value = _product.ProductCost;
        numDiscount.Value = _product.ProductDiscountAmount ?? 0;
        numStock.Value = _product.ProductQuantityInStock;
        
        cmbCategory.SelectedValue = _product.CategoryID;
        cmbManufacturer.SelectedValue = _product.ManufacturerID;
        cmbSupplier.SelectedValue = _product.SupplierID;

        // Load existing photos
        foreach (var p in _product.Photos)
        {
            AddPhotoBox(p.PhotoPath, p);
        }
    }

    private void BtnAddPhoto_Click(object? sender, EventArgs e)
    {
        // Limit 6 logic
        if (_pnlPhotos.Controls.Count >= 6)
        {
            MessageBox.Show("Максимум 6 фотографий");
            return;
        }

        using (var ofd = new OpenFileDialog())
        {
            ofd.Filter = "Images|*.jpg;*.jpeg;*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _newPhotos.Add(ofd.FileName);
                AddPhotoBox(ofd.FileName, null);
            }
        }
    }

    private void AddPhotoBox(string pathOrName, ProductPhoto? dbPhoto)
    {
        string fullPath = dbPhoto == null ? pathOrName : Path.Combine(Application.StartupPath, "Media", pathOrName);
        
        if (!File.Exists(fullPath)) return;

        var pb = new PictureBox {
            Size = new Size(140, 140),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = Image.FromFile(fullPath),
            BackColor = Color.White,
            Margin = new Padding(5)
        };

        // Delete button overlay
        var btnDel = new Button {
            Text = "X",
            Size = new Size(25, 25),
            BackColor = Color.Red,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Parent = pb,
            Location = new Point(115, 0)
        };
        btnDel.FlatAppearance.BorderSize = 0;
        btnDel.Click += (s, e) => {
            _pnlPhotos.Controls.Remove(pb);
            if (dbPhoto != null) _existingPhotosToDelete.Add(dbPhoto);
            else _newPhotos.Remove(pathOrName);
        };

        _pnlPhotos.Controls.Add(pb);
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
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
            
            // Defaults
            _product.UnitDescription = "шт";
            _product.AnimalTypeID = _context.AnimalTypes.First().AnimalTypeID; 
        }
        else
        {
            _context.Attach(_product); 
        }

        _product.ProductArticleNumber = txtArticle.Text;
        _product.ProductName = txtName.Text;
        _product.ProductDescription = txtDesc.Text;
        _product.ProductCost = numCost.Value;
        _product.ProductDiscountAmount = (byte)numDiscount.Value;
        _product.ProductQuantityInStock = (int)numStock.Value;
        
        _product.CategoryID = (int)cmbCategory.SelectedValue!;
        _product.ManufacturerID = (int)cmbManufacturer.SelectedValue!;
        _product.SupplierID = (int)cmbSupplier.SelectedValue!;

        // Handle deletions
        if (_existingPhotosToDelete.Any())
        {
            _context.ProductPhotos.RemoveRange(_existingPhotosToDelete);
        }

        // Handle new photos
        foreach (var newPath in _newPhotos)
        {
            try {
                 string objectName = await MinioService.Instance.UploadFileAsync(newPath); // Upload to MinIO
                 
                 var newPhoto = new ProductPhoto {
                     PhotoPath = objectName, // Store MinIO object name
                     IsMain = !_product.Photos.Any() && _product.Photos.Count == 0 // First is main
                 };
                 _product.Photos.Add(newPhoto);
                 
                 // Backward compatibility for single photo column
                 if (string.IsNullOrEmpty(_product.ProductPhoto)) _product.ProductPhoto = objectName;
                 
            } catch (Exception ex) {
                MessageBox.Show("Ошибка сохранения фото в MinIO: " + ex.Message);
            }
        }
        
        // Update main photo ref if deleted
        if (_product.Photos.Any()) 
            _product.ProductPhoto = _product.Photos.First().PhotoPath;
        else 
            _product.ProductPhoto = null;

        try {
            _context.SaveChanges();
            MessageBox.Show("Товар сохранен!");
            this.Close();
        } catch (Exception ex) {
            MessageBox.Show("Ошибка БД: " + ex.Message);
        }
    }
}