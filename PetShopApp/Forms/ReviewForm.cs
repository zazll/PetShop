using PetShopApp.Data;
using PetShopApp.Models;
using PetShopApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace PetShopApp.Forms;

public class ReviewForm : Form
{
    private PetShopContext _context;
    private Product _product;
    private DataGridView dgvReviews;
    private TextBox txtComment;
    private ComboBox cmbRating;
    private Button btnAddReview;

    public ReviewForm(Product product)
    {
        _product = product;
        _context = new PetShopContext();
        InitializeComponent();
        LoadReviews();
    }

    private void InitializeComponent()
    {
        this.Text = $"Отзывы о товаре: {_product.ProductName}";
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterScreen;

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
        
        var lblRating = new Label { Text = "Оценка:", Location = new Point(10, 10), AutoSize = true };
        cmbRating = new ComboBox { Location = new Point(70, 8), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbRating.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
        cmbRating.SelectedIndex = 4;

        var lblComment = new Label { Text = "Комментарий:", Location = new Point(10, 40), AutoSize = true };
        txtComment = new TextBox { Location = new Point(100, 40), Width = 350, Height = 50, Multiline = true };

        btnAddReview = new Button { Text = "Оставить отзыв", Location = new Point(460, 40), Width = 110, Height = 50 };
        btnAddReview.Click += BtnAddReview_Click;

        topPanel.Controls.Add(lblRating);
        topPanel.Controls.Add(cmbRating);
        topPanel.Controls.Add(lblComment);
        topPanel.Controls.Add(txtComment);
        topPanel.Controls.Add(btnAddReview);

        dgvReviews = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserName", HeaderText = "Пользователь", Width = 120 });
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Оценка", Width = 60 });
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Комментарий", Width = 250 });
        dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Date", HeaderText = "Дата", Width = 100 });

        this.Controls.Add(dgvReviews);
        this.Controls.Add(topPanel);

        // Access check
        if (AuthService.CurrentUser == null)
        {
            topPanel.Enabled = false;
            this.Text += " (Авторизуйтесь, чтобы оставить отзыв)";
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

        MessageBox.Show("Отзыв добавлен!");
        txtComment.Clear();
        LoadReviews();
    }
}
