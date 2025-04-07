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
            LoadPartnersData();
            SetupStats();
            SetupConverters();
        }

        private void SetupConverters()
        {
            //if (!Resources.Contains("StringToVisibilityConverter"))
            //    Resources.Add("StringToVisibilityConverter", new StringToVisibilityConverter());
        }

        public void LoadPartnersData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"SELECT 
                                partner_id AS PartnerID,
                                name AS Name,
                                partner_type AS PartnerType,
                                contact_person AS ContactPerson,
                                phone AS Phone,
                                contract_start AS ContractStart,
                                contract_end AS ContractEnd,
                                credit_limit AS CreditLimit
                                FROM partners";

                    var adapter = new MySqlDataAdapter(query, conn);
                    partnersDataTable.Clear();
                    adapter.Fill(partnersDataTable);
                    PartnersDataGrid.ItemsSource = partnersDataTable.DefaultView;
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
            var filter = $"Name LIKE '%{searchText}%' OR ContactPerson LIKE '%{searchText}%' " +
                        $"OR Phone LIKE '%{searchText}%' OR PartnerType LIKE '%{searchText}%'";
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
            this.Hide();
        }

        private void Distributors_Click(object sender, RoutedEventArgs e)
        {
            Distributors distributors = new Distributors();
            distributors.Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Reports();
            page.Show();
            this.Hide();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var page = new UserProfile();
            page.ShowDialog();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var page = new PartnerDetails(1);
            page.ShowDialog();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var pages = new Purchases();
            pages.Show();
            this.Close();
        }
    }
}