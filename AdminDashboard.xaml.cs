using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class AdminDashboard : Window
    {
        public class AdminUser
        {
            public string UserId { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
            public DateTime LastLogin { get; set; }
            public bool IsActive { get; set; }
            public bool IsCritical { get; set; }
        }

        public AdminDashboard()
        {
            InitializeComponent();
            LoadSampleData();
            UpdateStats();
        }

        private void LoadSampleData()
        {
            var users = new List<AdminUser>
            {
                new AdminUser { UserId = "ADM001", Username = "admin", Role = "Administrator",
                               LastLogin = DateTime.Now.AddHours(-1), IsActive = true },
                new AdminUser { UserId = "MGR002", Username = "manager1", Role = "Manager",
                               LastLogin = DateTime.Now.AddDays(-1), IsActive = true },
                new AdminUser { UserId = "USR003", Username = "user01", Role = "Staff",
                               LastLogin = DateTime.Now.AddDays(-5), IsActive = false, IsCritical = true }
            };

            AdminDataGrid.ItemsSource = users;
        }

        private void UpdateStats()
        {
            TotalUsers.Text = "45";
            ActiveSessions.Text = "12";
            TodaysLogs.Text = "237";
            SystemHealth.Text = "98%";
        }

        // Navigation Handlers
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            new Dashboard().Show();
            this.Close();
        }

        private void UserManagement_Click(object sender, RoutedEventArgs e)
        {
            AdminDataGrid.Visibility = Visibility.Visible;
            // Add specific user management logic
        }

        private void AuditLogs_Click(object sender, RoutedEventArgs e)
        {
            // Implement audit logs view
        }

        private void SystemSettings_Click(object sender, RoutedEventArgs e)
        {
            // Open system settings dialog
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        // Admin Actions
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            // Implement add user logic
            MessageBox.Show("Add User functionality");
        }

        private void SystemConfig_Click(object sender, RoutedEventArgs e)
        {
            // Implement system configuration
            MessageBox.Show("System Configuration");
        }

        private void AdminSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Implement search functionality
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var editUserWindow = new EditUserWindow();
            editUserWindow.ShowDialog();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (AdminDataGrid.SelectedItem is AdminUser user)
            {
                MessageBox.Show($"Resetting password for: {user.Username}");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search Users...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Implement later
        }

        // Add these empty handlers to prevent errors
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void AddOrder_Click(object sender, RoutedEventArgs e) 
        {
            var addOrderWindow = new NewUserWindow();
            addOrderWindow.ShowDialog();
        }
        private void Profile_Click(object sender, RoutedEventArgs e) 
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Show();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            Reports reports = new Reports();
            reports.Show();
            this.Close();
        }

        private void Partners_Click(object sender, RoutedEventArgs e)
        {
            Partners partners = new Partners();
            partners.Show();
            this.Close();

        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            OrderDetails orderDetails = new OrderDetails();
            orderDetails.Show();
            this.Close();
        }

        private void ManageStore_Click(object sender, RoutedEventArgs e)
        {
            Distributors distributors = new Distributors();
            distributors.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Purchases_Click(object sender, RoutedEventArgs e)
        {
            var page = new Purchases();
            page.Show();
            this.Close();
        }
    }
}