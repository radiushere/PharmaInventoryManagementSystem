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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public MainWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            InitializeComponent();
            LoadMedicinesData();
            SetupAutoRefresh();
            SetupStats();
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
                            id AS ID, 
                            name AS Name, 
                            quantity AS Quantity, 
                            price AS Price, 
                            expiry_date AS ExpiryDate, 
                            batch_number AS BatchNumber, 
                            image_path AS ImagePath, 
                            description AS Description 
                        FROM medicines";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    medicineDataTable.Clear();
                    adapter.Fill(medicineDataTable);
                    MedicinesDataGrid.ItemsSource = medicineDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void SetupAutoRefresh()
        {
            refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            refreshTimer.Tick += (s, e) => LoadMedicinesData();
            refreshTimer.Start();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (medicineDataTable == null || medicineDataTable.Columns.Count == 0)
            {
                Console.WriteLine("medicineDataTable is null or has no columns.");
                return;
            }

            // Debugging: Print column names
            Console.WriteLine("Available Columns:");
            foreach (DataColumn col in medicineDataTable.Columns)
            {
                Console.WriteLine(col.ColumnName);
            }

            string filter = SearchBox.Text.Trim();

            try
            {
                // Use the correct column names from the database
                medicineDataTable.DefaultView.RowFilter =
                    $"name LIKE '%{filter}%' OR BatchNumber LIKE '%{filter}%'";
            }
            catch (EvaluateException ex)
            {
                Console.WriteLine("Error in RowFilter: " + ex.Message);
                MessageBox.Show("Error in search filter: " + ex.Message);
            }
        }
        public void SetupStats()
        {
            ProductCount.Text = medicineDataTable.Rows.Count.ToString();

            int availableCount = 0;
            int lowstockCount= 0;

            foreach (DataRow row in medicineDataTable.Rows)
            {
                var quantity = (int)row["Quantity"];
                if (quantity > 0) availableCount++;
                if (quantity < 5) lowstockCount++;
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

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var page = new Dashboard();
            page.Show();
            this.Hide();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var page = new MainWindow();
            page.Show();
            this.Hide();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Reports();
            page.Show();
            this.Hide();
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            //Suppliers suppliers = new Suppliers();
            //suppliers.Show();
            //this.Hide();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            Orders orders = new Orders();
            orders.Show();
            this.Hide();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var newPartnerWindow = new NewProductWindow(this);
            newPartnerWindow.Owner = this;
            newPartnerWindow.ShowDialog();
        }

        private void SearchBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

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
            this.Hide();
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            Partners partners = new Partners();
            partners.Show();
            this.Hide();
        }

        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var page = new Purchases();
            page.Show();
            this.Hide();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            var page = new ProductInfo(1);
            page.Show();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
