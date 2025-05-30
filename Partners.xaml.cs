using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class Partners : Window
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private DataTable partnersDataTable = new DataTable();

        public Partners()
        {
            InitializeComponent();
            AccessLevelControl();
            LoadPartnersData();
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

        public void LoadPartnersData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"
                       SELECT
                            p.partner_id AS PartnerID,
                            p.partner_name AS PartnerName,
                            p.partner_type AS PartnerType,
                            p.contact_person AS ContactPerson,

                            (
                                SELECT pp.phone_number
                                FROM partner_phones pp
                                WHERE pp.partner_id = p.partner_id
                                LIMIT 1
                            ) AS PhoneNumber,
                            p.contract_start AS ContractStart,
                            p.contract_end AS ContractEnd,
                            p.credit_limit AS CreditLimit
                        FROM partners p;";

                    var adapter = new MySqlDataAdapter(query, conn);
                    partnersDataTable.Clear();
                    adapter.Fill(partnersDataTable);
                    PartnersDataGrid.ItemsSource = partnersDataTable.DefaultView;

                    Console.WriteLine($"Number of rows loaded: {partnersDataTable.Rows.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetupStats()
        {
            TotalPartnersCount.Text = partnersDataTable.Rows.Count.ToString();

            int activeContracts = 0;
            var typeCounts = new Dictionary<string, int>();

            foreach (DataRow row in partnersDataTable.Rows)
            {
                // Active contracts
                if (!row.IsNull("ContractEnd") && (DateTime)row["ContractEnd"] > DateTime.Now)
                    activeContracts++;

                // Partner type counts
                var type = row["PartnerType"].ToString();
                if (typeCounts.ContainsKey(type))
                    typeCounts[type]++;
                else
                    typeCounts[type] = 1;
            }

            ActiveContractsCount.Text = activeContracts.ToString();
            TopPartnerType.Text = typeCounts.Count > 0
                ? $"{typeCounts.MaxBy(kvp => kvp.Value).Key} ({typeCounts.MaxBy(kvp => kvp.Value).Value})"
                : "N/A";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            var filter = $"PartnerName LIKE '%{searchText}%' OR ContactPerson LIKE '%{searchText}%' " +
                        $"OR PhoneNumber LIKE '%{searchText}%' OR PartnerType LIKE '%{searchText}%'";
            partnersDataTable.DefaultView.RowFilter = filter;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedIndex == 0)
                partnersDataTable.DefaultView.RowFilter = "";
            else
            {
                var selectedType = ((ComboBoxItem)FilterComboBox.SelectedItem).Content.ToString();
                partnersDataTable.DefaultView.RowFilter = $"PartnerType = '{selectedType}'";
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Partners...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search Partners...";
        }

        private void AddPartner_Click(object sender, RoutedEventArgs e)
        {
            var newPartnerWindow = new NewPartnerWindow(this);
            newPartnerWindow.Owner = this;
            newPartnerWindow.ShowDialog();
        }

        // Navigation Methods (similar to previous pages)
        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            Orders orders = new Orders();
            orders.Show();
            this.Close();
        }
        private void Partners_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show("Already on partners page");
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        // Other standard methods
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) =>
            Application.Current.MainWindow?.Show();

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Close();
        }

        private void Distributor_Click(object sender, RoutedEventArgs e)
        {
            Distributors distributors = new Distributors();
            distributors.Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Reports();
            page.Show();
            this.Close();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var page = new UserProfile();
            page.ShowDialog();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected partner from the DataGrid
            var selectedPartner = (DataRowView)PartnersDataGrid.SelectedItem;
            if (selectedPartner == null)
            {
                MessageBox.Show("Please select a partner to view details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get the partner ID
            int partnerId = (int)selectedPartner["PartnerID"];

            // Open the PartnerDetails window with the selected partner ID
            var detailsWindow = new PartnerDetails(partnerId);
            detailsWindow.ShowDialog();
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected partner from the DataGrid
            var selectedPartner = (DataRowView)PartnersDataGrid.SelectedItem;
            if (selectedPartner == null)
            {
                MessageBox.Show("Please select a partner to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Confirm deletion
            var result = MessageBox.Show("Are you sure you want to delete this partner?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            // Get the partner ID
            int partnerId = (int)selectedPartner["PartnerID"];

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Start a transaction to ensure both deletions succeed or fail together
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Delete from partner_phones table
                            var deletePhonesQuery = "DELETE FROM partner_phones WHERE partner_id = @PartnerId";
                            using (var cmd = new MySqlCommand(deletePhonesQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PartnerId", partnerId);
                                cmd.ExecuteNonQuery();
                            }

                            // Delete from partners table
                            var deletePartnerQuery = "DELETE FROM partners WHERE partner_id = @PartnerId";
                            using (var cmd = new MySqlCommand(deletePartnerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PartnerId", partnerId);
                                cmd.ExecuteNonQuery();
                            }

                            // Commit the transaction
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if an error occurs
                            transaction.Rollback();
                            MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // Refresh the DataGrid to reflect the deletion
                LoadPartnersData();
                MessageBox.Show("Partner deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var pages = new Purchases();
            pages.Show();
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
