using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace SimpleLoginWPF
{
    public partial class EditDistributorPopup : Window
    {
        private readonly DistributorDetails _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private List<TextBox> phoneNumberTextBoxes = new List<TextBox>();
        private int _distributorId;

        public EditDistributorPopup(DistributorDetails parent, int distributorId)
        {
            InitializeComponent();
            _parentWindow = parent;
            _distributorId = distributorId;

            // Load distributor data
            LoadDistributorData();
        }

        private void LoadDistributorData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Fetch distributor details
                    var distributorQuery = @"SELECT name, region, distributor_type, email, service_area
                                            FROM distributors
                                            WHERE dist_id = @distributorId";

                    using (var distributorCmd = new MySqlCommand(distributorQuery, conn))
                    {
                        distributorCmd.Parameters.AddWithValue("@distributorId", _distributorId);

                        using (var reader = distributorCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtDistributorName.Text = reader["name"].ToString();
                                txtPrimaryRegion.Text = reader["region"].ToString();
                                txtServiceAreas.Text = reader["service_area"].ToString();
                                txtContactEmail.Text = reader["email"].ToString();
                                cmbDistributorType.SelectedItem = reader["distributor_type"].ToString();
                            }
                        }
                    }

                    // Fetch distributor phone numbers
                    var phonesQuery = @"SELECT phone
                                       FROM distributor_phones
                                       WHERE dist_id = @distributorId";

                    using (var phonesCmd = new MySqlCommand(phonesQuery, conn))
                    {
                        phonesCmd.Parameters.AddWithValue("@distributorId", _distributorId);

                        using (var reader = phonesCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AddPhoneNumberTextBox(reader["phone"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading distributor data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPhoneNumberTextBox(string phoneNumber = "")
        {
            StackPanel phonePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            TextBox phoneTextBox = new TextBox
            {
                Text = phoneNumber,
                Style = FindResource("InputBoxStyle") as Style,
                Height = 50,
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Update distributor details
                    var distributorQuery = @"UPDATE distributors
                                            SET name = @name, region = @region, distributor_type = @distributorType,
                                                email = @email, service_area = @serviceArea
                                            WHERE dist_id = @distributorId";

                    using (var distributorCmd = new MySqlCommand(distributorQuery, conn))
                    {
                        distributorCmd.Parameters.AddWithValue("@name", txtDistributorName.Text.Trim());
                        distributorCmd.Parameters.AddWithValue("@region", txtPrimaryRegion.Text.Trim());
                        distributorCmd.Parameters.AddWithValue("@distributorType", ((ComboBoxItem)cmbDistributorType.SelectedItem).Content.ToString());
                        distributorCmd.Parameters.AddWithValue("@email", txtContactEmail.Text.Trim());
                        distributorCmd.Parameters.AddWithValue("@serviceArea", txtServiceAreas.Text.Trim());
                        distributorCmd.Parameters.AddWithValue("@distributorId", _distributorId);

                        distributorCmd.ExecuteNonQuery();
                    }

                    // Delete existing phone numbers
                    var deletePhonesQuery = @"DELETE FROM distributor_phones WHERE dist_id = @distributorId";
                    using (var deletePhonesCmd = new MySqlCommand(deletePhonesQuery, conn))
                    {
                        deletePhonesCmd.Parameters.AddWithValue("@distributorId", _distributorId);
                        deletePhonesCmd.ExecuteNonQuery();
                    }

                    // Insert updated phone numbers
                    foreach (var phoneTextBox in phoneNumberTextBoxes)
                    {
                        var phoneQuery = @"INSERT INTO distributor_phones (dist_id, phone)
                                           VALUES (@distributorId, @phone)";

                        using (var phoneCmd = new MySqlCommand(phoneQuery, conn))
                        {
                            phoneCmd.Parameters.AddWithValue("@distributorId", _distributorId);
                            phoneCmd.Parameters.AddWithValue("@phone", phoneTextBox.Text.Trim());
                            phoneCmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Distributor details updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the parent window data
                _parentWindow.LoadDistributorDetails(_distributorId);

                // Close the popup
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving distributor: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmbDistributorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
