using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleLoginWPF
{
    public partial class AddPurchaseWindow : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private decimal currentProductPrice = 0; 

        public AddPurchaseWindow()
        {
            InitializeComponent();
            LoadSuppliers();
            LoadProducts(); 
            dpPurchaseDate.SelectedDate = DateTime.Now;
            UpdateTotalAmount(); 
        }

        private void LoadSuppliers()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT supplier_id, supplier_name FROM suppliers ORDER BY supplier_name";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            DataTable suppliersTable = new DataTable();
                            suppliersTable.Load(reader);
                            cmbSuppliers.ItemsSource = suppliersTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT product_id, product_name, price FROM products WHERE quantity > 0 ORDER BY product_name"; 
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            DataTable productsTable = new DataTable();
                            productsTable.Load(reader);
                            cmbProducts.ItemsSource = productsTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProducts.SelectedItem is DataRowView selectedProduct)
            {
                try
                {
                    currentProductPrice = Convert.ToDecimal(selectedProduct["price"]);
                    txtUnitPriceDisplay.Text = $"Rs {currentProductPrice:F2}";
                }
                catch (Exception ex)
                {
                    currentProductPrice = 0;
                    txtUnitPriceDisplay.Text = "Rs 0.00";
                    MessageBox.Show($"Error fetching product price: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                currentProductPrice = 0;
                txtUnitPriceDisplay.Text = "Rs 0.00";
            }
            UpdateTotalAmount();
        }

        private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            if (int.TryParse(txtQuantity.Text, out int quantity) && quantity > 0)
            {
                decimal totalAmount = currentProductPrice * quantity;
                txtTotalPriceDisplay.Text = $"Total Price: Rs {totalAmount:F2}";
            }
            else
            {
                txtTotalPriceDisplay.Text = "Total Price: Rs 0.00";
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); 
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSuppliers.SelectedValue == null ||
                dpPurchaseDate.SelectedDate == null ||
                cmbStatus.SelectedItem == null ||
                cmbProducts.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtQuantity.Text) ||
                !int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please fill in all required fields correctly (Supplier, Date, Status, Product, and valid Quantity).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedStatusItem = cmbStatus.SelectedItem as ComboBoxItem;
            if (selectedStatusItem == null)
            {
                MessageBox.Show("Invalid status selected.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string status = selectedStatusItem.Content.ToString();
            int productId = (int)cmbProducts.SelectedValue;
            decimal totalMonetaryAmount = currentProductPrice * quantity;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {

                            var purchaseQuery = @"INSERT INTO product_purchases 
                                                  (supplier_id, product_id, date, status, total_amount)
                                                  VALUES 
                                                  (@supplierId, @productId, @date, @status, @totalAmount)";
                            using (var purchaseCmd = new MySqlCommand(purchaseQuery, conn, transaction))
                            {
                                purchaseCmd.Parameters.AddWithValue("@supplierId", cmbSuppliers.SelectedValue);
                                purchaseCmd.Parameters.AddWithValue("@productId", productId);
                                purchaseCmd.Parameters.AddWithValue("@date", dpPurchaseDate.SelectedDate);
                                purchaseCmd.Parameters.AddWithValue("@status", status);
                                purchaseCmd.Parameters.AddWithValue("@totalAmount", totalMonetaryAmount); 

                                purchaseCmd.ExecuteNonQuery();
                            }

                            
                            if (status == "Completed")
                            {
                                UpdateProductQuantityInProductsTable(conn, transaction, productId, quantity);
                            }

                            transaction.Commit();
                            MessageBox.Show("Purchase saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error saving purchase: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to database: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProductQuantityInProductsTable(MySqlConnection conn, MySqlTransaction transaction, int productId, int purchasedQuantity)
        {
            var updateQuery = "UPDATE products SET quantity = quantity + @purchasedQuantity WHERE product_id = @productId";
            using (var updateCmd = new MySqlCommand(updateQuery, conn, transaction))
            {
                updateCmd.Parameters.AddWithValue("@purchasedQuantity", purchasedQuantity);
                updateCmd.Parameters.AddWithValue("@productId", productId);
                updateCmd.ExecuteNonQuery();
            }
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