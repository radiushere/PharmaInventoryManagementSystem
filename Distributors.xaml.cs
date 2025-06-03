using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class Distributors : Window
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private DataTable distributorsDataTable = new DataTable();

        public Distributors()
        {
            InitializeComponent();
            AccessLevelControl();
            LoadDistributorsData();
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

        public void LoadDistributorsData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"
                       SELECT
                            d.dist_id AS DistributorID,
                            d.name AS Name,
                            d.region AS Region,
                            d.distributor_type AS DistributorType,
                            d.email AS Email,
                            d.service_area AS ServiceArea,
                            (
                                SELECT dp.phone
                                FROM distributor_phones dp
                                WHERE dp.dist_id = d.dist_id
                                LIMIT 1
                            ) AS Phone
                        FROM
                            distributors d;";

                    var adapter = new MySqlDataAdapter(query, conn);

                    distributorsDataTable.Clear();
                    adapter.Fill(distributorsDataTable);
                    DistributorsDataGrid.ItemsSource = distributorsDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetupStats()
        {
            TotalDistributorsCount.Text = distributorsDataTable.Rows.Count.ToString();

            var regions = new HashSet<string>();
            int wholesaleCount = 0;

            foreach (DataRow row in distributorsDataTable.Rows)
            {
                regions.Add(row["Region"].ToString());
                if (row["DistributorType"].ToString() == "Institutional") wholesaleCount++;
            }

            WholesaleCount.Text = wholesaleCount.ToString();
            ActiveRegionsCount.Text = regions.Count.ToString();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            var filter = $"Name LIKE '%{searchText}%' OR Region LIKE '%{searchText}%' " +
                        $"OR Email LIKE '%{searchText}%' OR Phone LIKE '%{searchText}%'";
            distributorsDataTable.DefaultView.RowFilter = string.IsNullOrEmpty(searchText) ? "" : filter;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedType = (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedType == "All")
            {
                distributorsDataTable.DefaultView.RowFilter = "";
            }
            else
            {
                distributorsDataTable.DefaultView.RowFilter = $"DistributorType = '{selectedType}'";
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Distributors...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search Distributors...";
        }

        private void AddDistributor_Click(object sender, RoutedEventArgs e)
        {
            var newPartnerWindow = new NewDistributorWindow(this);
            newPartnerWindow.Owner = this;
            newPartnerWindow.ShowDialog();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var page = new UserProfile();
            page.Show();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var distributor = button.DataContext as DataRowView;
            var popupDetails = new DistributorDetails(Convert.ToInt32(distributor["DistributorID"]));
            popupDetails.Owner = this;
            popupDetails.ShowDialog();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var distributor = button.DataContext as DataRowView;

            if (MessageBox.Show("Are you sure you want to delete this distributor?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        var deleteQuery = "DELETE FROM distributors WHERE dist_id = @distId";
                        var command = new MySqlCommand(deleteQuery, conn);
                        command.Parameters.AddWithValue("@distId", distributor["DistributorID"]);
                        command.ExecuteNonQuery();
                    }
                    LoadDistributorsData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting distributor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            new MainWindow().Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Reports();
            page.Show();
            this.Close();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            new Orders().Show();
            this.Close();
        }

        private void Distributors_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show("Already on distributors page");

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) =>
            Application.Current.MainWindow?.Show();

        private void DistributorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implement selection logic if needed
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

        private void Distributor_Click(object sender, RoutedEventArgs e)
        {
            new Distributors().Show();
            this.Close();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            new AdminDashboard().Show();
            this.Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDistributorsData();
        }
    }
}
