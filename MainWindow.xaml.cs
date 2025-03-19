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
            LoadMedicinesData();
            SetupAutoRefresh();
        }

        private void LoadMedicinesData()
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

        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {

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

        }

        private void SearchBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
