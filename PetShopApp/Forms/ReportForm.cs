using PetShopApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace PetShopApp.Forms;

public class ReportForm : Form
{
    private PetShopContext _context;
    private PictureBox pbxChart;
    private ComboBox cmbReportType;

    public ReportForm()
    {
        _context = new PetShopContext();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Отчетность и аналитика";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 50 };
        cmbReportType = new ComboBox { Location = new Point(10, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
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
        // Aggregate data
        // For simulation, if no orders, we just count products by category
        var data = _context.Products
            .GroupBy(p => p.Category.CategoryName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToList();

        if (!data.Any())
        {
            g.DrawString("Нет данных", this.Font, Brushes.Black, 10, 10);
            return;
        }

        int maxVal = data.Max(d => d.Count);
        int barWidth = 50;
        int spacing = 20;
        int startX = 50;
        int startY = pbxChart.Height - 50;
        int maxHeight = pbxChart.Height - 100;

        for (int i = 0; i < data.Count; i++)
        {
            int h = (int)((double)data[i].Count / maxVal * maxHeight);
            var brush = Brushes.CornflowerBlue;
            
            g.FillRectangle(brush, startX + i * (barWidth + spacing), startY - h, barWidth, h);
            g.DrawRectangle(Pens.Black, startX + i * (barWidth + spacing), startY - h, barWidth, h);
            
            // Label
            g.DrawString(data[i].Name, new Font("Arial", 8), Brushes.Black, startX + i * (barWidth + spacing), startY + 5);
            // Value
            g.DrawString(data[i].Count.ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.Black, startX + i * (barWidth + spacing) + 15, startY - h - 20);
        }
        
        g.DrawString("Количество товаров по категориям", new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 10, 10);
    }

    private void DrawTopProducts(Graphics g)
    {
        // Mock data for Top Products as we might not have orders
        var data = _context.Products
            .OrderByDescending(p => p.ProductCost)
            .Take(5)
            .Select(p => new { Name = p.ProductName, Value = (int)p.ProductCost })
            .ToList();

        if (!data.Any()) return;

        int maxVal = data.Max(d => d.Value);
        int barHeight = 40;
        int spacing = 20;
        int startX = 150;
        int startY = 50;
        int maxWidth = pbxChart.Width - 200;

        for (int i = 0; i < data.Count; i++)
        {
            int w = (int)((double)data[i].Value / maxVal * maxWidth);
            
            g.FillRectangle(Brushes.LightCoral, startX, startY + i * (barHeight + spacing), w, barHeight);
            g.DrawString(data[i].Name, new Font("Arial", 9), Brushes.Black, 10, startY + i * (barHeight + spacing) + 10);
            g.DrawString($"{data[i].Value:C0}", new Font("Arial", 9), Brushes.Black, startX + w + 5, startY + i * (barHeight + spacing) + 10);
        }

        g.DrawString("Топ 5 самых дорогих товаров", new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 10, 10);
    }
}
