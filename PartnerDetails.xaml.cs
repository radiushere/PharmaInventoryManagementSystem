using System;
using System.Collections.Generic;
using System.Windows;

namespace SimpleLoginWPF
{
    /// <summary>
    /// Interaction logic for PartnerDetails.xaml
    /// </summary>
    public partial class PartnerDetails : Window
    {
        // Sample data model for Order history
        public class OrderData
        {
            public string OrderID { get; set; }
            public DateTime OrderDate { get; set; }
            public int ProductCount { get; set; }
            public decimal Total { get; set; }
            public string Status { get; set; }
        }

        public PartnerDetails(int partnerId)
        {
            InitializeComponent();

            // In a real application, you would load the partner details from the database using the partnerId
            LoadPartnerDetails(partnerId);
            LoadRecentOrders(partnerId);
        }

        private void LoadPartnerDetails(int partnerId)
        {
            // This method would normally fetch data from a database
            // For demonstration, we're using static sample data

            // For a real implementation, replace this with actual database queries
            // Example: var partnerData = _dbContext.Partners.FirstOrDefault(p => p.PartnerID == partnerId);

            // Set the partner details to the UI elements
            PartnerId.Text = "P1001";
            PartnerName.Text = "Allied Hospital";
            PartnerType.Text = "Hospital";
            PartnerInitials.Text = "AH"; // First letters of the partner name
            PartnerStatus.Text = "Active";

            ContactPerson.Text = "Dr. Sarah Johnson";
            ContactPhone.Text = "+1 (555) 123-4567";
            ContactEmail.Text = "sjohnson@alliedhospital.org";
            Address.Text = "1234 Medical Center Blvd, New York, NY 10001";
            SecondaryContact.Text = "Robert Williams (Operations Manager)";
            SecondaryPhone.Text = "+1 (555) 987-6543";

            ContractStart.Text = "January 15, 2023";
            ContractEnd.Text = "January 14, 2026";
            CreditLimit.Text = "$250,000.00";
            CurrentBalance.Text = "$78,450.25";
            PaymentTerms.Text = "Net 30";
            ContractType.Text = "Standard Supply";

            Notes.Text = "Allied Hospital is one of our premium partners. They typically order monthly with large volumes. They have specific requirements for packaging and delivery scheduling. Special discount of 12% applies to all orders. Contact Dr. Johnson for new product proposals.";
        }

        private void LoadRecentOrders(int partnerId)
        {
            // This would normally be fetched from a database
            // For demonstration, using sample data
            var orders = new List<OrderData>
            {
                new OrderData
                {
                    OrderID = "ORD-5423",
                    OrderDate = DateTime.Now.AddDays(-5),
                    ProductCount = 24,
                    Total = 12540.50m,
                    Status = "Delivered"
                },
                new OrderData
                {
                    OrderID = "ORD-5224",
                    OrderDate = DateTime.Now.AddDays(-35),
                    ProductCount = 18,
                    Total = 9875.75m,
                    Status = "Completed"
                },
                new OrderData
                {
                    OrderID = "ORD-5102",
                    OrderDate = DateTime.Now.AddDays(-65),
                    ProductCount = 30,
                    Total = 15980.25m,
                    Status = "Completed"
                },
                new OrderData
                {
                    OrderID = "ORD-4985",
                    OrderDate = DateTime.Now.AddDays(-95),
                    ProductCount = 22,
                    Total = 11450.00m,
                    Status = "Completed"
                }
            };

            OrdersDataGrid.ItemsSource = orders;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Return to the Partners management screen
            this.Close();
        }

        private void EditPartner_Click(object sender, RoutedEventArgs e)
        {
            var editPopup = new EditPartnerPopup(1001); // Example partner ID
            editPopup.Owner = this;
            if (editPopup.ShowDialog() == true)
            {
                // Refresh the partner details after editing
                LoadPartnerDetails(1001);
            }
        }

        private void ContactPartner_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this might open an email client or messaging system
            MessageBox.Show("Contact functionality would open here.", "Contact Partner",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Buttton(object sender, RoutedEventArgs e)
        {

        }
    }
}