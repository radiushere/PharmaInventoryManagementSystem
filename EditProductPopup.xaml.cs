using System;
using System.Windows;

namespace SimpleLoginWPF
{
    public partial class EditProductPopup : Window
    {
        private int _productId;

        public EditProductPopup(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductData(productId);
        }

        private void LoadProductData(int productId)
        {
            // In a real application, you would fetch the data from a database
            // This is just a simplified version that loads the same data

            // Basic information is already populated in XAML
            // You could populate with real data here if needed
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Collect all form data
                string productName = ProductNameTextBox.Text;
                string ndcCode = ProductIdTextBox.Text;
                string activeIngredient = ActiveIngredientTextBox.Text;
                string packageSize = PackageSizeTextBox.Text;
                string storage = StorageTextBox.Text;
                string manufacturer = ManufacturerTextBox.Text;
                string indications = IndicationsTextBox.Text;
                string halfLife = HalfLifeTextBox.Text;
                string bioavailability = BioavailabilityTextBox.Text;
                string proteinBinding = ProteinBindingTextBox.Text;
                string excretion = ExcretionTextBox.Text;
                DateTime? expiryDate = ExpiryDatePicker.SelectedDate;

                // Get values from ComboBoxes
                //string category = ((ComboBoxItem)CategoryComboBox.SelectedItem)?.Content.ToString();
                //string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content.ToString();
                //string dosageForm = ((ComboBoxItem)DosageFormComboBox.SelectedItem)?.Content.ToString();
                //string schedule = ((ComboBoxItem)ScheduleComboBox.SelectedItem)?.Content.ToString();
                //string pregnancyCategory = ((ComboBoxItem)PregnancyCategoryComboBox.SelectedItem)?.Content.ToString();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(ndcCode))
                {
                    MessageBox.Show("Product name and NDC code are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // In a real application, you would save these values to a database

                // Show success message
                MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Close the popup
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}