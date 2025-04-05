using System.Windows;
using System.Collections.Generic;

namespace SimpleLoginWPF
{
    public partial class OrderDetails : Window
    {
        public class OrderItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total => Quantity * Price;
        }

        public OrderDetails()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var items = new List<OrderItem>
            {
                new OrderItem { Name = "Aspirin 500mg", Quantity = 100, Price = 25.50m },
                new OrderItem { Name = "Antibiotic X", Quantity = 50, Price = 45.75m },
                new OrderItem { Name = "Pain Reliever", Quantity = 75, Price = 12.99m }
            };

            OrderItemsGrid.ItemsSource = items;
        }

        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            InvoicePopup.Visibility = Visibility.Visible;
        }

        private void CloseInvoicePopup_Click(object sender, RoutedEventArgs e)
        {
            InvoicePopup.Visibility = Visibility.Collapsed;
        }

        private void DownloadInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invoice downloaded successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            InvoicePopup.Visibility = Visibility.Collapsed;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}