using PetShopApp.Services;
using System.Drawing;

namespace PetShopApp.Forms;

public class RegistrationForm : Form
{
    private TextBox txtSurname;
    private TextBox txtName;
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnRegister;
    private AuthService _authService;

    public RegistrationForm()
    {
        _authService = new AuthService();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Регистрация";
        this.Size = new Size(400, 400);
        this.StartPosition = FormStartPosition.CenterScreen;

        int y = 30;
        int spacing = 40;

        var lblSurname = new Label { Text = "Фамилия:", Location = new Point(30, y), AutoSize = true };
        txtSurname = new TextBox { Location = new Point(130, y), Width = 200 };
        
        y += spacing;
        var lblName = new Label { Text = "Имя:", Location = new Point(30, y), AutoSize = true };
        txtName = new TextBox { Location = new Point(130, y), Width = 200 };

        y += spacing;
        var lblLogin = new Label { Text = "Email:", Location = new Point(30, y), AutoSize = true };
        txtLogin = new TextBox { Location = new Point(130, y), Width = 200 };

        y += spacing;
        var lblPass = new Label { Text = "Пароль:", Location = new Point(30, y), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(130, y), Width = 200, PasswordChar = '*' };

        y += spacing * 2;
        btnRegister = new Button { Text = "Зарегистрироваться", Location = new Point(100, y), Width = 180, Height = 40, BackColor = Color.CornflowerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnRegister.Click += BtnRegister_Click;

        this.Controls.Add(lblSurname);
        this.Controls.Add(txtSurname);
        this.Controls.Add(lblName);
        this.Controls.Add(txtName);
        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPass);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnRegister);
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSurname.Text) || string.IsNullOrWhiteSpace(txtName.Text) ||
            string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Заполните все поля");
            return;
        }

        bool success = _authService.Register(txtSurname.Text, txtName.Text, null, txtLogin.Text, txtPassword.Text);
        if (success)
        {
            MessageBox.Show("Регистрация успешна! Теперь вы можете войти.");
            this.Close();
        }
        else
        {
            MessageBox.Show("Пользователь с таким логином уже существует.");
        }
    }
}
