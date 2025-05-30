using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Media;

namespace SimpleLoginWPF
{
    public partial class PartnerDetails : Window
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private int currentPartnerId;

        public PartnerDetails(int partnerId)
        {
            InitializeComponent();
            currentPartnerId = partnerId;
            LoadPartnerDetails(partnerId);
        }

        private void LoadPartnerDetails(int partnerId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Load partner basic details
                    string partnerQuery = @"
                        SELECT partner_name, partner_type, contact_person, contract_start, contract_end, credit_limit 
                        FROM partners 
                        WHERE partner_id = @id";

                    using (MySqlCommand partnerCmd = new MySqlCommand(partnerQuery, conn))
                    {
                        partnerCmd.Parameters.AddWithValue("@id", partnerId);

                        using (MySqlDataReader reader = partnerCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PartnerName.Text = reader["partner_name"].ToString();
                                PartnerType.Text = reader["partner_type"].ToString();
                                ContactPerson.Text = reader["contact_person"].ToString();

                                if (DateTime.TryParse(reader["contract_start"].ToString(), out DateTime contractStart))
                                    ContractStart.Text = contractStart.ToString("MMMM dd, yyyy");
                                else
                                    ContractStart.Text = "N/A";

                                if (DateTime.TryParse(reader["contract_end"].ToString(), out DateTime contractEnd))
                                    ContractEnd.Text = contractEnd.ToString("MMMM dd, yyyy");
                                else
                                    ContractEnd.Text = "N/A";

                                DateTime contractEndDate = Convert.ToDateTime(reader["contract_end"]);
                                ContractEnd.Text = contractEndDate.ToString("MMMM dd, yyyy");

                                // Determine status
                                if (contractEndDate >= DateTime.Today)
                                {
                                    PartnerStatus.Text = "Active";
                                    PartnerStatus.Foreground = new SolidColorBrush(Colors.Green);
                                }
                                else
                                {
                                    PartnerStatus.Text = "Not Active";
                                    PartnerStatus.Foreground = new SolidColorBrush(Colors.Red);
                                }


                                if (decimal.TryParse(reader["credit_limit"].ToString(), out decimal creditLimit))
                                    CreditLimit.Text = creditLimit.ToString("C");
                                else
                                    CreditLimit.Text = "N/A";

                                PartnerId.Text = partnerId.ToString();

                                // Generate initials from name
                                string[] nameParts = reader["partner_name"].ToString().Split(' ');
                                PartnerInitials.Text = nameParts.Length >= 2
                                    ? $"{nameParts[0][0]}{nameParts[1][0]}".ToUpper()
                                    : nameParts[0][0].ToString().ToUpper();
                            }
                            else
                            {
                                MessageBox.Show("Partner not found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }

                    // Load partner phone number
                    string phoneQuery = "SELECT phone_number FROM partner_phones WHERE partner_id = @id LIMIT 1";
                    using (MySqlCommand phoneCmd = new MySqlCommand(phoneQuery, conn))
                    {
                        phoneCmd.Parameters.AddWithValue("@id", partnerId);

                        object phoneResult = phoneCmd.ExecuteScalar();
                        ContactPhone.Text = phoneResult != null ? phoneResult.ToString() : "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading partner details:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditPartner_Click(object sender, RoutedEventArgs e)
        {
            var editPopup = new EditPartnerPopup(currentPartnerId);
            editPopup.Owner = this;
            if (editPopup.ShowDialog() == true)
            {
                LoadPartnerDetails(currentPartnerId); // Refresh after edit
            }
        }

        private void Exit_Buttton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}