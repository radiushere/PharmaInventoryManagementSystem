using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            cmbType.ItemsSource = new[] { "Hospital", "Clinic", "Pharmacy", "Research Center" };
            cmbType.SelectedIndex = 0;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Partner name is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtContactPerson.Text))
            {
                MessageBox.Show("Contact person is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhones.Text))
            {
                MessageBox.Show("At least one phone number is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpContractStart.SelectedDate == null)
            {
                MessageBox.Show("Contract start date is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (dpContractEnd.SelectedDate == null)
            {
                MessageBox.Show("Contract end date is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(txtCreditLimit.Text, out _))
            {
                MessageBox.Show("Invalid credit limit format", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                    // Insert into partners table
                    var partnerQuery = @"INSERT INTO partners 
                                        (partner_name, partner_type, contact_person, contract_start, contract_end, credit_limit)
                                        VALUES (@name, @type, @contact, @start, @end, @credit);
                                        SELECT LAST_INSERT_ID();";

                    long partnerId;
                    using (var cmd = new MySqlCommand(partnerQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cmbType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@contact", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@start", dpContractStart.SelectedDate);
                        cmd.Parameters.AddWithValue("@end", dpContractEnd.SelectedDate);
                        cmd.Parameters.AddWithValue("@credit", decimal.Parse(txtCreditLimit.Text));
                        partnerId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    // Insert phone numbers into partner_contacts table
                    string[] phones = txtPhones.Text.Split(',').Select(p => p.Trim()).Where(p => p != "").ToArray();
                    var phoneQuery = @"INSERT INTO partner_phones (partner_id, phone_number) VALUES (@partnerId, @phone)";
                    foreach (var phone in phones)
                    {
                        using (var cmd = new MySqlCommand(phoneQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@partnerId", partnerId);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                _parentWindow.LoadPartnersData();
                _parentWindow.SetupStats();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving partner: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
