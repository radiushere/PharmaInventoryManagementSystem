using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class EditPartnerPopup : Window
    {
        private int _partnerId;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private List<TextBox> phoneNumberTextBoxes = new List<TextBox>();

        public EditPartnerPopup(int partnerId)
        {
            InitializeComponent();
            _partnerId = partnerId;

            LoadPartnerData();
        }

        private void LoadPartnerData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT p.partner_id, p.partner_name, p.partner_type, p.contact_person, p.credit_limit, p.contract_start, p.contract_end " +
                                   "FROM partners p WHERE p.partner_id = @partnerId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@partnerId", _partnerId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PartnerIdInput.Text = reader["partner_id"].ToString();
                                PartnerNameInput.Text = reader["partner_name"].ToString();
                                PartnerTypeInput.SelectedItem = PartnerTypeInput.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == reader["partner_type"].ToString());
                                ContactPersonInput.Text = reader["contact_person"].ToString();
                                CreditLimitInput.Text = reader["credit_limit"].ToString();
                                ContractStartInput.SelectedDate = Convert.ToDateTime(reader["contract_start"]);
                                ContractEndInput.SelectedDate = Convert.ToDateTime(reader["contract_end"]);
                            }
                        }
                    }

                    // Load phone numbers
                    query = "SELECT phone_number FROM partner_phones WHERE partner_id = @partnerId";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@partnerId", _partnerId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AddPhoneNumberTextBox(reader["phone_number"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading partner data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddPhoneNumberTextBox(string phoneNumber = "")
        {
            StackPanel phonePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            TextBox phoneTextBox = new TextBox
            {
                Text = phoneNumber,
                Style = FindResource("InputField") as Style,
                Height = 30,
                Width = 200,
                Margin = new Thickness(0, 0, 10, 0)
            };
            phoneNumberTextBoxes.Add(phoneTextBox);

            Button removeButton = new Button
            {
                Content = "Remove",
                Style = FindResource("SecondaryButton") as Style,
                Tag = phonePanel
            };
            removeButton.Click += RemovePhoneNumber_Click;

            phonePanel.Children.Add(phoneTextBox);
            phonePanel.Children.Add(removeButton);
            PhoneNumbersPanel.Children.Add(phonePanel);
        }

        private void RemovePhoneNumber_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel panel = button.Tag as StackPanel;
            TextBox textBox = panel.Children[0] as TextBox;
            phoneNumberTextBoxes.Remove(textBox);
            PhoneNumbersPanel.Children.Remove(panel);
        }

        private void AddPhoneNumber_Click(object sender, RoutedEventArgs e)
        {
            AddPhoneNumberTextBox();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(PartnerNameInput.Text))
                {
                    MessageBox.Show("Partner name cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ContactPersonInput.Text))
                {
                    MessageBox.Show("Contact person cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ContractStartInput.SelectedDate.HasValue || !ContractEndInput.SelectedDate.HasValue)
                {
                    MessageBox.Show("Contract dates must be selected.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ContractEndInput.SelectedDate <= ContractStartInput.SelectedDate)
                {
                    MessageBox.Show("Contract end date must be after start date.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save changes to database
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE partners SET partner_name = @partnerName, partner_type = @partnerType, contact_person = @contactPerson, " +
                                   "credit_limit = @creditLimit, contract_start = @contractStart, contract_end = @contractEnd WHERE partner_id = @partnerId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@partnerName", PartnerNameInput.Text);
                        command.Parameters.AddWithValue("@partnerType", (PartnerTypeInput.SelectedItem as ComboBoxItem)?.Content.ToString());
                        command.Parameters.AddWithValue("@contactPerson", ContactPersonInput.Text);
                        command.Parameters.AddWithValue("@creditLimit", float.Parse(CreditLimitInput.Text));
                        command.Parameters.AddWithValue("@contractStart", ContractStartInput.SelectedDate.Value);
                        command.Parameters.AddWithValue("@contractEnd", ContractEndInput.SelectedDate.Value);
                        command.Parameters.AddWithValue("@partnerId", _partnerId);

                        command.ExecuteNonQuery();
                    }

                    query = "DELETE FROM partner_phones WHERE partner_id = @partnerId";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@partnerId", _partnerId);
                        command.ExecuteNonQuery();
                    }

                    foreach (TextBox phoneTextBox in phoneNumberTextBoxes)
                    {
                        if (!string.IsNullOrWhiteSpace(phoneTextBox.Text))
                        {
                            query = "INSERT INTO partner_phones (partner_id, phone_number) VALUES (@partnerId, @phoneNumber)";
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@partnerId", _partnerId);
                                command.Parameters.AddWithValue("@phoneNumber", phoneTextBox.Text);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                MessageBox.Show("Partner information updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
