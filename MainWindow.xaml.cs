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
        private DataTable medicineDataTable = new DataTable(); // Ensuring it's initialized

        public MainWindow()
        {
            InitializeComponent();
            LoadMedicinesData(); // Initial data load
            SetupAutoRefresh(); // Start automatic refresh
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

                    // Ensure we clear and refill DataTable to prevent errors
                    medicineDataTable.Clear();
                    adapter.Fill(medicineDataTable);

                    // Ensure UI updates properly
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
            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // Refresh every 30 seconds
            };
            refreshTimer.Tick += (s, e) => LoadMedicinesData();
            refreshTimer.Start();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (medicineDataTable == null || medicineDataTable.DefaultView == null)
                return;

            string filter = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(filter))
            {
                medicineDataTable.DefaultView.RowFilter = ""; // Reset filter
                return;
            }

            // Ensure the column names match exactly!
            if (medicineDataTable.Columns.Contains("Name") && medicineDataTable.Columns.Contains("BatchNumber"))
            {
                medicineDataTable.DefaultView.RowFilter =
                    $"Name LIKE '%{filter}%' OR BatchNumber LIKE '%{filter}%'";
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Medicine...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search Medicine...";
                SearchBox.Foreground = Brushes.Gray;
            }
        }
    }
}
