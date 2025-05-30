using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using LiveCharts;
using LiveCharts.Wpf;
using System.IO; // Required for file operations
using Microsoft.Win32; // Required for SaveFileDialog

namespace SimpleLoginWPF
{
    public partial class Reports : Window, INotifyPropertyChanged
    {
        private const string DB_CONNECTION_STRING = "server=localhost;database=learndb;user=root;password=doll9876;SslMode=None;";

        // Data Models (Keep these as they are clean and describe your data)
        public class SalesByPeriod
        {
            public string Period { get; set; }
            public double TotalAmount { get; set; }
        }

        public class ProductSale
        {
            public DateTime SaleDate { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public double TotalAmount { get; set; }
        }

        public class InventoryStatus
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public string Status { get; set; }
        }

        // LiveCharts Properties
        public SeriesCollection SalesSeries { get; set; }

        private string[] _chartLabels;
        public string[] ChartLabels
        {
            get { return _chartLabels; }
            set
            {
                _chartLabels = value;
                OnPropertyChanged(nameof(ChartLabels));
            }
        }

        public Func<double, string> Formatter { get; set; }

        // DataGrid ObservableCollections
        public ObservableCollection<ProductSale> RecentSalesData { get; set; }
        public ObservableCollection<InventoryStatus> InventoryAlertsData { get; set; }

        public Reports()
        {
            SalesSeries = new SeriesCollection();
            InitializeComponent();
            AccessLevelControl();
            DataContext = this;
            Formatter = value => value.ToString("N0");
            RecentSalesData = new ObservableCollection<ProductSale>();
            InventoryAlertsData = new ObservableCollection<InventoryStatus>();
        }

        public void AccessLevelControl()
        {
            string role = UserSession.Role;

            switch (role)
            {
                case "Admin":
                    DashboardButton.Visibility = Visibility.Visible;
                    InventoryButton.Visibility = Visibility.Visible;
                    ReportsButton.Visibility = Visibility.Visible;
                    PartnersButton.Visibility = Visibility.Visible;
                    OrdersButton.Visibility = Visibility.Visible;
                    DistributorsButton.Visibility = Visibility.Visible;
                    PurchasesButton.Visibility = Visibility.Visible;
                    AdminButton.Visibility = Visibility.Visible;
                    break;

                case "Manager":
                    DashboardButton.Visibility = Visibility.Visible;
                    InventoryButton.Visibility = Visibility.Visible;
                    ReportsButton.Visibility = Visibility.Visible;
                    PartnersButton.Visibility = Visibility.Visible;
                    OrdersButton.Visibility = Visibility.Visible;
                    DistributorsButton.Visibility = Visibility.Visible;
                    PurchasesButton.Visibility = Visibility.Visible;
                    AdminButton.Visibility = Visibility.Collapsed;
                    break;

                case "Warehouse Manager":
                    DashboardButton.Visibility = Visibility.Visible;
                    InventoryButton.Visibility = Visibility.Visible;
                    ReportsButton.Visibility = Visibility.Visible;
                    PartnersButton.Visibility = Visibility.Collapsed;
                    OrdersButton.Visibility = Visibility.Visible;
                    DistributorsButton.Visibility = Visibility.Visible;
                    PurchasesButton.Visibility = Visibility.Visible;
                    AdminButton.Visibility = Visibility.Collapsed;
                    break;

                case "Product Specialist":
                    DashboardButton.Visibility = Visibility.Visible;
                    InventoryButton.Visibility = Visibility.Visible;
                    ReportsButton.Visibility = Visibility.Visible;
                    PartnersButton.Visibility = Visibility.Collapsed;
                    OrdersButton.Visibility = Visibility.Visible;
                    DistributorsButton.Visibility = Visibility.Visible;
                    PurchasesButton.Visibility = Visibility.Visible;
                    AdminButton.Visibility = Visibility.Collapsed;
                    break;

                case "Distributor":
                    DashboardButton.Visibility = Visibility.Visible;
                    InventoryButton.Visibility = Visibility.Visible;
                    ReportsButton.Visibility = Visibility.Visible;
                    PartnersButton.Visibility = Visibility.Collapsed;
                    OrdersButton.Visibility = Visibility.Visible;
                    DistributorsButton.Visibility = Visibility.Visible;
                    PurchasesButton.Visibility = Visibility.Visible;
                    AdminButton.Visibility = Visibility.Collapsed;
                    break;

                default:
                    MessageBox.Show("Unknown role: " + role);
                    break;
            }
        }


        private void Reports_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMetricCards();
            LoadSalesChart("Monthly");
            LoadRecentSales();
            LoadInventoryAlerts();
        }

        private string GetScalarValue(MySqlConnection connection, string query, string formatString)
        {
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    return string.Format(formatString, Convert.ToDouble(result));
                }
            }
            return string.Format(formatString, 0);
        }

        private double GetDoubleScalar(MySqlConnection connection, string query)
        {
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    return Convert.ToDouble(result);
                }
            }
            return 0;
        }

        private void LoadMetricCards()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(DB_CONNECTION_STRING))
                {
                    connection.Open();
                    GrossProfitValue.Text = GetScalarValue(connection, "SELECT SUM(total_amount) FROM sales;", "Rs. {0:N0}");
                    TotalSalesValue.Text = GetScalarValue(connection, "SELECT SUM(quantity) FROM sales;", "{0:N0} Items");
                    InventoryValue.Text = GetScalarValue(connection, "SELECT SUM(p.quantity * p.price) FROM products p;", "Rs. {0:N0}");
                    double totalSales = GetDoubleScalar(connection, "SELECT SUM(total_amount) FROM sales;");
                    double totalPurchases = GetDoubleScalar(connection, "SELECT SUM(total_amount) FROM product_purchases;");
                    NetProfitValue.Text = $"Rs. {(totalSales - totalPurchases):N0}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading metric cards: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSalesChart(string period)
        {
            try
            {
                string query = "";
                switch (period)
                {
                    case "Daily":
                        query = @"SELECT DATE_FORMAT(sale_date, '%Y-%m-%d') AS Period, SUM(total_amount) AS TotalAmount
                                  FROM sales WHERE sale_date >= CURDATE() - INTERVAL 7 DAY
                                  GROUP BY Period ORDER BY Period;";
                        break;
                    case "Weekly":
                        query = @"SELECT DATE_FORMAT(sale_date, '%Y-%u') AS Period, SUM(total_amount) AS TotalAmount
                                  FROM sales WHERE sale_date >= CURDATE() - INTERVAL 12 WEEK
                                  GROUP BY Period ORDER BY Period;";
                        break;
                    case "Monthly":
                        query = @"SELECT DATE_FORMAT(sale_date, '%Y-%m') AS Period, SUM(total_amount) AS TotalAmount
                                  FROM sales WHERE sale_date >= CURDATE() - INTERVAL 12 MONTH
                                  GROUP BY Period ORDER BY Period;";
                        break;
                    case "Yearly":
                        query = @"SELECT DATE_FORMAT(sale_date, '%Y') AS Period, SUM(total_amount) AS TotalAmount
                                  FROM sales WHERE sale_date >= CURDATE() - INTERVAL 5 YEAR
                                  GROUP BY Period ORDER BY Period;";
                        break;
                    default:
                        MessageBox.Show("Invalid time period selected.");
                        return;
                }

                List<SalesByPeriod> salesData = new List<SalesByPeriod>();
                using (MySqlConnection connection = new MySqlConnection(DB_CONNECTION_STRING))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                salesData.Add(new SalesByPeriod
                                {
                                    Period = reader.GetString("Period"),
                                    TotalAmount = reader.GetDouble("TotalAmount")
                                });
                            }
                        }
                    }
                }

                if (!salesData.Any())
                {
                    salesData.Add(new SalesByPeriod { Period = "No Data", TotalAmount = 0 });
                }

                Dispatcher.Invoke(() =>
                {
                    SalesSeries.Clear();
                    SalesSeries.Add(new LineSeries
                    {
                        Title = "Sales",
                        Values = new ChartValues<double>(salesData.Select(s => s.TotalAmount)),
                        PointGeometrySize = 8,
                        StrokeThickness = 2
                    });
                    ChartLabels = salesData.Select(s => s.Period).ToArray();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales chart data: {ex.Message}", "Chart Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentSales()
        {
            RecentSalesData.Clear();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(DB_CONNECTION_STRING))
                {
                    connection.Open();
                    string query = @"SELECT s.sale_date, p.product_name, s.quantity, s.total_amount
                                     FROM sales s
                                     JOIN products p ON s.product_id = p.product_id
                                     ORDER BY s.sale_date DESC LIMIT 10;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RecentSalesData.Add(new ProductSale
                                {
                                    SaleDate = reader.GetDateTime("sale_date"),
                                    ProductName = reader.GetString("product_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    TotalAmount = reader.GetDouble("total_amount")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recent sales: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInventoryAlerts()
        {
            InventoryAlertsData.Clear();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(DB_CONNECTION_STRING))
                {
                    connection.Open();
                    string query = @"SELECT product_name, quantity,
                                     CASE
                                         WHEN quantity < 10 THEN 'Critical'
                                         WHEN quantity < 50 THEN 'Low Stock'
                                         ELSE 'Adequate'
                                     END AS Status
                                     FROM products
                                     ORDER BY quantity ASC LIMIT 10;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                InventoryAlertsData.Add(new InventoryStatus
                                {
                                    ProductName = reader.GetString("product_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    Status = reader.GetString("Status")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory alerts: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimePeriod_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.IsChecked == true)
            {
                LoadSalesChart(rb.Content.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- NEW LOGIC FOR DOWNLOAD REPORT BUTTON ---
        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a SaveFileDialog to allow the user to choose the save location and filename
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Sales_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt", // Default filename
                    DefaultExt = ".txt", // Default file extension
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" // Filter for file types
                };

                if (saveFileDialog.ShowDialog() == true) // If the user clicks Save
                {
                    string filePath = saveFileDialog.FileName;

                    // Build the report content
                    System.Text.StringBuilder reportContent = new System.Text.StringBuilder();

                    reportContent.AppendLine("--- Sales Report ---");
                    reportContent.AppendLine($"Generated On: {DateTime.Now}");
                    reportContent.AppendLine("--------------------");
                    reportContent.AppendLine();

                    // Add Metric Card Data
                    reportContent.AppendLine("METRIC OVERVIEW:");
                    reportContent.AppendLine($"Gross Revenue: {GrossProfitValue.Text}");
                    reportContent.AppendLine($"Total Profit: {NetProfitValue.Text}");
                    reportContent.AppendLine($"Total Items Sold: {TotalSalesValue.Text}");
                    reportContent.AppendLine($"Current Inventory Value: {InventoryValue.Text}");
                    reportContent.AppendLine();

                    // Add Recent Sales Data
                    if (RecentSalesData.Any())
                    {
                        reportContent.AppendLine("RECENT SALES:");
                        reportContent.AppendLine("Sale Date    | Product Name           | Qty | Total Amount");
                        reportContent.AppendLine("---------------------------------------------------------");
                        foreach (var sale in RecentSalesData)
                        {
                            reportContent.AppendLine($"{sale.SaleDate:yyyy-MM-dd} | {sale.ProductName,-22} | {sale.Quantity,-3} | {sale.TotalAmount,12:N2}");
                        }
                        reportContent.AppendLine();
                    }
                    else
                    {
                        reportContent.AppendLine("RECENT SALES: No recent sales data available.");
                        reportContent.AppendLine();
                    }


                    // Add Inventory Alerts Data
                    if (InventoryAlertsData.Any())
                    {
                        reportContent.AppendLine("INVENTORY ALERTS:");
                        reportContent.AppendLine("Product Name           | Current Stock | Status");
                        reportContent.AppendLine("-------------------------------------------------");
                        foreach (var alert in InventoryAlertsData)
                        {
                            reportContent.AppendLine($"{alert.ProductName,-22} | {alert.Quantity,-13} | {alert.Status}");
                        }
                        reportContent.AppendLine();
                    }
                    else
                    {
                        reportContent.AppendLine("INVENTORY ALERTS: No inventory alerts.");
                        reportContent.AppendLine();
                    }

                    // Write the content to the chosen file
                    File.WriteAllText(filePath, reportContent.ToString());

                    MessageBox.Show($"Report successfully saved to:\n{filePath}", "Report Downloaded",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating or saving report: {ex.Message}", "Report Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Navigation Handlers (No changes needed here) ---
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You are already on the Reports page.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            var page = new Partners();
            page.Show();
            this.Close();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            var orders = new Orders();
            orders.Show();
            this.Close();
        }

        private void Distributor_Click(object sender, RoutedEventArgs e)
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

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Show();
        }

        private void Purchases_Click(object gloomy, RoutedEventArgs e)
        {
            var page = new Purchases();
            page.Show();
            this.Close();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            var adminPage = new AdminDashboard();
            adminPage.Show();
            this.Close();
        }
    }
}