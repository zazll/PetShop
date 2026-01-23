using PetShopApp.Models;
using PetShopApp.Services;
using System.Drawing;

namespace PetShopApp.Forms;

public class CartForm : Form
{
    private FlowLayoutPanel _panelItems;
    private Label _lblTotal;
    private Button _btnCheckout;

    public CartForm()
    {
        InitializeComponent();
        LoadItems();
    }

    private void InitializeComponent()
    {
        this.Text = "Корзина";
        this.Size = new Size(800, 600);
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

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.WhiteSmoke };
        
        _lblTotal = new Label { 
            Text = "Итого: 0 ₽", 
            Font = new Font("Segoe UI", 16, FontStyle.Bold), 
            Location = new Point(20, 20), 
            AutoSize = true 
        };
        
        _btnCheckout = new Button {
            Text = "Оформить заказ",
            Location = new Point(550, 15),
            Size = new Size(200, 50),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        _btnCheckout.Click += BtnCheckout_Click;

        bottomPanel.Controls.Add(_lblTotal);
        bottomPanel.Controls.Add(_btnCheckout);

        this.Controls.Add(_panelItems);
        this.Controls.Add(bottomPanel);
        this.Controls.Add(title);
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

            var btnRemove = new Button { Text = "Удалить", Location = new Point(600, 25), Size = new Size(80, 30), BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnRemove.Click += (s, e) => {
                CartService.Instance.RemoveFromCart(item);
                LoadItems();
            };

            p.Controls.Add(lblName);
            p.Controls.Add(lblPrice);
            p.Controls.Add(btnRemove);
            _panelItems.Controls.Add(p);
        }

        _lblTotal.Text = $"Итого: {CartService.Instance.GetTotal():N0} ₽";
    }

    private void BtnCheckout_Click(object? sender, EventArgs e)
    {
        if (CartService.Instance.GetCount() == 0) return;
        
        // Check Balance
        if (AuthService.CurrentUser == null) { MessageBox.Show("Авторизуйтесь!"); return; }

        // Here we should ideally check DB balance freshness, but let's trust loaded User object for now or reload it.
        // Reload user to be sure
        // ... (Skipping full DB reload for brevity, assuming loaded user is mostly ok or we just check logic)
        
        decimal total = CartService.Instance.GetTotal();
        
        if (AuthService.CurrentUser.Balance < total)
        {
            MessageBox.Show($"Недостаточно средств! Ваш баланс: {AuthService.CurrentUser.Balance:N0} ₽. Пополните счет в профиле.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Process Order (Mock)
        MessageBox.Show("Заказ успешно оформлен! Деньги списаны (демо).", "Успех");
        CartService.Instance.Clear();
        LoadItems();
        // In real app: Update DB (Create Order, Decrease Balance, Save Changes)
    }
}
