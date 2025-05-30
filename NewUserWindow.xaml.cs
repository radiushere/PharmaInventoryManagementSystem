using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class NewUserWindow : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public NewUserWindow()
        {
            InitializeComponent();
            // Set default account status to "Active"
            cmbRole.SelectedIndex = 0; // Select the first role by default
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) ||
                string.IsNullOrEmpty(txtFullName.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) ||
                string.IsNullOrEmpty(txtPassword.Text) ||
                string.IsNullOrEmpty(txtConfirmPassword.Text) ||
                cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var fullName = txtFullName.Text.Split(' ');
                    var query = @"INSERT INTO users (username, email, fname, lname, password, account_status, role, created_at)
                                  VALUES (@username, @email, @fname, @lname, @password, @accountStatus, @role, @createdAt)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@fname", fullName[0]);
                        cmd.Parameters.AddWithValue("@lname", fullName.Length > 1 ? fullName[1] : string.Empty);
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@accountStatus", "Active"); // Default account status
                        cmd.Parameters.AddWithValue("@role", ((ComboBoxItem)cmbRole.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now); // Set created_at to current date and time

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}