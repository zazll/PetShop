using PetShopApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace PetShopApp.Forms;

public class ReportForm : Form
{
    private PetShopContext _context;
    private PictureBox pbxChart;
    private ComboBox cmbReportType;
    
    // Theme
    private readonly Color PrimaryColor = Color.FromArgb(46, 204, 113);
    private readonly Color TextColor = Color.FromArgb(64, 64, 64);

    public ReportForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Отчетность и аналитика";
        this.Size = new Size(900, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(20) };
        cmbReportType = new ComboBox { 
            Location = new Point(20, 20), 
            Width = 250, 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.White
        };
        cmbReportType.Items.Add("Продажи по категориям");
        cmbReportType.Items.Add("Топ товаров");
        cmbReportType.SelectedIndex = 0;
        cmbReportType.SelectedIndexChanged += (s, e) => DrawChart();

        topPanel.Controls.Add(cmbReportType);

        pbxChart = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };
        pbxChart.Paint += PbxChart_Paint;

        this.Controls.Add(pbxChart);
        this.Controls.Add(topPanel);
    }

    private void DrawChart()
    {
        pbxChart.Invalidate();
    }

    private void PbxChart_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (cmbReportType.SelectedIndex == 0) // Sales by Category
        {
            DrawSalesByCategory(g);
        }
        else // Top Products
        {
            DrawTopProducts(g);
        }
    }

    private void DrawSalesByCategory(Graphics g)
    {
        var data = _context.Products
            .GroupBy(p => p.Category.CategoryName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToList();

        if (!data.Any())
        {
            g.DrawString("Нет данных для отображения", new Font("Segoe UI", 12), Brushes.Gray, 20, 20);
            return;
        }

        int maxVal = data.Max(d => d.Count);
        int barWidth = 60;
        int spacing = 30;
        int startX = 60;
        int startY = pbxChart.Height - 60;
        int maxHeight = pbxChart.Height - 150;
        
        g.DrawString("Количество товаров по категориям", new Font("Segoe UI", 16, FontStyle.Bold), new SolidBrush(TextColor), 20, 10);

        for (int i = 0; i < data.Count; i++)
        {
            int h = (int)((double)data[i].Count / maxVal * maxHeight);
            
            // Bar
            var rect = new Rectangle(startX + i * (barWidth + spacing), startY - h, barWidth, h);
            g.FillRectangle(new SolidBrush(PrimaryColor), rect);
            
            // Value
            var valStr = data[i].Count.ToString();
            var valFont = new Font("Segoe UI", 10, FontStyle.Bold);
            var valSize = g.MeasureString(valStr, valFont);
            g.DrawString(valStr, valFont, Brushes.Gray, rect.X + (rect.Width - valSize.Width)/2, rect.Y - 20);
            
            // Label
            g.DrawString(data[i].Name, new Font("Segoe UI", 9), Brushes.Black, rect.X, startY + 5);
        }
        
        // Base line
        g.DrawLine(Pens.LightGray, 40, startY, pbxChart.Width - 40, startY);
    }

    private void DrawTopProducts(Graphics g)
    {
        var data = _context.Products
            .OrderByDescending(p => p.ProductCost)
            .Take(5)
            .Select(p => new { Name = p.ProductName, Value = (int)p.ProductCost })
            .ToList();

        if (!data.Any()) return;

        int maxVal = data.Max(d => d.Value);
        int barHeight = 40;
        int spacing = 25;
        int startX = 200;
        int startY = 80;
        int maxWidth = pbxChart.Width - 250;

        g.DrawString("Топ 5 самых дорогих товаров", new Font("Segoe UI", 16, FontStyle.Bold), new SolidBrush(TextColor), 20, 10);

        for (int i = 0; i < data.Count; i++)
        {
            int w = (int)((double)data[i].Value / maxVal * maxWidth);
            
            // Bar
            var rect = new Rectangle(startX, startY + i * (barHeight + spacing), w, barHeight);
            g.FillRectangle(new SolidBrush(Color.LightSalmon), rect); // Accent color for high price
            
            // Label
            g.DrawString(data[i].Name, new Font("Segoe UI", 10), Brushes.Black, 20, rect.Y + 10);
            
            // Value
            g.DrawString($"{data[i].Value:C0}", new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, rect.X + w + 10, rect.Y + 10);
        }
    }
}