using PetShopApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Drawing2D;

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
        this.Text = "Аналитика продаж";
        this.Size = new Size(1000, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(20) };
        cmbReportType = new ComboBox { 
            Location = new Point(20, 20), 
            Width = 300, 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.White
        };
        cmbReportType.Items.Add("Продажи по категориям (Количество)");
        cmbReportType.Items.Add("Топ товаров (По стоимости)");
        cmbReportType.Items.Add("Динамика продаж (По дням)"); // New
        cmbReportType.SelectedIndex = 0;
        cmbReportType.SelectedIndexChanged += (s, e) => DrawChart();

        topPanel.Controls.Add(cmbReportType);

        pbxChart = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };
        pbxChart.Paint += PbxChart_Paint;
        pbxChart.Resize += (s, e) => pbxChart.Invalidate();

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
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (cmbReportType.SelectedIndex == 0) // Category Sales
        {
            DrawSalesByCategory(g);
        }
        else if (cmbReportType.SelectedIndex == 1) // Top Products
        {
            DrawTopProducts(g);
        }
        else // Dynamics
        {
            DrawSalesDynamics(g);
        }
    }

    private void DrawSalesByCategory(Graphics g)
    {
        var data = _context.Products
            .GroupBy(p => p.Category.CategoryName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToList();

        DrawBarChart(g, "Распределение товаров по категориям", data.Select(d => (d.Name, (double)d.Count)).ToList(), Color.CornflowerBlue, false);
    }

    private void DrawTopProducts(Graphics g)
    {
        var data = _context.Products
            .OrderByDescending(p => p.ProductCost)
            .Take(5)
            .Select(p => new { Name = p.ProductName, Value = (double)p.ProductCost })
            .ToList();

        DrawBarChart(g, "Топ 5 самых дорогих товаров", data.Select(d => (d.Name, d.Value)).ToList(), Color.LightSalmon, true);
    }
    
    // Generic Bar Chart Renderer
    private void DrawBarChart(Graphics g, string title, List<(string Name, double Value)> data, Color color, bool isCurrency)
    {
        if (!data.Any()) {
            g.DrawString("Нет данных", new Font("Segoe UI", 12), Brushes.Gray, 20, 20);
            return;
        }

        double maxVal = data.Max(d => d.Value);
        if (maxVal == 0) maxVal = 1;

        int marginX = 100; // Increased margin for labels
        int marginY = 80;
        int chartWidth = pbxChart.Width - marginX * 2;
        int chartHeight = pbxChart.Height - marginY * 2;
        
        // Title
        g.DrawString(title, new Font("Segoe UI", 16, FontStyle.Bold), new SolidBrush(TextColor), 20, 20);

        int barWidth = Math.Min(80, chartWidth / data.Count / 2);
        int spacing = barWidth / 2;
        
        // Grid lines
        using (var pen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dash })
        {
            for (int i = 0; i <= 5; i++)
            {
                int y = marginY + chartHeight - (int)(chartHeight * i / 5.0);
                g.DrawLine(pen, marginX, y, marginX + chartWidth, y);
                g.DrawString((maxVal * i / 5.0).ToString(isCurrency ? "N0" : "0"), new Font("Segoe UI", 9), Brushes.Gray, 5, y - 8);
            }
        }

        // Bars
        for (int i = 0; i < data.Count; i++)
        {
            int h = (int)(data[i].Value / maxVal * chartHeight);
            int x = marginX + i * (barWidth + spacing) + 20;
            int y = marginY + chartHeight - h;

            var rect = new Rectangle(x, y, barWidth, h);
            
            using (var brush = new LinearGradientBrush(rect, color, ControlPaint.Light(color), LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawRectangle(Pens.Gray, rect);

            // Value Label
            string valStr = isCurrency ? $"{data[i].Value:C0}" : data[i].Value.ToString();
            var valSize = g.MeasureString(valStr, new Font("Segoe UI", 9, FontStyle.Bold));
            g.DrawString(valStr, new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, x + (barWidth - valSize.Width)/2, y - 20);

            // X Axis Label (Rotated if too long)
            string label = data[i].Name;
            if (label.Length > 15) label = label.Substring(0, 12) + "...";
            
            g.TranslateTransform(x + barWidth/2, marginY + chartHeight + 10);
            g.RotateTransform(30);
            g.DrawString(label, new Font("Segoe UI", 9), Brushes.Black, 0, 0);
            g.ResetTransform();
        }
    }

    private void DrawSalesDynamics(Graphics g)
    {
        g.DrawString("Динамика продаж (демо)", new Font("Segoe UI", 16, FontStyle.Bold), new SolidBrush(TextColor), 20, 20);

        // Simulated Data (Date, Amount)
        // In real app: _context.OrderHeaders.GroupBy...
        var points = new List<PointF>();
        var dates = new List<string>();
        
        var random = new Random();
        double[] values = { 5000, 7000, 4500, 12000, 9000, 15000, 11000 };
        
        int marginX = 80;
        int marginY = 100;
        int w = pbxChart.Width - marginX * 2;
        int h = pbxChart.Height - marginY * 2;

        double maxVal = 20000;
        
        // Grid
        using (var pen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot })
        {
            for (int i = 0; i <= 4; i++)
            {
                int y = marginY + h - (int)(h * i / 4.0);
                g.DrawLine(pen, marginX, y, marginX + w, y);
                g.DrawString((maxVal * i / 4.0).ToString("N0"), new Font("Segoe UI", 9), Brushes.Gray, 10, y - 8);
            }
        }
        
        // Plot points
        for (int i = 0; i < values.Length; i++)
        {
            float x = marginX + (float)i / (values.Length - 1) * w;
            float y = marginY + h - (float)(values[i] / maxVal * h);
            points.Add(new PointF(x, y));
            
            // X Label
            string date = DateTime.Now.AddDays(-6 + i).ToString("dd.MM");
            g.DrawString(date, new Font("Segoe UI", 9), Brushes.Black, x - 15, marginY + h + 10);
        }

        // Draw Line
        if (points.Count > 1)
        {
            using (var pen = new Pen(PrimaryColor, 3))
            {
                g.DrawLines(pen, points.ToArray());
            }
            
            // Draw Dots
            foreach (var p in points)
            {
                g.FillEllipse(Brushes.White, p.X - 4, p.Y - 4, 8, 8);
                g.DrawEllipse(new Pen(PrimaryColor, 2), p.X - 4, p.Y - 4, 8, 8);
            }
        }
    }
}
