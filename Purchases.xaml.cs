using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class Purchases : Window
    {
        public class Purchase
        {
            public string PurchaseId { get; set; }
            public string Supplier { get; set; }
            public DateTime Date { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; } // e.g., Completed, Pending, Cancelled
        }

        public Purchases()
        {
            InitializeComponent();
            LoadSampleData();
            UpdateStats();
        }

        private void LoadSampleData()
        {
            var purchases = new List<Purchase>
            {
                new Purchase { PurchaseId = "PUR-001", Supplier = "MediCorp",
                    Date = DateTime.Now.AddDays(-5), TotalAmount = 24500.00m, Status = "Completed" },
                new Purchase { PurchaseId = "PUR-002", Supplier = "PharmaPlus",
                    Date = DateTime.Now.AddDays(-2), TotalAmount = 17800.50m, Status = "Pending" },
                new Purchase { PurchaseId = "PUR-003", Supplier = "Global Health",
                    Date = DateTime.Now.AddMonths(-1), TotalAmount = 53400.75m, Status = "Completed" }
            };

            PurchasesDataGrid.ItemsSource = purchases;
        }

        private void UpdateStats()
        {
            TotalPurchases.Text = "143";
            MonthlyTotal.Text = "$45,800";
            PendingOrders.Text = "23";
            TotalSuppliers.Text = "15";
        }

        // Navigation Handlers (Same as AdminDashboard)
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            new Dashboard().Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            new Reports().Show();
            this.Close();
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            new Partners().Show();
            this.Close();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            new OrderDetails().Show();
            this.Close();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
        {
            new Distributors().Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        // Purchase-specific Handlers
        private void AddPurchase_Click(object sender, RoutedEventArgs e)
        {
            var addPurchaseWindow = new AddPurchaseWindow();
            addPurchaseWindow.ShowDialog();
        }

        private void EditPurchase_Click(object sender, RoutedEventArgs e)
        {
            var addPurchaseWindow = new PurchaseDetails();
            addPurchaseWindow.ShowDialog();
        }

        private void DeletePurchase_Click(object sender, RoutedEventArgs e)
        {
            if (PurchasesDataGrid.SelectedItem is Purchase purchase)
            {
                MessageBox.Show($"Deleting purchase: {purchase.PurchaseId}");
            }
        }

        // Existing handlers from AdminDashboard
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Purchases...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search Purchases...";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void Purchases_Click(object sender, RoutedEventArgs e) { }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var addPurchaseWindow = new UserProfile();
            addPurchaseWindow.ShowDialog();
        }
    }
}