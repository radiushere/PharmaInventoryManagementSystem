using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class EditUserWindow : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private int userId;

        public EditUserWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            InitializePasswordControls();
            LoadUserData(userId);
        }

        private void InitializePasswordControls()
        {
            chkChangePassword.Checked += (s, e) =>
            {
                txtPassword.Visibility = Visibility.Visible;
                txtConfirmPassword.Visibility = Visibility.Visible;
            };

            chkChangePassword.Unchecked += (s, e) =>
            {
                txtPassword.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
            };
        }

        private void LoadUserData(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT id, username, email, fname, lname, account_status, role FROM users WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtUsername.Text = reader.GetString("username");
                                txtFullName.Text = $"{reader.GetString("fname")} {reader.GetString("lname")}".Trim();
                                txtEmail.Text = reader.GetString("email");

                                // Set the selected item in the role ComboBox
                                foreach (var item in cmbRole.Items)
                                {
                                    if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == reader.GetString("role"))
                                    {
                                        cmbRole.SelectedItem = comboBoxItem;
                                        break;
                                    }
                                }

                                // Set the selected item in the status ComboBox
                                foreach (var item in cmbStatus.Items)
                                {
                                    if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == reader.GetString("account_status"))
                                    {
                                        cmbStatus.SelectedItem = comboBoxItem;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"UPDATE users
                                  SET email = @email,
                                      fname = @fname,
                                      lname = @lname,
                                      account_status = @accountStatus,
                                      role = @role";

                    if (chkChangePassword.IsChecked == true)
                    {
                        query += ", password = @password";
                    }

                    query += " WHERE id = @id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);

                        var fullName = txtFullName.Text.Split(' ');
                        cmd.Parameters.AddWithValue("@fname", fullName[0]);
                        cmd.Parameters.AddWithValue("@lname", fullName.Length > 1 ? fullName[1] : string.Empty);

                        cmd.Parameters.AddWithValue("@accountStatus", ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@role", ((ComboBoxItem)cmbRole.SelectedItem).Content.ToString());

                        if (chkChangePassword.IsChecked == true)
                        {
                            cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
