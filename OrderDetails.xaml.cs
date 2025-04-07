using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        private ObservableCollection<OrderItem> _orderItems;
        private ObservableCollection<OrderItem> _editItems;

        public OrderDetails()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            _orderItems = new ObservableCollection<OrderItem>
            {
                new OrderItem { Name = "Aspirin 500mg", Quantity = 100, Price = 25.50m },
                new OrderItem { Name = "Antibiotic X", Quantity = 50, Price = 45.75m },
                new OrderItem { Name = "Pain Reliever", Quantity = 75, Price = 12.99m }
            };
            OrderItemsGrid.ItemsSource = _orderItems;
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

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            // Create a copy of the order items for editing
            _editItems = new ObservableCollection<OrderItem>();

            foreach (var item in _orderItems)
            {
                _editItems.Add(new OrderItem
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            // Set the edit items as the source for the edit control
            EditItemsControl.ItemsSource = _editItems;

            // Show the edit popup
            EditOrderPopup.Visibility = Visibility.Visible;
        }

        private void CloseEditPopup_Click(object sender, RoutedEventArgs e)
        {
            EditOrderPopup.Visibility = Visibility.Collapsed;
        }

        private void SaveOrderChanges_Click(object sender, RoutedEventArgs e)
        {
            // Update the original order items with the edited values
            _orderItems.Clear();

            foreach (var item in _editItems)
            {
                _orderItems.Add(new OrderItem
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            // Refresh the data grid
            OrderItemsGrid.ItemsSource = null;
            OrderItemsGrid.ItemsSource = _orderItems;

            // Close the edit popup
            EditOrderPopup.Visibility = Visibility.Collapsed;

            // Show confirmation message
            MessageBox.Show("Order updated successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}