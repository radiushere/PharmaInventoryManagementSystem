using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class Orders : Window
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private DataTable ordersDataTable = new DataTable();

        public Orders()
        {
            InitializeComponent();
            AccessLevelControl();
            LoadOrdersData();
            SetupStats();
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

        public void LoadOrdersData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"SELECT
                                o.order_id AS OrderID,
                                p.product_name AS ProductName,
                                o.order_date AS OrderDate,
                                o.delivery_date AS DeliveryDate,
                                o.status AS Status,
                                o.price AS TotalAmount
                              FROM orders o
                              JOIN products p ON o.product_id = p.product_id";

                    var adapter = new MySqlDataAdapter(query, conn);
                    ordersDataTable.Clear();
                    adapter.Fill(ordersDataTable);
                    OrdersDataGrid.ItemsSource = ordersDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupStats()
        {
            TotalOrdersCount.Text = ordersDataTable.Rows.Count.ToString();

            int pending = 0;
            int completed = 0;

            foreach (DataRow row in ordersDataTable.Rows)
            {
                if (row["Status"].ToString() == "Pending") pending++;
                if (row["Status"].ToString() == "Completed") completed++;
            }

            PendingOrdersCount.Text = pending.ToString();
            CompletedOrdersCount.Text = completed.ToString();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            ordersDataTable.DefaultView.RowFilter =
                $"ProductName LIKE '%{searchText}%' OR OrderID LIKE '%{searchText}%' OR Status LIKE '%{searchText}%'";
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Orders...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search Orders...";
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (FilterComboBox.SelectedIndex)
            {
                case 0: ordersDataTable.DefaultView.RowFilter = ""; break;
                case 1: ordersDataTable.DefaultView.RowFilter = "Status = 'Pending'"; break;
                case 2: ordersDataTable.DefaultView.RowFilter = "Status = 'Completed'"; break;
            }
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var newOrderWindow = new NewOrderWindow(this);
            newOrderWindow.Owner = this;
            newOrderWindow.ShowDialog();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button.DataContext as DataRowView;
            if (order != null)
            {
                // Convert OrderID from string to int
                if (int.TryParse(order.Row["OrderID"].ToString(), out int orderId))
                {
                    var detailPopup = new OrderDetails(orderId);
                    detailPopup.Owner = this;
                    detailPopup.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Invalid Order ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button.DataContext as DataRowView;
            if (order != null)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete order: {order.Row["OrderID"]}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            var query = "DELETE FROM orders WHERE order_id = @orderId";
                            using (var cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@orderId", order.Row["OrderID"]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadOrdersData(); // Refresh the data after deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveOrderToSales(int orderId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if the order is already in the sales table
                    var checkQuery = @"SELECT COUNT(*) FROM sales WHERE order_id = @orderId";
                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@orderId", orderId);
                        var count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            // Order already exists in sales table, no need to insert again
                            return;
                        }
                    }

                    // Fetch order details
                    var query = @"SELECT o.order_id, o.product_id, o.quantity, o.price, o.delivery_date
                          FROM orders o
                          WHERE o.order_id = @orderId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var productId = reader.GetInt32("product_id");
                                var quantity = reader.GetInt32("quantity");
                                var price = reader.GetFloat("price");
                                var deliveryDate = reader.GetDateTime("delivery_date");

                                // Insert into sales table
                                var insertQuery = @"INSERT INTO sales (product_id, quantity, payment_method, total_amount, sale_date)
                                            VALUES (@productId, @quantity, @paymentMethod, @totalAmount, @saleDate)";

                                using (var insertCmd = new MySqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@productId", productId);
                                    insertCmd.Parameters.AddWithValue("@quantity", quantity);
                                    insertCmd.Parameters.AddWithValue("@paymentMethod", "unspecified");
                                    insertCmd.Parameters.AddWithValue("@totalAmount", price);
                                    insertCmd.Parameters.AddWithValue("@saleDate", deliveryDate);

                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order to sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Navigation Methods
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            var page = new Orders();
            page.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        private void Distributor_Click(object sender, RoutedEventArgs e)
        {
            var page = new Distributors();
            page.Show();
            this.Hide();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            Reports reports = new Reports();
            reports.Show();
            this.Close();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Show();
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            Partners partners = new Partners();
            partners.Show();
            this.Close();
        }

        private void Distributors_Click(object sender, RoutedEventArgs e)
        {
            Distributors distributors = new Distributors();
            distributors.Show();
            this.Close();
        }

        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var page = new Purchases();
            page.Show();
            this.Close();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            new AdminDashboard().Show();
            this.Close();
        }
    }
}
