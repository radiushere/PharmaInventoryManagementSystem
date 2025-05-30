using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class AdminDashboard : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private List<AdminUser> users = new List<AdminUser>();

        public class AdminUser
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Fname { get; set; }
            public string Mname { get; set; }
            public string Lname { get; set; }
            public string Role { get; set; }
            public string AccountStatus { get; set; }
            public DateTime CreatedAt { get; set; }

            public string FullName => $"{Fname} {Lname}".Trim();
        }

        public AdminDashboard()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT id, username, email, fname, mname, lname, account_status, created_at, role FROM users";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            users.Clear();
                            while (reader.Read())
                            {
                                users.Add(new AdminUser
                                {
                                    Id = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Fname = reader.IsDBNull(reader.GetOrdinal("fname")) ? string.Empty : reader.GetString("fname"),
                                    Mname = reader.IsDBNull(reader.GetOrdinal("mname")) ? string.Empty : reader.GetString("mname"),
                                    Lname = reader.IsDBNull(reader.GetOrdinal("lname")) ? string.Empty : reader.GetString("lname"),
                                    Role = reader.GetString("role"),
                                    AccountStatus = reader.IsDBNull(reader.GetOrdinal("account_status")) ? string.Empty : reader.GetString("account_status"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }
                AdminDataGrid.ItemsSource = users;
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStats()
        {
            TotalUsers.Text = users.Count.ToString();
            AdminCount.Text = users.Count(u => u.Role == "Admin").ToString();
            ManagerCount.Text = users.Count(u => u.Role == "Manager").ToString();
            DistributorCount.Text = users.Count(u => u.Role == "Distributor").ToString();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            var filteredUsers = users.Where(u =>
                u.Username.ToLower().Contains(searchText) ||
                u.Email.ToLower().Contains(searchText) ||
                u.FullName.ToLower().Contains(searchText)
            ).ToList();
            AdminDataGrid.ItemsSource = filteredUsers;
            UpdateStats();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)FilterComboBox.SelectedItem;
            string filter = selectedItem?.Content.ToString();

            if (filter == "All Users")
            {
                AdminDataGrid.ItemsSource = users;
            }
            else
            {
                AdminDataGrid.ItemsSource = users.Where(u => u.Role == filter).ToList();
            }
            UpdateStats();
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button.DataContext as AdminUser;
            if (user != null)
            {
                // Open Edit User Window with user details
                EditUserWindow editUserWindow = new EditUserWindow(user.Id);
                editUserWindow.ShowDialog();
                LoadData(); // Refresh the data after editing
            }
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button.DataContext as AdminUser;
            if (user != null)
            {
                // Confirm deletion with the user
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete user: {user.Username}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            var query = "DELETE FROM users WHERE id = @id";
                            using (var cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", user.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadData(); // Refresh the data after deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
                SearchBox.Text = "Search Users...";
        }

        // Navigation Handlers
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            new Dashboard().Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            // Implement add user logic
            MessageBox.Show("Add User functionality");
        }


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
            Orders orderDetails = new Orders();
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

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            var adminPage = new AdminDashboard();
            adminPage.Show();
            this.Close();
        }
    }
}
