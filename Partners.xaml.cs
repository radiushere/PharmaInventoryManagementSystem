using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            Loaded += Partners_Loaded;
        }

        private async void Partners_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPartnersDataAsync();
            SetupStats();
        }

        public void AccessLevelControl()
        {
            string role = UserSession.Role;

            DashboardButton.Visibility = Visibility.Visible;
            InventoryButton.Visibility = Visibility.Visible;
            ReportsButton.Visibility = Visibility.Visible;
            OrdersButton.Visibility = Visibility.Visible;
            DistributorsButton.Visibility = Visibility.Visible;
            PurchasesButton.Visibility = Visibility.Visible;

            AdminButton.Visibility = role == "Admin" ? Visibility.Visible : Visibility.Collapsed;
            PartnersButton.Visibility = (role == "Admin" || role == "Manager") ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrEmpty(role))
                MessageBox.Show("Unknown role.");
        }

        public async Task LoadPartnersDataAsync()
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                string query = @"
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
                DataTable newTable = new DataTable();
                await Task.Run(() => adapter.Fill(newTable)); // Fill the data table on a background thread
                partnersDataTable = newTable;
                PartnersDataGrid.ItemsSource = partnersDataTable.DefaultView;

                // SetupStats will now be called after this async method completes
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading partners: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetupStats()
        {
            TotalPartnersCount.Text = partnersDataTable.Rows.Count.ToString();

            int activeContracts = 0;
            var typeCounts = new Dictionary<string, int>();

            foreach (DataRow row in partnersDataTable.Rows)
            {
                if (!row.IsNull("ContractEnd") &&
                    DateTime.TryParse(row["ContractEnd"].ToString(), out DateTime contractEnd) &&
                    contractEnd > DateTime.Now)
                {
                    activeContracts++;
                }

                string type = row["PartnerType"]?.ToString() ?? "Unknown";
                if (typeCounts.ContainsKey(type))
                    typeCounts[type]++;
                else
                    typeCounts[type] = 1;
            }

            ActiveContractsCount.Text = activeContracts.ToString();

            if (typeCounts.Count > 0)
            {
                var topType = typeCounts.OrderByDescending(k => k.Value).First();
                TopPartnerType.Text = $"{topType.Key} ({topType.Value})";
            }
            else
            {
                TopPartnerType.Text = "N/A";
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(searchText) || searchText == "Search Partners...")
            {
                partnersDataTable.DefaultView.RowFilter = "";
                return;
            }

            partnersDataTable.DefaultView.RowFilter =
                $"PartnerName LIKE '%{searchText}%' OR ContactPerson LIKE '%{searchText}%' " +
                $"OR PhoneNumber LIKE '%{searchText}%' OR PartnerType LIKE '%{searchText}%'";
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedIndex <= 0)
            {
                partnersDataTable.DefaultView.RowFilter = "";
                return;
            }

            string selectedType = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            partnersDataTable.DefaultView.RowFilter = $"PartnerType = '{selectedType.Replace("'", "''")}'";
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

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersDataGrid.SelectedItem is not DataRowView selectedPartner)
            {
                MessageBox.Show("Please select a partner to view details.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedPartner["PartnerID"] == DBNull.Value || !int.TryParse(selectedPartner["PartnerID"].ToString(), out int partnerId))
            {
                MessageBox.Show("Invalid Partner ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var detailsWindow = new PartnerDetails(partnerId);
            detailsWindow.ShowDialog();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersDataGrid.SelectedItem is not DataRowView selectedPartner)
            {
                MessageBox.Show("Please select a partner to delete.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(selectedPartner["PartnerID"]?.ToString(), out int partnerId))
            {
                MessageBox.Show("Invalid Partner ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this partner?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();
                using var transaction = await conn.BeginTransactionAsync();

                try
                {
                    string deletePhonesQuery = "DELETE FROM partner_phones WHERE partner_id = @PartnerId";
                    using (var cmd = new MySqlCommand(deletePhonesQuery, conn, (MySqlTransaction)transaction))
                    {
                        cmd.Parameters.AddWithValue("@PartnerId", partnerId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    string deletePartnerQuery = "DELETE FROM partners WHERE partner_id = @PartnerId";
                    using (var cmd = new MySqlCommand(deletePartnerQuery, conn, (MySqlTransaction)transaction))
                    {
                        cmd.Parameters.AddWithValue("@PartnerId", partnerId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                    MessageBox.Show("Partner deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPartnersDataAsync(); // Refresh
                    SetupStats(); // Also refresh stats after deletion and reload
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show($"Deletion failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Navigation
        private void Inventory_Click(object sender, RoutedEventArgs e) => NavigateTo(new MainWindow());
        private void Orders_Click(object sender, RoutedEventArgs e) => NavigateTo(new Orders());
        private void Partners_Click(object sender, RoutedEventArgs e) => NavigateTo(new Partners());
        private void Purchases_Click(object sender, RoutedEventArgs e) => NavigateTo(new Purchases());
        private void Reports_Click(object sender, RoutedEventArgs e) => NavigateTo(new Reports());
        private void Dashboard_Click(object sender, RoutedEventArgs e) => NavigateTo(new Dashboard());
        private void Distributor_Click(object sender, RoutedEventArgs e) => NavigateTo(new Distributors());
        private void AdminDashboard_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDashboard());

        private void NavigateTo(Window window)
        {
            window.Show();
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            Close();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            new UserProfile().ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Application.Current.MainWindow?.Show();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPartnersDataAsync();
            SetupStats();
        }
    }
}