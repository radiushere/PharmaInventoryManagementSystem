using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class NewOrderWindow : Window
    {
        private readonly Orders _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public NewOrderWindow(Orders parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            LoadDistributors();
            LoadProducts();
            dpOrderDate.SelectedDate = DateTime.Today;
            dpDeliveryDate.SelectedDate = DateTime.Today.AddDays(7);
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadDistributors()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT dist_id, name FROM distributors";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbDistributors.Items.Add(new
                            {
                                Id = reader.GetInt32("dist_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
            }
            cmbDistributors.DisplayMemberPath = "Name";
            cmbDistributors.SelectedValuePath = "Id";
        }

        private void LoadProducts()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT product_id, product_name, price FROM products";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbProducts.Items.Add(new
                            {
                                Id = reader.GetInt32("product_id"),
                                Name = reader.GetString("product_name"),
                                Price = reader.GetDecimal("price")
                            });
                        }
                    }
                }
            }
            cmbProducts.DisplayMemberPath = "Name";
            cmbProducts.SelectedValuePath = "Id";
            cmbProducts.SelectionChanged += CmbProducts_SelectionChanged;
        }

        private void CmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProducts.SelectedItem != null)
            {
                var selectedProduct = (dynamic)cmbProducts.SelectedItem;
                txtPrice.Text = selectedProduct.Price.ToString();
                CalculateTotalAmount();
            }
        }

        private void CalculateTotalAmount()
        {
            if (cmbProducts.SelectedItem != null && !string.IsNullOrEmpty(txtQuantity.Text))
            {
                var selectedProduct = (dynamic)cmbProducts.SelectedItem;
                decimal price = selectedProduct.Price;
                int quantity = int.Parse(txtQuantity.Text);
                txtTotalAmount.Text = (price * quantity).ToString();
            }
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotalAmount();
        }

        private bool ValidateInputs()
        {
            if (cmbDistributors.SelectedItem == null)
            {
                MessageBox.Show("Please select a distributor", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpOrderDate.SelectedDate == null || dpOrderDate.SelectedDate > DateTime.Today)
            {
                MessageBox.Show("Invalid order date", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpDeliveryDate.SelectedDate == null || dpDeliveryDate.SelectedDate < dpOrderDate.SelectedDate)
            {
                MessageBox.Show("Delivery date must be after order date", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Please select order status", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbProducts.SelectedItem == null || string.IsNullOrEmpty(txtQuantity.Text) || int.Parse(txtQuantity.Text) <= 0)
            {
                MessageBox.Show("Please select a product and enter a valid quantity", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var selectedProduct = (dynamic)cmbProducts.SelectedItem;
                            var selectedDistributor = (dynamic)cmbDistributors.SelectedItem;
                            var quantity = int.Parse(txtQuantity.Text);
                            var price = selectedProduct.Price * quantity;
                            var status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();

                            // Insert order
                            var orderQuery = @"INSERT INTO orders
                                            (dist_id, product_id, quantity, price, order_date, delivery_date, status)
                                            VALUES (@distId, @productId, @quantity, @price, @orderDate, @deliveryDate, @status);
                                            SELECT LAST_INSERT_ID();";

                            var orderCmd = new MySqlCommand(orderQuery, conn, transaction);
                            orderCmd.Parameters.AddWithValue("@distId", selectedDistributor.Id);
                            orderCmd.Parameters.AddWithValue("@productId", selectedProduct.Id);
                            orderCmd.Parameters.AddWithValue("@quantity", quantity);
                            orderCmd.Parameters.AddWithValue("@price", price);
                            orderCmd.Parameters.AddWithValue("@orderDate", dpOrderDate.SelectedDate);
                            orderCmd.Parameters.AddWithValue("@deliveryDate", dpDeliveryDate.SelectedDate);
                            orderCmd.Parameters.AddWithValue("@status", status);

                            int orderId = Convert.ToInt32(orderCmd.ExecuteScalar());

                            // If the order status is "Completed," save it to the sales table
                            if (status == "Completed")
                            {
                                SaveOrderToSales(orderId, selectedProduct.Id, quantity, price, dpDeliveryDate.SelectedDate.Value);
                            }

                            transaction.Commit();
                            MessageBox.Show("Order saved successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            _parentWindow.LoadOrdersData();
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error saving order: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveOrderToSales(int orderId, int productId, int quantity, decimal price, DateTime deliveryDate)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"INSERT INTO sales
                                  (product_id, quantity, payment_method, total_amount, sale_date)
                                  VALUES (@productId, @quantity, @paymentMethod, @totalAmount, @saleDate)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@paymentMethod", "unspecified");
                        cmd.Parameters.AddWithValue("@totalAmount", price);
                        cmd.Parameters.AddWithValue("@saleDate", deliveryDate);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order to sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}