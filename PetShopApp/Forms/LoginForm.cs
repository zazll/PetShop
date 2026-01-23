using PetShopApp.Services;
using System.Drawing;

namespace PetShopApp.Forms;

public class LoginForm : Form
{
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;
    private LinkLabel lnkRegister;
    private Label lblStatus;
    private AuthService _authService;

    public LoginForm()
    {
        _authService = new AuthService();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Вход в систему";
        this.Size = new Size(400, 350);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        var lblTitle = new Label { Text = "PetShop Вход", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.Purple, Location = new Point(100, 30), AutoSize = true };

        var lblLogin = new Label { Text = "Логин (Email):", Location = new Point(50, 100), AutoSize = true };
        txtLogin = new TextBox { Location = new Point(50, 120), Width = 280, Font = new Font("Segoe UI", 10) };

        var lblPass = new Label { Text = "Пароль:", Location = new Point(50, 160), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(50, 180), Width = 280, PasswordChar = '*', Font = new Font("Segoe UI", 10) };

        btnLogin = new Button { Text = "Войти", Location = new Point(50, 230), Width = 280, Height = 40, BackColor = Color.Purple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnLogin.Click += BtnLogin_Click;

        lnkRegister = new LinkLabel { Text = "Нет аккаунта? Зарегистрироваться", Location = new Point(90, 280), AutoSize = true };
        lnkRegister.LinkClicked += (s, e) => new RegistrationForm().ShowDialog();

        lblStatus = new Label { Location = new Point(50, 210), Width = 280, ForeColor = Color.Red };

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPass);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
        this.Controls.Add(lnkRegister);
        this.Controls.Add(lblStatus);
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