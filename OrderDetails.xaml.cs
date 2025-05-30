using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class OrderDetails : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private int orderId;

        public OrderDetails(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            LoadProducts();
            LoadOrderData();
        }

        private class ProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private void LoadOrderData()
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                var query = @"SELECT o.order_id, p.product_id, p.product_name, o.quantity, o.price, o.status, o.order_date, o.delivery_date
                              FROM orders o
                              JOIN products p ON o.product_id = p.product_id
                              WHERE o.order_id = @orderId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    OrderIdText.Text = reader["order_id"].ToString();
                    TotalAmountText.Text = $"${reader.GetDecimal("price")}";
                    StatusText.Text = reader["status"].ToString();

                    // Display in data grid
                    var orderItem = new
                    {
                        ProductName = reader["product_name"].ToString(),
                        Quantity = reader.GetInt32("quantity"),
                        Price = reader.GetDecimal("price"),
                        Total = reader.GetDecimal("price") * reader.GetInt32("quantity")
                    };

                    OrderItemsGrid.ItemsSource = new[] { orderItem };

                    // Set edit fields
                    cmbEditProducts.SelectedValue = reader.GetInt32("product_id");
                    QuantityTextBox.Text = reader["quantity"].ToString();
                    PriceTextBox.Text = reader["price"].ToString();
                    dpEditOrderDate.SelectedDate = reader.GetDateTime("order_date");
                    dpEditDeliveryDate.SelectedDate = reader.GetDateTime("delivery_date");
                    cmbEditStatus.Text = reader["status"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            var productList = new List<ProductItem>();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var query = "SELECT product_id, product_name, price FROM products";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productList.Add(new ProductItem
                    {
                        Id = reader.GetInt32("product_id"),
                        Name = reader.GetString("product_name"),
                        Price = reader.GetDecimal("price")
                    });
                }

                cmbEditProducts.ItemsSource = productList;
                cmbEditProducts.DisplayMemberPath = "Name";
                cmbEditProducts.SelectedValuePath = "Id";
                cmbEditProducts.SelectionChanged += CmbEditProducts_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbEditProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEditProducts.SelectedItem is ProductItem selectedProduct)
            {
                PriceTextBox.Text = selectedProduct.Price.ToString("F2");
                CalculateTotalAmount();
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e) => CalculateTotalAmount();

        private void CalculateTotalAmount()
        {
            if (cmbEditProducts.SelectedItem is ProductItem selectedProduct &&
                int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                decimal total = selectedProduct.Price * quantity;
                PriceTextBox.Text = total.ToString("F2");
            }
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e) => EditOrderPopup.Visibility = Visibility.Visible;

        private void CloseEditPopup_Click(object sender, RoutedEventArgs e) => EditOrderPopup.Visibility = Visibility.Collapsed;

        private void SaveOrderChanges_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEditProducts.SelectedItem is not ProductItem selectedProduct ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) ||
                !decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                MessageBox.Show("Invalid input data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                var status = cmbEditStatus.Text;

                var updateQuery = @"UPDATE orders
                                    SET product_id = @productId,
                                        quantity = @quantity,
                                        price = @price,
                                        order_date = @orderDate,
                                        delivery_date = @deliveryDate,
                                        status = @status
                                    WHERE order_id = @orderId";

                using var updateCmd = new MySqlCommand(updateQuery, conn, transaction);
                updateCmd.Parameters.AddWithValue("@orderId", orderId);
                updateCmd.Parameters.AddWithValue("@productId", selectedProduct.Id);
                updateCmd.Parameters.AddWithValue("@quantity", quantity);
                updateCmd.Parameters.AddWithValue("@price", price);
                updateCmd.Parameters.AddWithValue("@orderDate", dpEditOrderDate.SelectedDate);
                updateCmd.Parameters.AddWithValue("@deliveryDate", dpEditDeliveryDate.SelectedDate);
                updateCmd.Parameters.AddWithValue("@status", status);
                updateCmd.ExecuteNonQuery();

                if (status == "Completed")
                {
                    SaveOrderToSales(orderId, selectedProduct.Id, quantity, price, dpEditDeliveryDate.SelectedDate.Value);
                }

                transaction.Commit();
                MessageBox.Show("Order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                EditOrderPopup.Visibility = Visibility.Collapsed;
                LoadOrderData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveOrderToSales(int orderId, int productId, int quantity, decimal price, DateTime deliveryDate)
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM sales WHERE order_id = @orderId", conn);
                checkCmd.Parameters.AddWithValue("@orderId", orderId);
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0) return;

                var insertCmd = new MySqlCommand(@"INSERT INTO sales (order_id, product_id, quantity, payment_method, total_amount, sale_date)
                                                   VALUES (@orderId, @productId, @quantity, 'unspecified', @totalAmount, @saleDate)", conn);
                insertCmd.Parameters.AddWithValue("@orderId", orderId);
                insertCmd.Parameters.AddWithValue("@productId", productId);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@totalAmount", price);
                insertCmd.Parameters.AddWithValue("@saleDate", deliveryDate);
                insertCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            InvoicePopup.Visibility = Visibility.Visible;
        }

        private void DownloadInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use SaveFileDialog to let the user choose the save location
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Invoice_Order_{OrderIdText.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    DefaultExt = ".txt",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    GenerateTextInvoice(filePath); // Call the new text invoice generation method
                    MessageBox.Show($"Invoice downloaded to:\n{filePath}", "Invoice Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating invoice download: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CloseInvoicePopup_Click(sender, e); // Assuming this closes your popup
        }

        private void CloseInvoicePopup_Click(object sender, RoutedEventArgs e) => InvoicePopup.Visibility = Visibility.Collapsed;

        private void GenerateTextInvoice(string filePath)
        {
            try
            {
                StringBuilder invoiceContent = new StringBuilder();

                invoiceContent.AppendLine("------------------------------------");
                invoiceContent.AppendLine("             INVOICE              ");
                invoiceContent.AppendLine("------------------------------------");
                invoiceContent.AppendLine();
                invoiceContent.AppendLine($"Order ID:     {OrderIdText.Text}");
                invoiceContent.AppendLine($"Total Amount: {TotalAmountText.Text}");
                invoiceContent.AppendLine($"Status:       {StatusText.Text}");
                invoiceContent.AppendLine();
                invoiceContent.AppendLine("PRODUCT DETAILS:");
                invoiceContent.AppendLine("------------------------------------");
                invoiceContent.AppendLine("Product Name           Qty    Price     Total");
                invoiceContent.AppendLine("------------------------------------");

                // Assuming OrderItemsGrid.Items contains objects with ProductName, Quantity, Price, Total properties
                // This iterates through ALL items in the DataGrid, not just the first one.
                foreach (var item in OrderItemsGrid.Items)
                {
                    // Using dynamic or casting to a known type is safer if possible
                    // For simplicity, we'll continue with reflection as in your original code
                    var productName = item.GetType().GetProperty("ProductName")?.GetValue(item)?.ToString() ?? "N/A";
                    var quantity = item.GetType().GetProperty("Quantity")?.GetValue(item)?.ToString() ?? "0";
                    var price = item.GetType().GetProperty("Price")?.GetValue(item)?.ToString() ?? "0.00";
                    var total = item.GetType().GetProperty("Total")?.GetValue(item)?.ToString() ?? "0.00";

                    // Basic formatting to align columns
                    invoiceContent.AppendLine($"{productName.PadRight(22)} {quantity.PadLeft(3)}    ${price.PadRight(8)}  ${total.PadLeft(8)}");
                }
                invoiceContent.AppendLine("------------------------------------");
                invoiceContent.AppendLine();
                invoiceContent.AppendLine("Thank you for your business!");
                invoiceContent.AppendLine("------------------------------------");

                // Write the content to the specified file
                File.WriteAllText(filePath, invoiceContent.ToString());
            }
            catch (Exception ex)
            {
                // Rethrow the exception to be caught by the DownloadInvoice_Click handler
                throw new Exception($"Failed to generate invoice content: {ex.Message}", ex);
            }
        }
    }
}
