using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class Reports : Window, INotifyPropertyChanged
    {
        private const string DB_CONNECTION_STRING = "server=localhost;database=learndb;user=root;password=doll9876;SslMode=None;";

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

        private ReportData LoadReportData()
        {
            var reportData = new ReportData();

            string connectionString = "server=localhost;database=learndb;user=root;password=doll9876;SslMode=None;";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new MySqlCommand(@"SELECT IFNULL(SUM(total_amount), 0) as total_revenue, IFNULL(SUM(quantity), 0) as total_items_sold FROM sales;", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        reportData.TotalSalesRevenue = reader.GetDecimal("total_revenue");
                        reportData.TotalItemsSold = reader.GetInt32("total_items_sold");
                    }
                }

                using (var cmd = new MySqlCommand(@"SELECT IFNULL(SUM(total_amount),0) as total_purchase_cost FROM product_purchases;", connection))
                {
                    var totalPurchaseCost = Convert.ToDecimal(cmd.ExecuteScalar());
                    reportData.TotalProfit = reportData.TotalSalesRevenue - totalPurchaseCost;
                }

                using (var cmd = new MySqlCommand(@"SELECT IFNULL(SUM(price * quantity),0) as total_inventory_value FROM products;", connection))
                {
                    reportData.TotalInventoryValue = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                using (var cmd = new MySqlCommand(@"SELECT s.sale_date, p.product_name, s.quantity, s.total_amount FROM sales s LEFT JOIN products p ON s.product_id = p.product_id ORDER BY s.sale_date DESC LIMIT 10;", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.RecentSales.Add(new Sale
                        {
                            SaleDate = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0),
                            ProductName = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                            Quantity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            TotalAmount = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                        });
                    }
                }

                using (var cmd = new MySqlCommand(@"SELECT n.date, p.product_name, n.message, n.status FROM notifications n LEFT JOIN products p ON n.product_id = p.product_id ORDER BY n.date DESC LIMIT 10;", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.InventoryAlerts.Add(new NotificationAlert
                        {
                            Date = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0),
                            ProductName = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                            Message = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Status = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        });
                    }
                }

                using (var cmd = new MySqlCommand(@"SELECT name, region, service_area, distributor_type, email FROM distributors ORDER BY dist_id DESC LIMIT 10;", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.Distributors.Add(new Distributor
                        {
                            Name = reader.IsDBNull(0) ? "N/A" : reader.GetString(0),
                            Region = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            ServiceArea = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            DistributorType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        });
                    }
                }
            }

            return reportData;
        }

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

        public ObservableCollection<ProductSale> RecentSalesData { get; set; }
        public ObservableCollection<InventoryStatus> InventoryAlertsData { get; set; }

        public Reports()
        {
            SalesSeries = new SeriesCollection();
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
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

        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Full_Sales_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = ".pdf",
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() != true) return;

                string filePath = saveFileDialog.FileName;
                var reportData = LoadReportData();

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        page.Size(PageSizes.A4);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                        // Define colors for the red theme
                        Color darkRed = Color.FromRGB(139, 0, 0);
                        Color mediumRed = Color.FromRGB(205, 38, 38);
                        Color lightRed = Color.FromRGB(255, 165, 165);

                        // Header
                        page.Header().Height(100).Row(row =>
                        {
                            row.RelativeColumn().AlignLeft().Height(80).Image("C:\\Users\\DELL\\source\\repos\\SimpleLoginWPF\\Assets\\aliflogo-removebg-preview.png", ImageScaling.FitArea);
                            row.RelativeColumn().AlignCenter().Text("Full Sales & Inventory Report").FontSize(22).Bold().FontColor(darkRed);
                            row.RelativeColumn().AlignRight().Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).FontSize(10);
                        });

                        // Content
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            col.Spacing(10);

                            // Summary Metrics Section
                            col.Item().Text("SUMMARY METRICS").SemiBold().FontSize(16).FontColor(darkRed);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Metric").FontColor(darkRed);
                                    header.Cell().Text("Value").FontColor(darkRed);
                                });

                                table.Cell().Text("Total Sales Revenue:").FontColor(darkRed);
                                table.Cell().Text($"{reportData.TotalSalesRevenue:C}");

                                table.Cell().Text("Total Profit:").FontColor(darkRed);
                                table.Cell().Text($"{reportData.TotalProfit:C}");

                                table.Cell().Text("Total Items Sold:").FontColor(darkRed);
                                table.Cell().Text(reportData.TotalItemsSold.ToString());

                                table.Cell().Text("Total Inventory Value:").FontColor(darkRed);
                                table.Cell().Text($"{reportData.TotalInventoryValue:C}");
                            });

                            // Recent Sales Section
                            col.Item().Text("RECENT SALES").SemiBold().FontSize(16).FontColor(darkRed);
                            if (reportData.RecentSales.Count == 0)
                            {
                                col.Item().Text("No recent sales data available.");
                            }
                            else
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Sale Date").FontColor(darkRed);
                                        header.Cell().Text("Product Name").FontColor(darkRed);
                                        header.Cell().AlignRight().Text("Qty").FontColor(darkRed);
                                        header.Cell().AlignRight().Text("Total Amount").FontColor(darkRed);
                                    });

                                    foreach (var sale in reportData.RecentSales)
                                    {
                                        table.Cell().Text(sale.SaleDate.ToString("yyyy-MM-dd"));
                                        table.Cell().Text(sale.ProductName ?? "N/A");
                                        table.Cell().AlignRight().Text(sale.Quantity.ToString());
                                        table.Cell().AlignRight().Text($"{sale.TotalAmount:C}");
                                    }
                                });
                            }

                            // Inventory Alerts Section
                            col.Item().Text("INVENTORY ALERTS").SemiBold().FontSize(16).FontColor(darkRed);
                            if (reportData.InventoryAlerts.Count == 0)
                            {
                                col.Item().Text("No inventory alerts.");
                            }
                            else
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(6);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Product Name").FontColor(darkRed);
                                        header.Cell().Text("Date").FontColor(darkRed);
                                        header.Cell().Text("Message").FontColor(darkRed);
                                        header.Cell().Text("Status").FontColor(darkRed);
                                    });

                                    foreach (var alert in reportData.InventoryAlerts)
                                    {
                                        table.Cell().Text(alert.ProductName ?? "N/A");
                                        table.Cell().Text(alert.Date.ToString("yyyy-MM-dd"));
                                        table.Cell().Text(alert.Message ?? "");
                                        table.Cell().Text(alert.Status ?? "");
                                    }
                                });
                            }

                            // Distributors Section
                            col.Item().Text("DISTRIBUTORS").SemiBold().FontSize(16).FontColor(darkRed);
                            if (reportData.Distributors.Count == 0)
                            {
                                col.Item().Text("No distributor data.");
                            }
                            else
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(5);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Name").FontColor(darkRed);
                                        header.Cell().Text("Region").FontColor(darkRed);
                                        header.Cell().Text("Service Area").FontColor(darkRed);
                                        header.Cell().Text("Type").FontColor(darkRed);
                                        header.Cell().Text("Email").FontColor(darkRed);
                                    });

                                    foreach (var dist in reportData.Distributors)
                                    {
                                        table.Cell().Text(dist.Name ?? "N/A");
                                        table.Cell().Text(dist.Region ?? "");
                                        table.Cell().Text(dist.ServiceArea ?? "");
                                        table.Cell().Text(dist.DistributorType ?? "");
                                        table.Cell().Text(dist.Email ?? "");
                                    }
                                });
                            }

                            // Signature Placeholder
                            col.Item().PaddingTop(40).Column(column =>
                            {
                                column.Item().LineHorizontal(1).LineColor(darkRed);
                                column.Item().PaddingTop(10).Text("Authorized Signature").FontColor(darkRed).FontSize(12).Bold();
                            });
                        });

                    });
                });

                document.GeneratePdf(filePath);
                MessageBox.Show($"PDF report generated successfully:\n{filePath}", "Report Generated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        class ReportData
        {
            public decimal TotalSalesRevenue { get; set; }
            public decimal TotalProfit { get; set; }
            public int TotalItemsSold { get; set; }
            public decimal TotalInventoryValue { get; set; }

            public List<Sale> RecentSales { get; set; } = new List<Sale>();
            public List<NotificationAlert> InventoryAlerts { get; set; } = new List<NotificationAlert>();
            public List<Distributor> Distributors { get; set; } = new List<Distributor>();
        }

        class Sale
        {
            public DateTime SaleDate { get; set; }
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal TotalAmount { get; set; }
        }

        class NotificationAlert
        {
            public string ProductName { get; set; } = "";
            public DateTime Date { get; set; }
            public string Message { get; set; } = "";
            public string Status { get; set; } = "";
        }

        class Distributor
        {
            public string Name { get; set; } = "";
            public string Region { get; set; } = "";
            public string ServiceArea { get; set; } = "";
            public string DistributorType { get; set; } = "";
            public string Email { get; set; } = "";
        }

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

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadReportData();
        }
    }
}
