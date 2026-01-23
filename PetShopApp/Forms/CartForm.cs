using PetShopApp.Models;
using PetShopApp.Services;
using PetShopApp.Controls;
using PetShopApp.Data;
using System.Drawing;
using System.Data;

namespace PetShopApp.Forms;

public class CartForm : Form
{
    private FlowLayoutPanel _panelItems;
    private Label _lblTotal;
    private RoundedButton _btnCheckout;
    private ComboBox _cmbPickupPoint;
    private PetShopContext _context;

    public CartForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
        LoadItems();
    }

    private void InitializeComponent()
    {
        this.Text = "Корзина";
        this.Size = new Size(800, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        var title = new Label { Text = "Ваша корзина", Font = new Font("Segoe UI", 20, FontStyle.Bold), Dock = DockStyle.Top, Height = 60, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20,0,0,0) };
        
        _panelItems = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(20),
            WrapContents = false
        };

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 130, BackColor = Color.WhiteSmoke };
        
        // Pickup Point Selector
        var lblPickup = new Label { Text = "Пункт выдачи:", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        _cmbPickupPoint = new ComboBox { Location = new Point(130, 12), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        LoadPickupPoints();

        _lblTotal = new Label { 
            Text = "Итого: 0 ₽", 
            Font = new Font("Segoe UI", 16, FontStyle.Bold), 
            Location = new Point(20, 60), 
            AutoSize = true 
        };
        
        _btnCheckout = new RoundedButton {
            Text = "Оформить заказ",
            Location = new Point(550, 50),
            Size = new Size(200, 50),
            BackColor = Color.FromArgb(46, 204, 113)
        };
        _btnCheckout.Click += BtnCheckout_Click;

        bottomPanel.Controls.Add(lblPickup);
        bottomPanel.Controls.Add(_cmbPickupPoint);
        bottomPanel.Controls.Add(_lblTotal);
        bottomPanel.Controls.Add(_btnCheckout);

        this.Controls.Add(_panelItems);
        this.Controls.Add(bottomPanel);
        this.Controls.Add(title);
    }

    private void LoadPickupPoints()
    {
        var points = _context.PickupPoints.ToList();
        // Format for display: "City, Street, House"
        var displayList = points.Select(p => new { 
            ID = p.PickupPointID, 
            Address = $"{p.City}, {p.Street}, {p.HouseNumber}" 
        }).ToList();

        _cmbPickupPoint.DataSource = displayList;
        _cmbPickupPoint.DisplayMember = "Address";
        _cmbPickupPoint.ValueMember = "ID";
    }

    private void LoadItems()
    {
        _panelItems.Controls.Clear();
        var items = CartService.Instance.Items;

        if (items.Count == 0)
        {
            _panelItems.Controls.Add(new Label { Text = "Корзина пуста", Font = new Font("Segoe UI", 14), AutoSize = true });
            _lblTotal.Text = "Итого: 0 ₽";
            return;
        }

        foreach (var item in items)
        {
            var p = new Panel { Size = new Size(700, 80), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10), BackColor = Color.White };
            
            var lblName = new Label { Text = item.ProductName, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            
            decimal price = item.ProductDiscountAmount > 0 
                ? item.ProductCost * (1 - item.ProductDiscountAmount.Value / 100m) 
                : item.ProductCost;

            var lblPrice = new Label { Text = $"{price:N0} ₽", Location = new Point(10, 40), Font = new Font("Segoe UI", 12), ForeColor = Color.Green, AutoSize = true };

            // Quantity Controls
            var btnMinus = new Button { Text = "-", Location = new Point(400, 25), Size = new Size(30, 30), BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            var lblQuantity = new Label { Text = item.Quantity.ToString(), Location = new Point(435, 30), Font = new Font("Segoe UI", 10), AutoSize = true };
            var btnPlus = new Button { Text = "+", Location = new Point(470, 25), Size = new Size(30, 30), BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };

            btnMinus.Click += (s, e) => {
                CartService.Instance.DecreaseQuantity(item);
                LoadItems();
            };
            btnPlus.Click += (s, e) => {
                CartService.Instance.IncreaseQuantity(item);
                LoadItems();
            };

            var btnRemove = new Button { Text = "Удалить", Location = new Point(600, 25), Size = new Size(80, 30), BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnRemove.Click += (s, e) => {
                CartService.Instance.RemoveFromCart(item); // Remove the specific CartItem
                LoadItems();
            };

            p.Controls.Add(lblName);
            p.Controls.Add(lblPrice);
            p.Controls.Add(btnMinus);
            p.Controls.Add(lblQuantity);
            p.Controls.Add(btnPlus);
            p.Controls.Add(btnRemove);
            _panelItems.Controls.Add(p);
        }

        _lblTotal.Text = $"Итого: {CartService.Instance.GetTotal():N0} ₽";
    }

    private void BtnCheckout_Click(object? sender, EventArgs e)
    {
        if (CartService.Instance.GetCount() == 0) return;
        
        if (AuthService.CurrentUser == null) { MessageBox.Show("Авторизуйтесь!"); return; }

        if (_cmbPickupPoint.SelectedItem == null) { MessageBox.Show("Выберите пункт выдачи!"); return; }
        
        decimal total = CartService.Instance.GetTotal();
        
        if (AuthService.CurrentUser.Balance < total)
        {
            MessageBox.Show($"Недостаточно средств! Ваш баланс: {AuthService.CurrentUser.Balance:N0} ₽.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // --- Create Real Order in DB ---
        int pickupPointId = (int)_cmbPickupPoint.SelectedValue!;
        int code = new Random().Next(100, 999);
        
        // Assuming OrderStatus ID 1 is "New" (We inserted it in fix_data.sql)
        int statusId = _context.OrderStatuses.FirstOrDefault(s => s.StatusName == "Новый")?.OrderStatusID ?? 1;

        var order = new OrderHeader {
            UserID = AuthService.CurrentUser.UserID,
            OrderStatusID = statusId,
            PickupPointID = pickupPointId,
            OrderDate = DateTime.Now,
            OrderDeliveryDate = DateTime.Now.AddDays(3),
            OrderPickupCode = code
        };

        _context.OrderHeaders.Add(order);
        _context.SaveChanges(); // Get OrderID

        // Add Products
        foreach (var item in CartService.Instance.Items)
        {
            // Simple logic: 1 row per item instance. If multiple same items, they are separate in list
            // For correct DB, we should group by ID.
            // But if Primary Key is (OrderID, ProductID), we MUST group!
        }
        
        // Group items for proper insert
        var groupedItems = CartService.Instance.Items
            .GroupBy(x => x.ProductID)
            .Select(g => new { ProductID = g.Key, Count = g.Count() });

        foreach (var g in groupedItems)
        {
            var op = new OrderProduct {
                OrderID = order.OrderID,
                ProductID = g.ProductID,
                Quantity = g.Count
            };
            _context.OrderProducts.Add(op);
        }

        // Deduct Balance
        var user = _context.AppUsers.Find(AuthService.CurrentUser.UserID);
        if (user != null) user.Balance -= total;
        
        _context.SaveChanges();

        // Update local user info
        AuthService.CurrentUser.Balance = user!.Balance;

        MessageBox.Show($"Заказ №{order.OrderID} успешно оформлен!\nКод получения: {code}\nПункт выдачи: {_cmbPickupPoint.Text}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        CartService.Instance.Clear();
        this.Close();
    }
}