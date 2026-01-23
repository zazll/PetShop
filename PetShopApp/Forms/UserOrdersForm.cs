using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PetShopApp.Data; // Assuming context might be needed for details
using PetShopApp.Models; // Assuming models might be needed for details
using System.Net.Http; // For HttpClient
using System.IO; // For MemoryStream

namespace PetShopApp.Forms;

public partial class UserOrdersForm : Form
{
    private int _orderId;
    private string _orderCode;
    private PetShopContext _context; // To load order details if needed

    private Label _lblOrderCode;
    private PictureBox _pbBarcode;

    public UserOrdersForm(int orderId, string orderCode)
    {
        _orderId = orderId;
        _orderCode = orderCode;
        _context = new PetShopContext();
        InitializeComponent();
        LoadOrderDetails(); // Method to display code and generate barcode
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // UserOrdersForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(400, 350); // Smaller window for barcode
        this.Name = "UserOrdersForm";
        this.Text = $"Заказ №{_orderId} - QR-код";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        _lblOrderCode = new Label {
            Text = $"Код: {_orderCode}",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter, // Center text
            Dock = DockStyle.Top // Dock to top for better positioning
        };

        _pbBarcode = new PictureBox {
            Location = new Point(50, 70), // Will re-center later
            Size = new Size(300, 200), // Larger for QR code
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White // White background for QR
        };

        this.Controls.Add(_lblOrderCode);
        this.Controls.Add(_pbBarcode);
        this.ResumeLayout(false);
        this.PerformLayout(); // Ensure layout updates
    }

    private async void LoadOrderDetails()
    {
        // Here we'll generate the QR code
        await GenerateQRCode(_orderCode);
    }

    private async Task GenerateQRCode(string code)
    {
        try
        {
            string qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(code)}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(qrCodeUrl);
                response.EnsureSuccessStatusCode(); // Throws if not a success code

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    _pbBarcode.Image = Image.FromStream(stream);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при генерации QR-кода: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _pbBarcode.Image = GetPlaceholderImage(); // Fallback to placeholder
        }
    }

    private Image GetPlaceholderImage()
    {
        Bitmap bmp = new Bitmap(_pbBarcode.Width, _pbBarcode.Height);
        using (Graphics g = Graphics.FromImage(bmp)) { 
            g.Clear(Color.LightGray); 
            g.DrawString("Нет QR-кода", new Font("Segoe UI", 10), Brushes.DarkGray, new RectangleF(0, 0, _pbBarcode.Width, _pbBarcode.Height), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }
        return bmp;
    }
}

