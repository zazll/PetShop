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
    
    // Theme
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113); 
    private readonly Color TextColor = Color.FromArgb(64, 64, 64);

    public RegistrationForm()
    {
        _authService = new AuthService();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Регистрация";
        this.Size = new Size(450, 550);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblTitle = new Label { 
            Text = "Создание аккаунта", 
            Font = new Font("Segoe UI", 18, FontStyle.Bold), 
            ForeColor = TextColor, 
            Location = new Point(0, 30), 
            AutoSize = false,
            Width = 450,
            TextAlign = ContentAlignment.MiddleCenter
        };

        int startY = 90;
        int inputWidth = 300;
        int startX = (450 - inputWidth) / 2;
        int spacing = 60;

        // Surname
        var lblSurname = new Label { Text = "Фамилия", Location = new Point(startX, startY), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };
        txtSurname = CreateStyledTextBox();
        txtSurname.Location = new Point(startX, startY + 20);
        txtSurname.Width = inputWidth;
        
        // Name
        startY += spacing;
        var lblName = new Label { Text = "Имя", Location = new Point(startX, startY), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };
        txtName = CreateStyledTextBox();
        txtName.Location = new Point(startX, startY + 20);
        txtName.Width = inputWidth;

        // Login
        startY += spacing;
        var lblLogin = new Label { Text = "Email (Логин)", Location = new Point(startX, startY), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };
        txtLogin = CreateStyledTextBox();
        txtLogin.Location = new Point(startX, startY + 20);
        txtLogin.Width = inputWidth;

        // Password
        startY += spacing;
        var lblPass = new Label { Text = "Пароль", Location = new Point(startX, startY), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };
        txtPassword = CreateStyledTextBox();
        txtPassword.Location = new Point(startX, startY + 20);
        txtPassword.Width = inputWidth;
        txtPassword.PasswordChar = '•';

        // Button
        startY += spacing * 1.5;
        btnRegister = new Button { 
            Text = "Зарегистрироваться", 
            Location = new Point(startX, startY), 
            Width = inputWidth, 
            Height = 45, 
            BackColor = PrimaryColor, 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.Click += BtnRegister_Click;

        this.Controls.Add(lblTitle);
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

    private TextBox CreateStyledTextBox()
    {
        return new TextBox {
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.FromArgb(250, 250, 250),
            Height = 30
        };
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSurname.Text) || string.IsNullOrWhiteSpace(txtName.Text) ||
            string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool success = _authService.Register(txtSurname.Text, txtName.Text, null, txtLogin.Text, txtPassword.Text);
        if (success)
        {
            MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        else
        {
            MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}