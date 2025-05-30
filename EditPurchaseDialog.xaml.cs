using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

namespace SimpleLoginWPF
{
    public partial class EditPurchaseDialog : Window
    {
        private readonly PurchaseDetails _parentWindow;
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public int PurchaseId { get; set; }

        public EditPurchaseDialog(PurchaseDetails parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;

            if (int.TryParse(_parentWindow.PurchaseId.Text, out int parsedPurchaseId))
            {
                PurchaseId = parsedPurchaseId;
            }
            else
            {
                MessageBox.Show("Invalid Purchase ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PurchaseId = 0;
            }

            LoadSuppliers();
            LoadProducts();
            LoadCurrentPurchaseData();
        }

        private void LoadSuppliers()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT supplier_id, supplier_name FROM suppliers";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SupplierComboBox.Items.Add(new
                                {
                                    Id = reader["supplier_id"],
                                    Name = reader["supplier_name"].ToString()
                                });
                            }
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
                    var query = "SELECT product_id, product_name FROM products";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductComboBox.Items.Add(new
                                {
                                    Id = reader["product_id"],
                                    Name = reader["product_name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCurrentPurchaseData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"SELECT pp.purchase_id, pp.date, pp.supplier_id, pp.product_id, pp.status, pp.total_amount
                                  FROM product_purchases pp
                                  WHERE pp.purchase_id = @purchaseId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@purchaseId", _parentWindow.PurchaseId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PurchaseIdTextBox.Text = reader["purchase_id"].ToString();
                                PurchaseDatePicker.SelectedDate = (DateTime)reader["date"];
                                SetComboBoxSelectedItem(SupplierComboBox, reader["supplier_id"].ToString());
                                SetComboBoxSelectedItem(ProductComboBox, reader["product_id"].ToString());
                                SetComboBoxSelectedItem(StatusComboBox, reader["status"].ToString());
                                TotalAmountTextBox.Text = reader["total_amount"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetComboBoxSelectedItem(System.Windows.Controls.ComboBox comboBox, string value)
        {
            foreach (var item in comboBox.Items)
            {
                var comboBoxItem = (dynamic)item;
                if (comboBoxItem.Id.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"UPDATE product_purchases
                                  SET date = @date, supplier_id = @supplierId, product_id = @productId, status = @status, total_amount = @totalAmount
                                  WHERE purchase_id = @purchaseId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@purchaseId", PurchaseIdTextBox.Text);
                        cmd.Parameters.AddWithValue("@date", PurchaseDatePicker.SelectedDate);
                        cmd.Parameters.AddWithValue("@supplierId", ((dynamic)SupplierComboBox.SelectedItem).Id);
                        cmd.Parameters.AddWithValue("@productId", ((dynamic)ProductComboBox.SelectedItem).Id);
                        cmd.Parameters.AddWithValue("@status", ((System.Windows.Controls.ComboBoxItem)StatusComboBox.SelectedItem).Content);
                        cmd.Parameters.AddWithValue("@totalAmount", TotalAmountTextBox.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving purchase data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
