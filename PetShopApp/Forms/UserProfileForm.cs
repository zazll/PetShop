using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;

namespace PetShopApp.Forms;

public class UserProfileForm : Form
{
    private PetShopContext _context;
    private Label _lblBalance;
    private DataGridView _dgvHistory;

    public UserProfileForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Профиль пользователя";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        var user = AuthService.CurrentUser!;

        var lblTitle = new Label { Text = $"{user.UserSurname} {user.UserName}", Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
        var lblEmail = new Label { Text = user.UserLogin, Font = new Font("Segoe UI", 12), Location = new Point(20, 60), AutoSize = true, ForeColor = Color.Gray };

        // Balance Section
        var pnlBalance = new Panel { Location = new Point(20, 100), Size = new Size(300, 100), BackColor = Color.FromArgb(240, 255, 240), BorderStyle = BorderStyle.FixedSingle };
        var lblBalTitle = new Label { Text = "Баланс:", Location = new Point(10, 10), Font = new Font("Segoe UI", 10) };
        _lblBalance = new Label { Text = "0 ₽", Location = new Point(10, 30), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.Green, AutoSize = true };
        
        var btnTopUp = new Button { Text = "Пополнить (+1000)", Location = new Point(10, 65), Width = 150, BackColor = Color.White };
        btnTopUp.Click += (s, e) => {
            // Mock Top Up
             user.Balance += 1000;
             // Update DB
             var dbUser = _context.AppUsers.Find(user.UserID);
             if (dbUser != null) {
                 dbUser.Balance += 1000;
                 _context.SaveChanges();
             }
             LoadData();
        };

        pnlBalance.Controls.Add(lblBalTitle);
        pnlBalance.Controls.Add(_lblBalance);
        pnlBalance.Controls.Add(btnTopUp);

        // History Section
        var lblHist = new Label { Text = "История заказов", Location = new Point(20, 220), Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true };
        
        _dgvHistory = new DataGridView {
            Location = new Point(20, 250),
            Size = new Size(740, 280),
            AutoGenerateColumns = false,
            BackgroundColor = Color.White,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false
        };
        _dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "№ Заказа", DataPropertyName = "OrderId", Width = 80 });
        _dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата", DataPropertyName = "Date", Width = 120 });
        _dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", Width = 120 });
        _dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Код получения", DataPropertyName = "Code", Width = 100 });

        var btnShowBarcode = new Button {
            Text = "Показать штрихкод",
            Location = new Point(20, 540),
            Width = 150,
            BackColor = Color.LightGray
        };
        btnShowBarcode.Click += BtnShowBarcode_Click;

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblEmail);
        this.Controls.Add(pnlBalance);
        this.Controls.Add(lblHist);
        this.Controls.Add(_dgvHistory);
        this.Controls.Add(btnShowBarcode);
    }

    private void BtnShowBarcode_Click(object? sender, EventArgs e)
    {
        if (_dgvHistory.CurrentRow == null)
        {
            MessageBox.Show("Пожалуйста, выберите заказ из списка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedOrder = _dgvHistory.CurrentRow.DataBoundItem as dynamic;
        if (selectedOrder == null) return;

        int orderId = selectedOrder.OrderId;
        string orderCode = selectedOrder.Code.ToString();

        new UserOrdersForm(orderId, orderCode).ShowDialog();
    }

    private void LoadData()
    {
        var user = AuthService.CurrentUser;
        if (user == null) return;

        // Refresh Balance display
        _lblBalance.Text = $"{user.Balance:N2} ₽";

        // Load History
        var orders = _context.OrderHeaders
            .Where(o => o.UserID == user.UserID)
            .Include(o => o.OrderStatus)
            .OrderByDescending(o => o.OrderDate)
            .ToList()
            .Select(o => new {
                OrderId = o.OrderID,
                Date = o.OrderDate.ToShortDateString(),
                Status = o.OrderStatus.StatusName,
                Code = o.OrderPickupCode
            })
            .ToList();
            
        _dgvHistory.DataSource = orders;
    }
}
