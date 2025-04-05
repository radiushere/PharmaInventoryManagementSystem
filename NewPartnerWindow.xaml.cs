using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace SimpleLoginWPF
{
    public partial class NewPartnerWindow : Window
    {
        private readonly Partners _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public NewPartnerWindow(Partners parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            LoadPartnerTypes();
            dpContractStart.SelectedDate = DateTime.Today;
        }

        private void LoadPartnerTypes()
        {
            cmbType.Items.Add("Hospital");
            cmbType.Items.Add("Clinic");
            cmbType.Items.Add("Pharmacy");
            cmbType.Items.Add("Research Center");
            cmbType.SelectedIndex = 0;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Partner name is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtContactPerson.Text))
            {
                MessageBox.Show("Contact person is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Phone number is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpContractStart.SelectedDate == null)
            {
                MessageBox.Show("Contract start date is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(txtCreditLimit.Text, out _))
            {
                MessageBox.Show("Invalid credit limit format", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"INSERT INTO partners 
                                (name, partner_type, contact_person, phone, 
                                 contract_start, contract_end, credit_limit)
                                VALUES
                                (@name, @type, @contact, @phone, 
                                 @start, @end, @credit)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cmbType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@contact", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@start", dpContractStart.SelectedDate);
                        cmd.Parameters.AddWithValue("@end", dpContractEnd.SelectedDate);
                        cmd.Parameters.AddWithValue("@credit", decimal.Parse(txtCreditLimit.Text));

                        cmd.ExecuteNonQuery();
                    }
                }

                _parentWindow.LoadPartnersData();
                _parentWindow.SetupStats();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving partner: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}