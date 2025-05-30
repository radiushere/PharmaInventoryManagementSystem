using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace SimpleLoginWPF
{
    public partial class NewDistributorWindow : Window
    {
        private readonly Distributors _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public NewDistributorWindow(Distributors parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            cmbDistributionType.SelectedIndex = 0;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRegion.Text))
            {
                MessageBox.Show("Region is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbDistributionType.SelectedItem == null)
            {
                MessageBox.Show("Distribution Type is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Phone number is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Valid email address is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtServiceArea.Text))
            {
                MessageBox.Show("Service Area is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Insert into distributors table
                    var distributorQuery = @"INSERT INTO distributors
                                            (name, region, distributor_type, email, service_area)
                                            VALUES
                                            (@name, @region, @distributorType, @email, @serviceArea);
                                            SELECT LAST_INSERT_ID();";

                    int distributorId;
                    using (var cmd = new MySqlCommand(distributorQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@region", txtRegion.Text.Trim());
                        cmd.Parameters.AddWithValue("@distributorType", ((ComboBoxItem)cmbDistributionType.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@serviceArea", txtServiceArea.Text.Trim());

                        distributorId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Insert phone numbers into distributor_phones table
                    var phoneNumbers = txtPhone.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var phoneNumber in phoneNumbers)
                    {
                        var phoneQuery = @"INSERT INTO distributor_phones
                                           (dist_id, phone)
                                           VALUES
                                           (@distId, @phone)";

                        using (var phoneCmd = new MySqlCommand(phoneQuery, conn))
                        {
                            phoneCmd.Parameters.AddWithValue("@distId", distributorId);
                            phoneCmd.Parameters.AddWithValue("@phone", phoneNumber.Trim());
                            phoneCmd.ExecuteNonQuery();
                        }
                    }
                }

                _parentWindow.LoadDistributorsData();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving distributor: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
