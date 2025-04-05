using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleLoginWPF
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the dashboard with sample data
            InitializeDashboard();

            // Load sample inventory data
            LoadInventoryData();

            // Initialize the order summary chart
            InitializeOrderChart();
        }

        private void InitializeDashboard()
        {
            // This method would typically load real data from a database
            // For now, we'll just ensure the UI elements are properly initialized

            // The chart in XAML is already populated with sample data
            // In a real application, you would bind to actual data
        }

        private void LoadInventoryData()
        {
            // Sample inventory data - in a real app, this would come from a database
            var inventoryItems = new List<InventoryItem>
            {
                new InventoryItem { ProductId = "PH-1001", ProductName = "Paracetamol 500mg", Category = "Pain Relief", Quantity = 150, Price = 12.50m, Supplier = "MediCorp", LastUpdated = DateTime.Now.AddDays(-2) },
                new InventoryItem { ProductId = "PH-1002", ProductName = "Ibuprofen 200mg", Category = "Pain Relief", Quantity = 85, Price = 15.75m, Supplier = "HealthPlus", LastUpdated = DateTime.Now.AddDays(-5) },
                new InventoryItem { ProductId = "PH-1003", ProductName = "Cetirizine 10mg", Category = "Allergy", Quantity = 120, Price = 8.99m, Supplier = "PharmaDirect", LastUpdated = DateTime.Now.AddDays(-1) },
                new InventoryItem { ProductId = "PH-1004", ProductName = "Loratadine 10mg", Category = "Allergy", Quantity = 65, Price = 10.25m, Supplier = "MediCorp", LastUpdated = DateTime.Now.AddDays(-3) },
                new InventoryItem { ProductId = "PH-1005", ProductName = "Omeprazole 20mg", Category = "Acid Reducer", Quantity = 90, Price = 22.50m, Supplier = "HealthPlus", LastUpdated = DateTime.Now.AddDays(-7) },
                new InventoryItem { ProductId = "PH-1006", ProductName = "Vitamin C 1000mg", Category = "Vitamins", Quantity = 200, Price = 9.99m, Supplier = "PharmaDirect", LastUpdated = DateTime.Now.AddDays(-4) },
                new InventoryItem { ProductId = "PH-1007", ProductName = "Vitamin D3 1000IU", Category = "Vitamins", Quantity = 150, Price = 12.75m, Supplier = "MediCorp", LastUpdated = DateTime.Now.AddDays(-6) }
            };

            InventoryDataGrid.ItemsSource = inventoryItems;
        }

        private void InitializeOrderChart()
        {
            // Sample data for the order summary chart
            double[] orderedValues = { 3200, 2800, 3500, 3800, 4200 };
            double[] deliveredValues = { 3000, 2700, 3200, 3500, 4000 };

            // Clear any existing elements
            ChartCanvas.Children.Clear();

            // Draw the lines
            Polyline orderedLine = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0xDE, 0x9A, 0x4F)),
                StrokeThickness = 2,
                Points = new PointCollection()
            };

            Polyline deliveredLine = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0x3F, 0x87, 0xE5)),
                StrokeThickness = 2,
                Points = new PointCollection()
            };

            // Scale factor to fit the data in the available space
            double yScale = 160.0 / 5000.0; // 160 is the canvas height for 4000 value
            double xStep = 54.0; // Width between points

            for (int i = 0; i < 5; i++)
            {
                double x = i * xStep + xStep / 2;

                // Ordered line points
                double orderedY = 160 - (orderedValues[i] * yScale);
                orderedLine.Points.Add(new Point(x, orderedY));

                // Delivered line points
                double deliveredY = 160 - (deliveredValues[i] * yScale);
                deliveredLine.Points.Add(new Point(x, deliveredY));
            }

            ChartCanvas.Children.Add(orderedLine);
            ChartCanvas.Children.Add(deliveredLine);

            // Add data point markers
            for (int i = 0; i < 5; i++)
            {
                double x = i * xStep + xStep / 2;

                // Ordered marker
                double orderedY = 160 - (orderedValues[i] * yScale);
                Ellipse orderedMarker = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromRgb(0xDE, 0x9A, 0x4F)),
                    Margin = new Thickness(x - 3, orderedY - 3, 0, 0)
                };
                ChartCanvas.Children.Add(orderedMarker);

                // Delivered marker
                double deliveredY = 160 - (deliveredValues[i] * yScale);
                Ellipse deliveredMarker = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromRgb(0x3F, 0x87, 0xE5)),
                    Margin = new Thickness(x - 3, deliveredY - 3, 0, 0)
                };
                ChartCanvas.Children.Add(deliveredMarker);
            }
        }

        #region Navigation Button Handlers

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would navigate to the dashboard view
            MessageBox.Show("Navigating to Dashboard", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var inventory = new MainWindow();
            inventory.Show();
            this.Hide();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            Reports reports = new Reports();
            reports.Show();
            this.Hide();
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigating to Suppliers", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            var page = new Orders();
            page.Show();
            this.Hide();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
        {
            var page = new Distributors();
            page.Show();
            this.Hide();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigating to Settings", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var page = new Login();
            page.Show();
            this.Hide();
        }

        #endregion

        #region Search Box Handlers

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search product, supplier, order")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search product, supplier, order";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(SearchBox.Text) && SearchBox.Text != "Search product, supplier, order")
            {
                // Perform search
                MessageBox.Show($"Searching for: {SearchBox.Text}", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Action Button Handlers

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You have no new notifications", "Notifications", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exporting inventory data...", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Printing inventory report...", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening add new item dialog...", "Add Item", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                MessageBox.Show($"Editing product: {productId}", "Edit Item", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                var result = MessageBox.Show($"Are you sure you want to delete product {productId}?", "Delete Item", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show($"Product {productId} deleted", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            Partners partners = new Partners();
            partners.Show();
            this.Hide();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Show();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard();
            adminDashboard.Show();
            this.Hide();
        }
    }

    // Simple data model for inventory items
    public class InventoryItem
    {
        public required string ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string Category { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public required string Supplier { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
