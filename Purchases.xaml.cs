using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class Purchases : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private DataTable purchasesDataTable = new DataTable();

        public class Purchase
        {
            public string PurchaseId { get; set; }
            public string Supplier { get; set; }
            public DateTime Date { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
        }

        public Purchases()
        {
            InitializeComponent();
            LoadPurchasesData();
            AccessLevelControl();
            UpdateStats();
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

        private void LoadPurchasesData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT pp.purchase_id AS PurchaseId, s.supplier_name AS Supplier,
                           pp.date AS Date, pp.total_amount AS TotalAmount, pp.status AS Status
                    FROM product_purchases pp
                    JOIN suppliers s ON pp.supplier_id = s.supplier_id";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    purchasesDataTable.Clear();
                    adapter.Fill(purchasesDataTable);
                    PurchasesDataGrid.ItemsSource = purchasesDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchases data: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStats()
        {
            TotalPurchases.Text = purchasesDataTable.Rows.Count.ToString();

            // Calculate the sum of TotalAmount for the current month
            DateTime now = DateTime.Now;
            var monthlyRows = purchasesDataTable.AsEnumerable()
                .Where(row => row.Field<DateTime>("Date").Month == now.Month && row.Field<DateTime>("Date").Year == now.Year)
                .Select(row => row.Field<decimal>("TotalAmount"));

            PendingOrders.Text = purchasesDataTable.Select("Status = 'Pending'").Length.ToString();
            TotalSuppliers.Text = purchasesDataTable.DefaultView.ToTable(true, "Supplier").Rows.Count.ToString();
        }


        // Navigation Handlers (Same as AdminDashboard)
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        // Purchase-specific Handlers
        private void AddPurchase_Click(object sender, RoutedEventArgs e)
        {
            var addPurchaseWindow = new AddPurchaseWindow();
            addPurchaseWindow.ShowDialog();
        }

        private void EditPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (PurchasesDataGrid.SelectedItem is DataRowView rowView)
            {
                var purchaseId = rowView["PurchaseId"].ToString();
                var purchaseDetailsWindow = new PurchaseDetails(int.Parse(purchaseId));
                purchaseDetailsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a purchase to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void DeletePurchase_Click(object sender, RoutedEventArgs e)
        {
            if (PurchasesDataGrid.SelectedItem is DataRowView rowView)
            {
                var purchaseId = rowView["PurchaseId"].ToString();

                // Confirm deletion
                var confirmResult = MessageBox.Show($"Are you sure you want to delete purchase: {purchaseId}?", "Confirm Deletion",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();

                            // Delete the purchase
                            var deleteQuery = "DELETE FROM product_purchases WHERE purchase_id = @purchaseId";
                            using (var deleteCmd = new MySqlCommand(deleteQuery, conn))
                            {
                                deleteCmd.Parameters.AddWithValue("@purchaseId", purchaseId);
                                int rowsAffected = deleteCmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show($"Purchase {purchaseId} deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    // Refresh the data grid
                                    LoadPurchasesData();
                                }
                                else
                                {
                                    MessageBox.Show($"Purchase {purchaseId} not found or could not be deleted.", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting purchase: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a purchase to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Existing handlers from AdminDashboard
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Purchases...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search Purchases...";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filterText = SearchBox.Text.Trim().Replace("'", "''");
            if (filterText == "Search Purchases...")
            {
                filterText = string.Empty;
            }

            string rowFilterString = string.Empty;
            if (!string.IsNullOrWhiteSpace(filterText))
            {
                rowFilterString = $"PurchaseId LIKE '%{filterText}%' OR " +
                                  $"Supplier LIKE '%{filterText}%' OR " +
                                  $"Status LIKE '%{filterText}%'";
            }

            try
            {
                purchasesDataTable.DefaultView.RowFilter = rowFilterString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RowFilter: " + ex.Message);
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var filterComboBox = sender as ComboBox;
            if (filterComboBox.SelectedItem == null) return;

            var selectedFilter = (filterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string rowFilterString = string.Empty;

            switch (selectedFilter)
            {
                case "This Month":
                    rowFilterString = $"MONTH(Date) = MONTH(CURRENT_DATE) AND YEAR(Date) = YEAR(CURRENT_DATE)";
                    break;
                case "Pending":
                    rowFilterString = $"Status = 'Pending'";
                    break;
                default:
                    rowFilterString = string.Empty;
                    break;
            }

            try
            {
                purchasesDataTable.DefaultView.RowFilter = rowFilterString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RowFilter: " + ex.Message);
            }
        }

        private void Purchases_Click(object sender, RoutedEventArgs e) { }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var addPurchaseWindow = new UserProfile();
            addPurchaseWindow.ShowDialog();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            var adminDashboard = new AdminDashboard();
            adminDashboard.Show();
            this.Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPurchasesData();
        }
    }
}
