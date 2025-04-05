using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
            LoadDistributorsData();
            SetupStats();
            SetupConverters();
        }

        private void SetupConverters()
        {
            //if (!Resources.Contains("StringToVisibilityConverter"))
            //{
            //    Resources.Add("StringToVisibilityConverter", new StringToVisibilityConverter());
            //}
        }

        public void LoadDistributorsData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"SELECT distributor_id AS DistributorID, name AS Name, 
                                region AS Region, distribution_type AS DistributionType,
                                phone AS Phone, email AS Email, service_area AS ServiceArea 
                                FROM distributors";

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
                if (row["DistributionType"].ToString() == "Wholesale") wholesaleCount++;
            }

            WholesaleCount.Text = wholesaleCount.ToString();
            ActiveRegionsCount.Text = regions.Count.ToString();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            var filter = $"Name LIKE '%{searchText}%' OR Region LIKE '%{searchText}%' " +
                        $"OR Email LIKE '%{searchText}%' OR Phone LIKE '%{searchText}%'";
            distributorsDataTable.DefaultView.RowFilter = filter;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedIndex == 0)
            {
                distributorsDataTable.DefaultView.RowFilter = "";
            }
            else
            {
                var filterType = ((ComboBoxItem)FilterComboBox.SelectedItem).Content.ToString();
                distributorsDataTable.DefaultView.RowFilter = $"DistributionType = '{filterType}'";
            }
        }

        // Navigation Methods
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Hide();
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
            this.Hide();
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            //new Suppliers().Show();
            //this.Close();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) =>
            Application.Current.MainWindow?.Show();

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
    }
}