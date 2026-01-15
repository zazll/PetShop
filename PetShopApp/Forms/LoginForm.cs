using PetShopApp.Services;

namespace PetShopApp.Forms;

public class LoginForm : Form
{
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;
    private Button btnRegister;
    private Label lblStatus;
    private AuthService _authService;

    public LoginForm()
    {
        _authService = new AuthService();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Авторизация";
        this.Size = new Size(400, 300);
        this.StartPosition = FormStartPosition.CenterScreen;

        var lblLogin = new Label { Text = "Логин:", Location = new Point(50, 50), AutoSize = true };
        txtLogin = new TextBox { Location = new Point(150, 50), Width = 150 };

        var lblPass = new Label { Text = "Пароль:", Location = new Point(50, 90), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(150, 90), Width = 150, PasswordChar = '*' };

        btnLogin = new Button { Text = "Войти", Location = new Point(50, 140), Width = 100 };
        btnLogin.Click += BtnLogin_Click;

        btnRegister = new Button { Text = "Регистрация", Location = new Point(180, 140), Width = 100 };
        btnRegister.Click += BtnRegister_Click;

        lblStatus = new Label { Location = new Point(50, 180), Width = 250, ForeColor = Color.Red };

        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPass);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
        this.Controls.Add(btnRegister);
        this.Controls.Add(lblStatus);
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var user = _authService.Login(txtLogin.Text, txtPassword.Text);
        if (user != null)
        {
            MessageBox.Show($"Добро пожаловать, {user.UserName}!");
            new MainForm().Show();
            this.Hide();
        }
        else
        {
            lblStatus.Text = "Неверный логин или пароль";
        }
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        // Simple registration for demo purposes
        // In real app, open a separate form
        if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            lblStatus.Text = "Введите логин и пароль для регистрации";
            return;
        }

        bool success = _authService.Register("GuestSurname", "GuestName", null, txtLogin.Text, txtPassword.Text);
        if (success)
        {
            MessageBox.Show("Регистрация успешна! Теперь войдите.");
        }
        else
        {
            lblStatus.Text = "Пользователь уже существует";
        }
    }
}
