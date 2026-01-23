using PetShopApp.Services;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PetShopApp.Forms;

public class LoginForm : Form
{
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;
    private LinkLabel lnkRegister;
    private Label lblStatus;
    private AuthService _authService;
    
    // Theme
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113); // Emerald Green
    private readonly Color TextColor = Color.FromArgb(64, 64, 64);

    public LoginForm()
    {
        _authService = new AuthService();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Вход в систему";
        this.Size = new Size(450, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // Logo/Header
        var lblTitle = new Label { 
            Text = "PetShop", 
            Font = new Font("Segoe UI", 24, FontStyle.Bold), 
            ForeColor = PrimaryColor, 
            Location = new Point(0, 50), // Moved down from 40 
            AutoSize = false,
            Width = 450,
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        var lblSubtitle = new Label { 
            Text = "Вход в аккаунт", 
            Font = new Font("Segoe UI", 12), 
            ForeColor = Color.Gray, 
            Location = new Point(0, 95), // Moved down from 85
            AutoSize = false,
            Width = 450,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Inputs
        int startY = 150; // Moved down from 140
        int inputWidth = 300;
        int startX = (450 - inputWidth) / 2;

        var lblLogin = new Label { Text = "Email или телефон", Location = new Point(startX, startY), AutoSize = true, ForeColor = TextColor, Font = new Font("Segoe UI", 10) };
        txtLogin = CreateStyledTextBox();
        txtLogin.Location = new Point(startX, startY + 25);
        txtLogin.Width = inputWidth;

        var lblPass = new Label { Text = "Пароль", Location = new Point(startX, startY + 70), AutoSize = true, ForeColor = TextColor, Font = new Font("Segoe UI", 10) };
        txtPassword = CreateStyledTextBox();
        txtPassword.Location = new Point(startX, startY + 95);
        txtPassword.Width = inputWidth;
        txtPassword.PasswordChar = '•';

        // Button
        btnLogin = new Button { 
            Text = "Войти", 
            Location = new Point(startX, startY + 150), 
            Width = inputWidth, 
            Height = 45, 
            BackColor = PrimaryColor, 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += BtnLogin_Click;

        // Link
        lnkRegister = new LinkLabel { 
            Text = "Зарегистрироваться", 
            Location = new Point(0, startY + 210), 
            AutoSize = false,
            Width = 450,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10),
            LinkColor = PrimaryColor,
            ActiveLinkColor = Color.DarkGreen
        };
        lnkRegister.LinkClicked += (s, e) => new RegistrationForm().ShowDialog();

        lblStatus = new Label { 
            Location = new Point(0, startY + 240), 
            Width = 450, 
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.IndianRed,
            Font = new Font("Segoe UI", 9) 
        };

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblSubtitle);
        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPass);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
        this.Controls.Add(lnkRegister);
        this.Controls.Add(lblStatus);
    }
    
    private TextBox CreateStyledTextBox()
    {
        return new TextBox {
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.FromArgb(250, 250, 250),
            Height = 30
        };
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var user = _authService.Login(txtLogin.Text, txtPassword.Text);
        if (user != null)
        {
            new MainForm().Show();
            this.Hide();
        }
        else
        {
            lblStatus.Text = "Неверный логин или пароль";
        }
    }
}
