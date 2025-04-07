using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class AddPurchaseWindow : Window
    {
        public class PurchaseItem
        {
            public string Medicine { get; set; }
            public string BatchNumber { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public DateTime ExpiryDate { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        public AddPurchaseWindow()
        {
            InitializeComponent();

            // Set default dates
            dpPurchaseDate.SelectedDate = DateTime.Today;
            dpDeliveryDate.SelectedDate = DateTime.Today.AddDays(7);

            // Initialize with some sample items
            dgPurchaseItems.ItemsSource = new List<PurchaseItem>
            {
                new PurchaseItem
                {
                    Medicine = "Paracetamol 500mg",
                    BatchNumber = "BATCH001",
                    Quantity = 100,
                    UnitPrice = 2.50m,
                    ExpiryDate = DateTime.Today.AddYears(2)
                },
                new PurchaseItem
                {
                    Medicine = "Ibuprofen 200mg",
                    BatchNumber = "BATCH002",
                    Quantity = 50,
                    UnitPrice = 3.75m,
                    ExpiryDate = DateTime.Today.AddYears(2)
                }
            };

            UpdateTotalAmount();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            //var items = dgPurchaseItems.ItemsSource as List<PurchaseItem> ?? new List<PurchaseItem>();
            //items.Add(new PurchaseItem
            //{
            //    Medicine = "Amoxicillin 250mg",
            //    BatchNumber = "NEWBATCH",
            //    Quantity = 1,
            //    UnitPrice = 5.00m,
            //    ExpiryDate = DateTime.Today.AddYears(1)
            //});
            //dgPurchaseItems.ItemsSource = items;
            //UpdateTotalAmount();

            var addPurchaseWindow = new AddItemDialog();
            addPurchaseWindow.ShowDialog();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgPurchaseItems.SelectedItem is PurchaseItem item)
            {
                var items = dgPurchaseItems.ItemsSource as List<PurchaseItem>;
                items?.Remove(item);
                dgPurchaseItems.ItemsSource = items;
                UpdateTotalAmount();
            }
        }

        private void UpdateTotalAmount()
        {
            decimal total = 0;
            if (dgPurchaseItems.ItemsSource is List<PurchaseItem> items)
            {
                foreach (var item in items)
                {
                    total += item.Total;
                }
            }
            txtTotalAmount.Text = $"Total Amount: {total:C}";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Purchase saved successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}