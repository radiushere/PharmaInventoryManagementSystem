using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SimpleLoginWPF
{
    public partial class MainWindow : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private DispatcherTimer refreshTimer;
        private DataTable medicineDataTable = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            AccessLevelControl();
            LoadMedicinesData();
            SetupAutoRefresh();
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

        public void LoadMedicinesData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT
                        product_id AS ID,
                        product_name AS Name,
                        quantity AS Quantity,
                        price AS Price,
                        expiry_date AS ExpiryDate,
                        batch_num AS BatchNumber,
                        description AS Description,
                        product_image AS ImagePath,
                        category AS Category,
                        supplier_id AS SupplierID
                    FROM products";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    medicineDataTable.Clear(); // Clear previous data
                    adapter.Fill(medicineDataTable);
                    MedicinesDataGrid.ItemsSource = medicineDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading inventory data: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupAutoRefresh()
        {
            refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            refreshTimer.Tick += (s, e) =>
            {
                LoadMedicinesData();
                SetupStats(); // Also refresh stats when data refreshes
            };
            refreshTimer.Start();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (medicineDataTable == null || medicineDataTable.Columns.Count == 0)
            {
                Console.WriteLine("medicineDataTable is null or has no columns.");
                return;
            }

            string filterText = SearchBox.Text.Trim().Replace("'", "''");
            if (filterText == "Search Medicine...")
            {
                filterText = string.Empty;
            }

            string rowFilterString = string.Empty;
            if (!string.IsNullOrWhiteSpace(filterText))
            {
                rowFilterString = $"Name LIKE '%{filterText}%' OR " +
                                  $"BatchNumber LIKE '%{filterText}%' OR " +
                                  $"Category LIKE '%{filterText}%'";
            }

            try
            {
                medicineDataTable.DefaultView.RowFilter = rowFilterString;
            }
            catch (EvaluateException ex)
            {
                Console.WriteLine("Error in RowFilter: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error in SearchBox_TextChanged: " + ex.Message);
            }
        }

        public void SetupStats()
        {
            if (medicineDataTable == null) return;

            DataTable viewToCount = medicineDataTable.DefaultView.ToTable();
            ProductCount.Text = viewToCount.Rows.Count.ToString();

            int availableCount = 0;
            int lowstockCount = 0;

            foreach (DataRow row in viewToCount.Rows) // Iterate over the filtered view
            {
                if (row.Table.Columns.Contains("Quantity") && row["Quantity"] != DBNull.Value)
                {
                    var quantity = Convert.ToInt32(row["Quantity"]);
                    if (quantity > 0) availableCount++;
                    if (quantity < 5 && quantity > 0) lowstockCount++;
                }
            }

            AvailableCount.Text = availableCount.ToString();
            LowStockCount.Text = lowstockCount.ToString();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Medicine...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search Medicine...";
        }

        // --- Navigation Methods ---
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            LoadMedicinesData(); // Refresh current view
            SetupStats();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Reports();
            page.Show();
            this.Close();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            Orders orders = new Orders();
            orders.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) // "Add New Product" button
        {
            var newProductWindow = new NewProductWindow(this); // Pass current window if NewProductWindow needs to call back
            newProductWindow.Owner = this;
            newProductWindow.ShowDialog();
            LoadMedicinesData();
            SetupStats();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var page = new UserProfile();
            page.Show();
        }

        private void Distributor_Click(object sender, RoutedEventArgs e)
        {
            Distributors distributors = new Distributors();
            distributors.Show();
            this.Close();
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            Partners partners = new Partners();
            partners.Show();
            this.Close();
        }

        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var page = new Purchases();
            page.Show();
            this.Close();
        }

        // --- DataGrid Action Methods ---
        private void ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                DataRowView rowView = button.DataContext as DataRowView;
                if (rowView != null)
                {
                    int inventoryId = Convert.ToInt32(rowView["ID"]);
                    var productInfoPage = new ProductInfo(inventoryId);
                    productInfoPage.Owner = this;
                    productInfoPage.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Could not retrieve details for the selected item.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                DataRowView rowView = button.DataContext as DataRowView;
                if (rowView != null)
                {
                    int productIdToDelete = Convert.ToInt32(rowView["ID"]);

                    string productName = rowView["Name"] != DBNull.Value ? rowView["Name"].ToString() : "this item";

                    // Updated confirmation message to reflect deleting the entire product
                    MessageBoxResult confirmResult = MessageBox.Show(
                        $"Are you sure you want to delete the ENTIRE product '{productName}' (Product ID: {productIdToDelete}) " +
                        $"and ALL its related data (all inventory batches, categories, suppliers, shipments, notifications, orders, purchases, sales, etc.)? " +
                        $"This action is irreversible.",
                        "Confirm Product Deletion",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection(connectionString))
                            {
                                conn.Open();
                                using (MySqlTransaction transaction = conn.BeginTransaction())
                                {
                                    try
                                    {

                                        string deleteNotificationsQuery = "DELETE FROM notifications WHERE product_id = @ProductID";
                                        MySqlCommand cmdDeleteNotifications = new MySqlCommand(deleteNotificationsQuery, conn, transaction);
                                        cmdDeleteNotifications.Parameters.AddWithValue("@ProductID", productIdToDelete);
                                        cmdDeleteNotifications.ExecuteNonQuery();

                                        string deleteOrderItemsQuery = "DELETE FROM orders WHERE product_id = @ProductID";
                                        MySqlCommand cmdDeleteOrderItems = new MySqlCommand(deleteOrderItemsQuery, conn, transaction);
                                        cmdDeleteOrderItems.Parameters.AddWithValue("@ProductID", productIdToDelete);
                                        cmdDeleteOrderItems.ExecuteNonQuery();

                                        string deleteProductPurchasesQuery = "DELETE FROM product_purchases WHERE product_id = @ProductID";
                                        MySqlCommand cmdDeleteProductPurchases = new MySqlCommand(deleteProductPurchasesQuery, conn, transaction);
                                        cmdDeleteProductPurchases.Parameters.AddWithValue("@ProductID", productIdToDelete);
                                        cmdDeleteProductPurchases.ExecuteNonQuery();

                                        string deleteSalesQuery = "DELETE FROM sales WHERE product_id = @ProductID";
                                        MySqlCommand cmdDeleteSales = new MySqlCommand(deleteSalesQuery, conn, transaction);
                                        cmdDeleteSales.Parameters.AddWithValue("@ProductID", productIdToDelete);
                                        cmdDeleteSales.ExecuteNonQuery();

                                        string deleteProductQuery = "DELETE FROM products WHERE product_id = @ProductID";
                                        MySqlCommand cmdDeleteProduct = new MySqlCommand(deleteProductQuery, conn, transaction);
                                        cmdDeleteProduct.Parameters.AddWithValue("@ProductID", productIdToDelete);
                                        int rowsAffectedOnProduct = cmdDeleteProduct.ExecuteNonQuery();

                                        if (rowsAffectedOnProduct > 0)
                                        {
                                            transaction.Commit();
                                            MessageBox.Show($"Product '{productName}' (Product ID: {productIdToDelete}) and all its related records deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            MessageBox.Show($"Product '{productName}' (Product ID: {productIdToDelete}) not found or could not be deleted. It might have been removed by another user or had no entry in the 'products' table.", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    catch (MySqlException ex)
                                    {
                                        transaction.Rollback();
                                        if (ex.Number == 1451)
                                        {
                                            MessageBox.Show($"Cannot delete product ID {productIdToDelete}. It is still referenced by other records not covered by the manual cascade delete. Details: {ex.Message}", "Deletion Blocked by Constraint", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Database error during deletion: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        MessageBox.Show("An unexpected error occurred during the deletion process: " + ex.Message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    finally
                                    {
                                        LoadMedicinesData();
                                        SetupStats();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred (e.g., connecting to the database): " + ex.Message, "Operation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Could not identify the item to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var Login = new Login();
            Login.Show();
            this.Close();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard();
            adminDashboard.Show();
            this.Close();
        }
    }
}