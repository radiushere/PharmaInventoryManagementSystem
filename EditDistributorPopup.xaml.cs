using System.Windows;

namespace SimpleLoginWPF
{
    public partial class EditDistributorPopup : Window
    {
        private readonly DistributorDetails _parentWindow;

        public EditDistributorPopup(DistributorDetails parent)
        {
            InitializeComponent();
            _parentWindow = parent;

            // Load distributor data
            LoadDistributorData();
        }

        private void LoadDistributorData()
        {
            // This method would normally fetch data from a database or service
            // but here we're just pre-populating the fields from the parent window

            // Basic info is already populated in XAML
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Here you would save the data to your database

            // For demo purposes just show a success message
            MessageBox.Show("Distributor details updated successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Refresh the parent window data
            // In a real implementation, you would need to update the parent window's data
            // _parentWindow.LoadDistributorDetails(...)

            // Close the popup
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Just close the window without saving
            this.Close();
        }
    }
}