using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace SimpleLoginWPF
{
    public partial class Dashboard : Window
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public SeriesCollection ChartSeries { get; set; }
        public SeriesCollection OrderChartSeries { get; set; }
        public string[] Labels { get; set; }
        public string[] OrderLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> OrderYFormatter { get; set; }

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;
            LoadDashboardData();
            InitializeCharts();
            LoadInventoryData();
        }

        private void LoadDashboardData()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Load Sales Overview
                var salesOverviewCommand = new MySqlCommand("SELECT SUM(total_amount) FROM sales", connection);
                var sales = salesOverviewCommand.ExecuteScalar()?.ToString() ?? "0";
                Sales = $"Rs {sales}";

                var revenueCommand = new MySqlCommand("SELECT SUM(total_amount) FROM sales", connection);
                var revenue = revenueCommand.ExecuteScalar()?.ToString() ?? "0";
                Revenue = $"Rs {revenue}";

                var profitCommand = new MySqlCommand("SELECT SUM(total_amount) FROM sales", connection);
                var profit = profitCommand.ExecuteScalar()?.ToString() ?? "0";
                Profit = $"Rs {profit}";

                var costCommand = new MySqlCommand("SELECT SUM(total_amount) FROM product_purchases", connection);
                var cost = costCommand.ExecuteScalar()?.ToString() ?? "0";
                Cost = $"Rs {cost}";

                // Load Inventory Summary
                var quantityInHandCommand = new MySqlCommand("SELECT SUM(quantity) FROM products", connection);
                var quantityInHand = quantityInHandCommand.ExecuteScalar()?.ToString() ?? "0";
                QuantityInHand = quantityInHand;

                // Placeholder for "To be received" as it requires specific business logic
                ToBeReceived = "200";

                // Load Purchase Overview
                var purchaseCommand = new MySqlCommand("SELECT COUNT(*) FROM product_purchases", connection);
                var purchase = purchaseCommand.ExecuteScalar()?.ToString() ?? "0";
                Purchase = purchase;

                PurchaseCost = Cost; // Reuse cost from above

                var cancelCommand = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE status = 'Cancelled'", connection);
                var cancel = cancelCommand.ExecuteScalar()?.ToString() ?? "0";
                Cancel = cancel;

                var returnCommand = new MySqlCommand("SELECT SUM(price) FROM orders WHERE status = 'Completed'", connection);
                var returnAmount = returnCommand.ExecuteScalar()?.ToString() ?? "0";
                Return = $"Rs {returnAmount}";

                // Load Product Summary
                var distributorsCommand = new MySqlCommand("SELECT COUNT(*) FROM distributors", connection);
                var distributors = distributorsCommand.ExecuteScalar()?.ToString() ?? "0";
                NumberOfDistributors = distributors;

                var categoriesCommand = new MySqlCommand("SELECT COUNT(DISTINCT category) FROM products", connection);
                var categories = categoriesCommand.ExecuteScalar()?.ToString() ?? "0";
                NumberOfCategories = categories;
            }
        }

        private void LoadInventoryData()
        {
            var inventoryItems = new List<InventoryItem>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT product_id, product_name, quantity, price FROM products ORDER BY quantity DESC LIMIT 10", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inventoryItems.Add(new InventoryItem
                        {
                            ProductId = reader["product_id"].ToString(),
                            ProductName = reader["product_name"].ToString(),
                            Quantity = Convert.ToInt32(reader["quantity"]),
                            Price = Convert.ToDecimal(reader["price"]),
                            LastUpdated = DateTime.Now // Placeholder, replace with actual field if available
                        });
                    }
                }
            }

            InventoryDataGrid.ItemsSource = inventoryItems;
        }

        private void InitializeCharts()
        {
            var salesValues = new ChartValues<double>();
            var purchaseValues = new ChartValues<double>();
            var orderedValues = new ChartValues<double>();
            var deliveredValues = new ChartValues<double>();

            string[] monthLabels = new string[12];
            for (int i = 0; i < 12; i++)
                monthLabels[i] = new DateTime(DateTime.Now.Year, i + 1, 1).ToString("MMM");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Sales
                    var salesCmd = new MySqlCommand(@"
                SELECT MONTH(sale_date) AS month, SUM(total_amount) AS total
                FROM sales
                WHERE YEAR(sale_date) = YEAR(CURDATE())
                GROUP BY MONTH(sale_date);", conn);
                    var salesReader = salesCmd.ExecuteReader();

                    var monthlySales = new double[12];
                    while (salesReader.Read())
                    {
                        int month = salesReader.GetInt32("month") - 1;
                        double total = salesReader.IsDBNull(1) ? 0 : salesReader.GetDouble("total");
                        monthlySales[month] = total;
                    }
                    salesReader.Close();

                    // Purchases
                    var purchaseCmd = new MySqlCommand(@"
                SELECT MONTH(date) AS month, SUM(total_amount) AS total
                FROM product_purchases
                WHERE YEAR(date) = YEAR(CURDATE())
                GROUP BY MONTH(date);", conn);
                    var purchaseReader = purchaseCmd.ExecuteReader();

                    var monthlyPurchases = new double[12];
                    while (purchaseReader.Read())
                    {
                        int month = purchaseReader.GetInt32("month") - 1;
                        double total = purchaseReader.IsDBNull(1) ? 0 : purchaseReader.GetDouble("total");
                        monthlyPurchases[month] = total;
                    }
                    purchaseReader.Close();

                    // Orders (Placed)
                    var orderCmd = new MySqlCommand(@"
                SELECT MONTH(order_date) AS month, COUNT(*) AS total
                FROM orders
                WHERE YEAR(order_date) = YEAR(CURDATE())
                GROUP BY MONTH(order_date);", conn);
                    var orderReader = orderCmd.ExecuteReader();

                    var monthlyOrders = new double[12];
                    while (orderReader.Read())
                    {
                        int month = orderReader.GetInt32("month") - 1;
                        double total = orderReader.GetInt32("total");
                        monthlyOrders[month] = total;
                    }
                    orderReader.Close();

                    // Deliveries
                    var deliveryCmd = new MySqlCommand(@"
                SELECT MONTH(delivery_date) AS month, COUNT(*) AS total
                FROM orders
                WHERE YEAR(delivery_date) = YEAR(CURDATE())
                GROUP BY MONTH(delivery_date);", conn);
                    var deliveryReader = deliveryCmd.ExecuteReader();

                    var monthlyDeliveries = new double[12];
                    while (deliveryReader.Read())
                    {
                        int month = deliveryReader.GetInt32("month") - 1;
                        double total = deliveryReader.GetInt32("total");
                        monthlyDeliveries[month] = total;
                    }
                    deliveryReader.Close();

                    // Populate chart values
                    salesValues.AddRange(monthlySales);
                    purchaseValues.AddRange(monthlyPurchases);
                    orderedValues.AddRange(monthlyOrders);
                    deliveredValues.AddRange(monthlyDeliveries);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart data: " + ex.Message);
            }

            // Bind to Sales Chart
            ChartSeries = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Sales",
            Values = salesValues,
            Fill = System.Windows.Media.Brushes.ForestGreen
        },
        new ColumnSeries
        {
            Title = "Purchase",
            Values = purchaseValues,
            Fill = System.Windows.Media.Brushes.SkyBlue
        }
    };

            XAxes = new AxesCollection
    {
        new Axis { Labels = monthLabels }
    };

            YAxes = new AxesCollection
    {
        new Axis { LabelFormatter = value => value.ToString("C") }
    };

            // Bind to Order Chart
            OrderChartSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Ordered",
            Values = orderedValues,
            Stroke = System.Windows.Media.Brushes.Orange,
            Fill = System.Windows.Media.Brushes.Transparent,
            PointGeometry = DefaultGeometries.Circle
        },
        new LineSeries
        {
            Title = "Delivered",
            Values = deliveredValues,
            Stroke = System.Windows.Media.Brushes.MediumPurple,
            Fill = System.Windows.Media.Brushes.Transparent,
            PointGeometry = DefaultGeometries.Square
        }
    };

            OrderXAxes = new AxesCollection
    {
        new Axis { Labels = monthLabels }
    };

            OrderYAxes = new AxesCollection
    {
        new Axis { LabelFormatter = value => value.ToString("N0") }
    };
        }


        // Add these properties to your class
        public AxesCollection XAxes { get; set; }
        public AxesCollection YAxes { get; set; }
        public AxesCollection OrderXAxes { get; set; }
        public AxesCollection OrderYAxes { get; set; }


        public string Sales { get; set; }
        public string Revenue { get; set; }
        public string Profit { get; set; }
        public string Cost { get; set; }
        public string QuantityInHand { get; set; }
        public string ToBeReceived { get; set; }
        public string Purchase { get; set; }
        public string PurchaseCost { get; set; }
        public string Cancel { get; set; }
        public string Return { get; set; }
        public string NumberOfDistributors { get; set; }
        public string NumberOfCategories { get; set; }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            new Dashboard().Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            new Reports().Show();
            this.Close();
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            new Partners().Show();
            this.Close();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            new Orders().Show();
            this.Close();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
        {
            new Distributors().Show();
            this.Close();
        }

        private void Purchase_Click(object sender, RoutedEventArgs e)
        {
            new Purchases().Show();
            this.Close();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            new AdminDashboard().Show();
            this.Close();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            new UserProfile().Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }
    }

    public class InventoryItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
