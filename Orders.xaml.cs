// Orders.xaml.cs
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
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
            LoadOrdersData();
            SetupStats();
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
                                s.name AS SupplierName,
                                o.order_date AS OrderDate,
                                o.delivery_date AS DeliveryDate,
                                o.status AS Status,
                                o.total_amount AS TotalAmount
                              FROM orders o
                              JOIN suppliers s ON o.supplier_id = s.supplier_id";

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
                $"SupplierName LIKE '%{searchText}%' OR OrderID LIKE '%{searchText}%' OR Status LIKE '%{searchText}%'";
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

        // Navigation Methods (same as Suppliers)
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Hide();
        }
        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            //Suppliers suppliers = new Suppliers();
            //suppliers.Show();
            //this.Close();
        }
        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            var page = new Orders();
            page.Show();
            this.Hide();
        }
        
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var newPartnerWindow = new NewOrderWindow(this);
            newPartnerWindow.Owner = this;
            newPartnerWindow.ShowDialog();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
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

        // Other event handlers same as Suppliers
    }
}