using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class PurchaseDetails : Window
    {
        public class PurchaseMaterial
        {
            public string MaterialName { get; set; }
            public string BatchNumber { get; set; }
            public decimal Quantity { get; set; }
            public string Unit { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
            public DateTime ExpiryDate { get; set; }
        }

        public PurchaseDetails()
        {
            InitializeComponent();
            LoadPurchaseData();
        }

        private void LoadPurchaseData()
        {
            // Sample purchase materials data
            var materials = new List<PurchaseMaterial>
            {
                new PurchaseMaterial
                {
                    MaterialName = "Paracetamol API (USP Grade)",
                    BatchNumber = "PC-API-1123",
                    Quantity = 500,
                    Unit = "kg",
                    UnitPrice = 32.50m,
                    ExpiryDate = new DateTime(2025, 6, 30)
                },
                new PurchaseMaterial
                {
                    MaterialName = "Ibuprofen API (BP Grade)",
                    BatchNumber = "IB-API-0923",
                    Quantity = 300,
                    Unit = "kg",
                    UnitPrice = 28.75m,
                    ExpiryDate = new DateTime(2025, 3, 15)
                },
                new PurchaseMaterial
                {
                    MaterialName = "Microcrystalline Cellulose",
                    BatchNumber = "MCC-1023",
                    Quantity = 1000,
                    Unit = "kg",
                    UnitPrice = 5.20m,
                    ExpiryDate = new DateTime(2026, 1, 15)
                },
                new PurchaseMaterial
                {
                    MaterialName = "Magnesium Stearate",
                    BatchNumber = "MG-ST-0823",
                    Quantity = 200,
                    Unit = "kg",
                    UnitPrice = 12.80m,
                    ExpiryDate = new DateTime(2025, 12, 1)
                }
            };

            MaterialsDataGrid.ItemsSource = materials;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PrintDetails_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Purchase details would be printed here", "Print Purchase",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReceiveMaterials_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Materials receipt process would start here", "Receive Materials",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the custom edit dialog instead of MessageBox
            EditPurchaseDialog editDialog = new EditPurchaseDialog(this);
            editDialog.Owner = this; // Set owner to center the dialog over this window
            editDialog.ShowDialog();
        }
    }
}