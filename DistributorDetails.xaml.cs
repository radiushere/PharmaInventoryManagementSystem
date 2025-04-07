using System.Windows;

namespace SimpleLoginWPF
{
    public partial class DistributorDetails : Window
    {
        private int _currentDistributorId;

        public DistributorDetails(int distributorId)
        {
            InitializeComponent();
            _currentDistributorId = distributorId;
            LoadDistributorDetails(distributorId);
        }

        private void LoadDistributorDetails(int distributorId)
        {
            // Sample static data - replace with real data in actual implementation
            DistributorId.Text = "D2001";
            DistributorName.Text = "PharmaMed Distributors";
            DistributorType.Text = "Regional Wholesaler";
            DistributorInitials.Text = "PM";
            DistributorStatus.Text = "Active";
            PrimaryRegion.Text = "Northeast";
            ServiceAreas.Text = "NY, PA, NJ, CT, MA";
            WarehouseLocations.Text = "Albany, NY; Philadelphia, PA";
            DeliveryRadius.Text = "300 miles";
            ContactPhone.Text = "+1 (800) 555-1234";
            EmergencyPhone.Text = "+1 (800) 555-5678";
            ContactEmail.Text = "support@pharmamed.com";
            Address.Text = "456 Distribution Parkway, Albany, NY 12205";
            OperationsManager.Text = "Michael Chen";
            SupportHours.Text = "24/7";
            DailyCapacity.Text = "1500 packages";
            ColdStorage.Text = "Yes (GMP Certified)";
            VehicleFleet.Text = "45 refrigerated trucks";
            Certifications.Text = "ISO 9001, GDP Compliant";
            Notes.Text = "Primary distributor for northeast region. Requires 48-hour notice for large orders. Maintains excellent cold chain compliance. Preferred partner for vaccine distribution.";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditDistributor_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the edit popup window
            EditDistributorPopup editPopup = new EditDistributorPopup(this);
            editPopup.Owner = this; // Set this window as the owner to center the popup relative to this window
            editPopup.ShowDialog(); // Show as modal dialog
        }

        private void RequestDelivery_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delivery request functionality would open here.", "Request Delivery",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitPopup_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}