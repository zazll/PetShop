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
        this.Text = $"Заказ №{_orderId} - Штрихкод";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        _lblOrderCode = new Label {
            Text = $"Код: {_orderCode}",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };

        _pbBarcode = new PictureBox {
            Location = new Point(50, 70),
            Size = new Size(300, 150),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.LightGray // Placeholder background
        };

        this.Controls.Add(_lblOrderCode);
        this.Controls.Add(_pbBarcode);
        this.ResumeLayout(false);
        this.PerformLayout(); // Ensure layout updates
    }

        private void LoadOrderDetails()

        {

            // Here we'll generate the barcode

            GenerateBarcode(_orderCode);

        }

    

        private void GenerateBarcode(string code)

        {

            try

            {

                // Code 39 only supports uppercase letters, numbers, and some symbols (- . $ / + % SPACE)

                // Convert to uppercase to be safe, or validate input

                string barcodeText = code.ToUpper().Replace(" ", "-"); 

    

                using (Bitmap barcodeBitmap = new Bitmap(_pbBarcode.Width, _pbBarcode.Height))

                using (Graphics graphics = Graphics.FromImage(barcodeBitmap))

                {

                    graphics.FillRectangle(Brushes.White, 0, 0, barcodeBitmap.Width, barcodeBitmap.Height);

    

                    int barWidth = 2; // Width of a narrow bar

                    int wideBarWidth = barWidth * 3; // Width of a wide bar

                    int height = _pbBarcode.Height - 50; // Leave space for text below

    

                    // Each character in Code 39 is 9 modules (5 bars and 4 spaces)

                    // Black bars and white spaces are represented as 1 (wide) or 0 (narrow)

                    // * is start/stop character

                    string fullCode = "*" + barcodeText + "*";

    

                    // Example: Code 39 character patterns (simplified for drawing)

                    // This is a basic illustration; a full implementation requires a lookup table for each char

                    // For simplicity, we'll draw fixed-width bars for now.

                    // A proper Code 39 implementation requires a mapping from character to 9-module pattern.

                    // For a quick visual, we'll just draw alternating black/white for the code length.

                    

                    // Let's use a simpler visual approach for now, showing alternaiting bars

                    int x = 10;

                    foreach (char c in fullCode)

                    {

                        // This is NOT a correct Code 39 pattern. This is a visual approximation.

                        // A proper Code 39 library or full implementation would be needed for scannable barcodes.

                        for (int i = 0; i < 5; i++) // 5 bars per char

                        {

                            graphics.FillRectangle(Brushes.Black, x, 10, barWidth, height);

                            x += barWidth * 2; // space between bars

                        }

                        x += wideBarWidth * 2; // space between characters

                    }

    

                    // Add the text below the barcode

                    using (Font font = new Font("Segoe UI", 12))

                    using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })

                    {

                        graphics.DrawString(code, font, Brushes.Black, new RectangleF(0, height + 10, barcodeBitmap.Width, 40), sf);

                    }

                }

                _pbBarcode.Image = barcodeBitmap;

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Ошибка при генерации штрихкода: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

    }
