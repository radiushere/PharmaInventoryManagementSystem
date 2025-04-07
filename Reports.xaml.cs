using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace SimpleLoginWPF
{
    public partial class Reports : Window
    {
        // Sample data classes
        public class SalesData
        {
            public string Date { get; set; }
            public string Product { get; set; }
            public string Amount { get; set; }
        }

        public class InventoryData
        {
            public string Product { get; set; }
            public int Stock { get; set; }
            public string Status { get; set; }
        }

        public Reports()
        {
            InitializeComponent();
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            // Initialize Recent Sales Data
            var recentSales = new ObservableCollection<SalesData>
            {
                new SalesData { Date = "2024-03-15", Product = "Paracetamol 500mg", Amount = "Rs. 12,400" },
                new SalesData { Date = "2024-03-14", Product = "Amoxicillin 250mg", Amount = "Rs. 8,750" },
                new SalesData { Date = "2024-03-13", Product = "Omeprazole 20mg", Amount = "Rs. 23,100" },
                new SalesData { Date = "2024-03-12", Product = "Lisinopril 10mg", Amount = "Rs. 15,300" }
            };

            // Initialize Inventory Alerts Data
            var inventoryAlerts = new ObservableCollection<InventoryData>
            {
                new InventoryData { Product = "Ibuprofen 400mg", Stock = 45, Status = "Low Stock" },
                new InventoryData { Product = "Aspirin 75mg", Stock = 120, Status = "Adequate" },
                new InventoryData { Product = "Metformin 500mg", Stock = 28, Status = "Critical" },
                new InventoryData { Product = "Atorvastatin 20mg", Stock = 150, Status = "Adequate" }
            };

            // Assign to DataGrids (make sure to add x:Name attributes in XAML)
            //RecentSalesDataGrid.ItemsSource = recentSales;
            //InventoryAlertsDataGrid.ItemsSource = inventoryAlerts;
        }

        // Navigation handlers
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Hide();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Reports();
            page.Show();
            this.Hide();
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            var page = new Partners();
            page.Show();
            this.Hide();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            var orders = new Orders();
            orders.Show();
            this.Close();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
        {
            var distributors = new Distributors();
            distributors.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }

        // Report period selection handler
        private void ReportPeriodChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        // Download report handler
        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Report download initiated...", "Download Report",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Profile handler
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Show();
        }

        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var page = new Purchases();
            page.Show();
            this.Close();
        }
    }
}