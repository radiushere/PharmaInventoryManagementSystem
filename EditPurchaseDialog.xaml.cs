using System;
using System.Windows;

namespace SimpleLoginWPF
{
    public partial class EditPurchaseDialog : Window
    {
        private readonly PurchaseDetails _parentWindow;

        public EditPurchaseDialog(PurchaseDetails parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            LoadCurrentPurchaseData();
        }

        private void LoadCurrentPurchaseData()
        {
            // Load current purchase data from parent window
            PurchaseIdTextBox.Text = _parentWindow.PurchaseId.Text;
            PurchaseDatePicker.SelectedDate = DateTime.Parse(_parentWindow.PurchaseDate.Text);
            DeliveryDatePicker.SelectedDate = DateTime.Parse(_parentWindow.DeliveryDate.Text);
            TotalAmountTextBox.Text = _parentWindow.TotalAmount.Text.TrimStart('$');
            NotesTextBox.Text = _parentWindow.PurchaseNotes.Text;

            // Set selected values in combo boxes
            SetComboBoxSelectedItem(SupplierComboBox, _parentWindow.SupplierName.Text);
            SetComboBoxSelectedItem(StatusComboBox, _parentWindow.PurchaseStatus.Text);
            SetComboBoxSelectedItem(PaymentMethodComboBox, _parentWindow.PaymentMethod.Text);
        }

        private void SetComboBoxSelectedItem(System.Windows.Controls.ComboBox comboBox, string value)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the parent window's data with edited values
                _parentWindow.PurchaseDate.Text = PurchaseDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                _parentWindow.DeliveryDate.Text = DeliveryDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "";

                _parentWindow.SupplierName.Text = ((System.Windows.Controls.ComboBoxItem)SupplierComboBox.SelectedItem).Content.ToString();
                _parentWindow.PurchaseStatus.Text = ((System.Windows.Controls.ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();
                _parentWindow.PaymentMethod.Text = ((System.Windows.Controls.ComboBoxItem)PaymentMethodComboBox.SelectedItem).Content.ToString();

                _parentWindow.TotalAmount.Text = "$" + TotalAmountTextBox.Text.TrimStart('$');
                _parentWindow.PurchaseNotes.Text = NotesTextBox.Text;

                // Update the purchase title to reflect any changes
                _parentWindow.PurchaseTitle.Text = $"Raw Materials Purchase #{PurchaseIdTextBox.Text}";

                MessageBox.Show("Purchase details updated successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}