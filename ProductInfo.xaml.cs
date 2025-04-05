using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SimpleLoginWPF
{
    public partial class ProductInfo : Window
    {
        public class SalesData
        {
            public string OrderID { get; set; }
            public string Partner { get; set; }
            public DateTime OrderDate { get; set; }
            public int Quantity { get; set; }
            public decimal Total { get; set; }
        }

        public ProductInfo(int productId)
        {
            InitializeComponent();
            LoadProductDetails(productId);
            LoadDistributionHistory(productId);
        }

        private void LoadProductDetails(int productId)
        {
            ProductId.Text = "NDC 12345-678-90";
            ProductName.Text = "CardioSafe 100mg Tablets";
            ProductCategory.Text = "Antihypertensives";
            StockStatus.Text = "Active Batch";

            // Product Details
            UnitPrice.Text = "Lisinopril";
            BatchNumber.Text = "BT-LIS2024-01";
            ManufactureDate.Text = "January 15, 2024";
            ExpiryDate.Text = "January 2026";

            StockQuantity.Text = "5,000 Bottles";
            MinStockLevel.Text = "2-8°C Refrigeration";
            SupplierName.Text = "PharmaCare Global Ltd.";
            StorageLocation.Text = "Warehouse 3, Cold Storage A";

            ProductDescription.Text = "ACE inhibitor for hypertension management. White, oval-shaped film-coated tablets.";

            try
            {
                // Load medicine image
                ProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
            }
            catch
            {
                // Fallback to default image
            }
        }

        private void LoadDistributionHistory(int productId)
        {
            var distributions = new List<SalesData>
            {
                new SalesData
                {
                    OrderID = "LOT-2024-001",
                    Partner = "National Pharma Distributors",
                    OrderDate = DateTime.Now.AddDays(-30),
                    Quantity = 1200,
                    Total = 240000.00m
                },
                new SalesData
                {
                    OrderID = "LOT-2023-045",
                    Partner = "Regional Medical Supply",
                    OrderDate = DateTime.Now.AddDays(-90),
                    Quantity = 750,
                    Total = 150000.00m
                },
                new SalesData
                {
                    OrderID = "LOT-2023-038",
                    Partner = "Hospital Network Co.",
                    OrderDate = DateTime.Now.AddDays(-150),
                    Quantity = 2000,
                    Total = 400000.00m
                }
            };

            SalesDataGrid.ItemsSource = distributions;
        }

        // Existing event handlers remain the same
        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();
        private void EditProduct_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Batch update functionality");
        private void PlaceOrder_Click(object sender, RoutedEventArgs e) => MessageBox.Show("New distribution workflow");
    }
}