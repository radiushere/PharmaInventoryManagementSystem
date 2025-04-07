using System;
using System.Windows;

namespace SimpleLoginWPF
{
    public partial class EditPartnerPopup : Window
    {
        private int _partnerId;

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
                // In a real application, this would fetch data from a database
                // Since we're working with the same sample data, it's already loaded in the form

                // Initialize date pickers with default values
                ContractStartInput.SelectedDate = DateTime.Parse("January 15, 2023");
                ContractEndInput.SelectedDate = DateTime.Parse("January 14, 2026");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading partner data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                if (string.IsNullOrWhiteSpace(ContactPersonInput.Text) ||
                    string.IsNullOrWhiteSpace(ContactPhoneInput.Text) ||
                    string.IsNullOrWhiteSpace(ContactEmailInput.Text))
                {
                    MessageBox.Show("Contact information cannot be empty.", "Validation Error",
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

                // In a real application, save changes to database here
                // For demonstration, just show success message
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