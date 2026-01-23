using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;

namespace PetShopApp.Forms;

public class ReviewForm : Form
{
    private PetShopContext _context;
    private Product _product;
    private DataGridView dgvReviews;
    private TextBox txtComment;
    private ComboBox cmbRating;
    private Button btnAddReview;
    
    // Theme
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113);
    private readonly Color TextColor = Color.FromArgb(64, 64, 64);
    private readonly Color SurfaceColor = Color.FromArgb(248, 255, 248);

    public ReviewForm(Product product)
    {
        _product = product;
        _context = new PetShopContext();
        InitializeComponent();
        LoadReviews();
    }

    private void InitializeComponent()
    {
        this.Text = $"Отзывы: {_product.ProductName}";
        this.Size = new Size(700, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        // Top Panel (Input)
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 140, Padding = new Padding(20), BackColor = SurfaceColor };
        
        var lblRating = new Label { Text = "Оценка:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
        cmbRating = new ComboBox { Location = new Point(100, 18), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbRating.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
        cmbRating.SelectedIndex = 4;
        cmbRating.BackColor = Color.White;

        var lblComment = new Label { Text = "Ваш отзыв:", Location = new Point(20, 50), AutoSize = true, Font = new Font("Segoe UI", 10) };
        txtComment = new TextBox { 
            Location = new Point(100, 50), 
            Width = 400, 
            Height = 60, 
            Multiline = true, 
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10)
        };

        btnAddReview = new Button { 
            Text = "Отправить", 
            Location = new Point(520, 50), 
            Width = 140, 
            Height = 60, 
            BackColor = PrimaryColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnAddReview.FlatAppearance.BorderSize = 0;
        btnAddReview.Click += BtnAddReview_Click;

        topPanel.Controls.Add(lblRating);
        topPanel.Controls.Add(cmbRating);
        topPanel.Controls.Add(lblComment);
        topPanel.Controls.Add(txtComment);
        topPanel.Controls.Add(btnAddReview);

        // Grid
        dgvReviews = new DataGridView { 
            Dock = DockStyle.Fill, 
            AutoGenerateColumns = false, 
            ReadOnly = true, 
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            GridColor = Color.FromArgb(240, 240, 240),
            RowTemplate = { Height = 60 },
            EnableHeadersVisualStyles = false
        };

        dgvReviews.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle {
            BackColor = Color.FromArgb(245, 245, 245),
            ForeColor = TextColor,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Padding = new Padding(5)
        };
        
        dgvReviews.DefaultCellStyle = new DataGridViewCellStyle {
            Padding = new Padding(5),
            ForeColor = TextColor,
            Font = new Font("Segoe UI", 10),
            SelectionBackColor = SurfaceColor,
            SelectionForeColor = TextColor
        };

        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserName", HeaderText = "Пользователь", Width = 150 });
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Оценка", Width = 80, DefaultCellStyle = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = PrimaryColor } });
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Комментарий", Width = 300 });
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Date", HeaderText = "Дата", Width = 120 });

        this.Controls.Add(dgvReviews);
        this.Controls.Add(topPanel);

        // Access check
        if (AuthService.CurrentUser == null)
        {
            topPanel.Enabled = false;
            txtComment.Text = "Пожалуйста, авторизуйтесь, чтобы оставить отзыв.";
        }
    }

    private void LoadReviews()
    {
        var reviews = _context.Reviews
            .Where(r => r.ProductID == _product.ProductID)
            .Include(r => r.User)
            .OrderByDescending(r => r.ReviewDate)
            .ToList()
            .Select(r => new
            {
                UserName = $"{r.User.UserSurname} {r.User.UserName}",
                r.Rating,
                r.Comment,
                Date = r.ReviewDate.ToShortDateString()
            })
            .ToList();

        dgvReviews.DataSource = reviews;
    }

    private void BtnAddReview_Click(object? sender, EventArgs e)
    {
        if (AuthService.CurrentUser == null) return;

        if (string.IsNullOrWhiteSpace(txtComment.Text))
        {
            MessageBox.Show("Введите комментарий");
            return;
        }

        var review = new Review
        {
            ProductID = _product.ProductID,
            UserID = AuthService.CurrentUser.UserID,
            Rating = int.Parse(cmbRating.SelectedItem.ToString()!),
            Comment = txtComment.Text,
            ReviewDate = DateTime.Now
        };

        _context.Reviews.Add(review);
        _context.SaveChanges();

        MessageBox.Show("Спасибо за ваш отзыв!");
        txtComment.Clear();
        LoadReviews();
    }
}