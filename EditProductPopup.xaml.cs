using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleLoginWPF
{
    // Helper class for Supplier ComboBox
    public class SupplierComboBoxItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class EditProductPopup : Window
    {
        private int _productId;
        // This will now store the relative path like "Images/filename.jpg" or null
        private string _originalImagePath;

        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public EditProductPopup(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadComboBoxItems(); // Load categories and suppliers
            LoadProductData(productId);
        }

        private void LoadComboBoxItems()
        {
            // Populate Category ComboBox from the ENUM values directly from the schema
            CategoryComboBox.Items.Clear();
            CategoryComboBox.Items.Add("Injection");
            CategoryComboBox.Items.Add("Ointment");
            CategoryComboBox.Items.Add("Syrup");
            CategoryComboBox.Items.Add("Tablet");
            CategoryComboBox.Items.Add("Capsules");


            // Populate Supplier ComboBox
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT supplier_id, supplier_name FROM suppliers ORDER BY supplier_name;";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    List<SupplierComboBoxItem> suppliers = new List<SupplierComboBoxItem>();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(new SupplierComboBoxItem
                            {
                                Id = reader.GetInt32("supplier_id"),
                                Name = reader.GetString("supplier_name")
                            });
                        }
                    }
                    SupplierComboBox.ItemsSource = suppliers;
                }
            }
            catch (MySqlException mex)
            {
                MessageBox.Show($"Database error loading suppliers: {mex.Message}\nError Code: {mex.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData(int productId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT
                            p.product_id,
                            p.product_name,
                            p.description,
                            p.price,
                            p.batch_num,
                            p.expiry_date,
                            p.quantity,
                            p.product_image,
                            p.category,
                            p.supplier_id
                        FROM
                            products p
                        WHERE
                            p.product_id = @productId;
                    ";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@productId", productId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _productId = reader.GetInt32("product_id");

                            ProductNameTextBox.Text = reader["product_name"]?.ToString() ?? "";
                            ProductIdTextBox.Text = _productId.ToString();

                            string category = reader["category"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(category))
                            {
                                CategoryComboBox.SelectedItem = category;
                            }
                            else
                            {
                                CategoryComboBox.SelectedIndex = -1;
                            }

                            BatchNumberTextBox.Text = reader["batch_num"]?.ToString() ?? "";
                            QuantityTextBox.Text = reader["quantity"] != DBNull.Value ? reader.GetInt32("quantity").ToString() : "0";
                            UnitPriceTextBox.Text = reader["price"] != DBNull.Value ? reader.GetFloat("price").ToString() : "0.00";

                            if (reader["expiry_date"] != DBNull.Value)
                            {
                                ExpiryDatePicker.SelectedDate = reader.GetDateTime("expiry_date");
                            }
                            else
                            {
                                ExpiryDatePicker.SelectedDate = null;
                            }

                            if (reader["supplier_id"] != DBNull.Value)
                            {
                                SupplierComboBox.SelectedValue = reader.GetInt32("supplier_id");
                            }
                            else
                            {
                                SupplierComboBox.SelectedIndex = -1;
                            }

                            IndicationsTextBox.Text = reader["description"]?.ToString() ?? "";

                            // --- Image Loading Logic (Aligned with AddNewProduct) ---
                            _originalImagePath = reader["product_image"]?.ToString() ?? string.Empty; // This is the relative path from DB
                            if (!string.IsNullOrEmpty(_originalImagePath))
                            {
                                try
                                {
                                    // Construct the full path to the image file
                                    string imageFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _originalImagePath);
                                    if (File.Exists(imageFullPath))
                                    {
                                        // Use BitmapCacheOption.OnLoad to avoid file locking issues
                                        BitmapImage bitmap = new BitmapImage();
                                        bitmap.BeginInit();
                                        bitmap.UriSource = new Uri(imageFullPath);
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        bitmap.EndInit();
                                        ProductImageEdit.Source = bitmap;
                                    }
                                    else
                                    {
                                        // Fallback to default image if file not found
                                        ProductImageEdit.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                                    }
                                }
                                catch (Exception)
                                {
                                    // Fallback to default image on error
                                    ProductImageEdit.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                                }
                            }
                            else
                            {
                                // If no image path in DB, show default
                                ProductImageEdit.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                            }
                            // --- End Image Loading Logic ---
                        }
                        else
                        {
                            MessageBox.Show("Product not found for editing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Close();
                        }
                    }
                }
            }
            catch (MySqlException mex)
            {
                MessageBox.Show($"Database error loading product data: {mex.Message}\nError Code: {mex.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading product data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string productName = ProductNameTextBox.Text.Trim();
                string batchNum = BatchNumberTextBox.Text.Trim();
                string quantityString = QuantityTextBox.Text.Trim();
                string unitPriceString = UnitPriceTextBox.Text.Trim();
                DateTime? expiryDate = ExpiryDatePicker.SelectedDate;
                string indications = IndicationsTextBox.Text.Trim();

                string category = CategoryComboBox.SelectedItem?.ToString() ?? "";
                int? supplierId = (int?)SupplierComboBox.SelectedValue;

                if (string.IsNullOrWhiteSpace(productName))
                {
                    MessageBox.Show("Product name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(quantityString, out int quantity) || quantity < 0)
                {
                    MessageBox.Show("Quantity must be a valid non-negative number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!float.TryParse(unitPriceString, out float unitPrice) || unitPrice < 0)
                {
                    MessageBox.Show("Unit Price must be a valid non-negative number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (expiryDate == null)
                {
                    MessageBox.Show("Expiry Date is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(category))
                {
                    MessageBox.Show("Category is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (supplierId == null || supplierId == 0)
                {
                    MessageBox.Show("Supplier is required. Please select a supplier.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string updateProductQuery = @"
                        UPDATE products
                        SET
                            product_name = @productName,
                            description = @description,
                            price = @price,
                            batch_num = @batchNum,
                            expiry_date = @expiryDate,
                            quantity = @quantity,
                            product_image = @productImage,
                            category = @category,
                            supplier_id = @supplierId
                        WHERE
                            product_id = @productId;
                    ";
                    MySqlCommand productCmd = new MySqlCommand(updateProductQuery, connection);
                    productCmd.Parameters.AddWithValue("@productName", productName);
                    productCmd.Parameters.AddWithValue("@description", indications);
                    productCmd.Parameters.AddWithValue("@price", unitPrice);
                    productCmd.Parameters.AddWithValue("@batchNum", batchNum);
                    productCmd.Parameters.AddWithValue("@expiryDate", expiryDate);
                    productCmd.Parameters.AddWithValue("@quantity", quantity);
                    // Use _originalImagePath directly, which now holds the relative path
                    productCmd.Parameters.AddWithValue("@productImage", (object)_originalImagePath ?? DBNull.Value);
                    productCmd.Parameters.AddWithValue("@category", category);
                    productCmd.Parameters.AddWithValue("@supplierId", supplierId.Value);
                    productCmd.Parameters.AddWithValue("@productId", _productId);
                    productCmd.ExecuteNonQuery();

                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (MySqlException sqlEx)
            {
                MessageBox.Show($"Database error saving product: {sqlEx.Message}\nError Code: {sqlEx.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                try
                {
                    // --- Image Handling Logic (Aligned with AddNewProduct) ---
                    // Release the current image source to avoid file locking before copying
                    ProductImageEdit.Source = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    string fileName = Path.GetFileName(selectedFileName);
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    string destinationPath = Path.Combine(targetDirectory, fileName);

                    // Copy the file, overwrite if it exists
                    File.Copy(selectedFileName, destinationPath, true);

                    // Set the image source from the copied file
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(destinationPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Crucial for releasing file handle
                    bitmap.EndInit();
                    ProductImageEdit.Source = bitmap;

                    // Store the relative path for the database
                    _originalImagePath = $"Images/{fileName}";
                    // --- End Image Handling Logic ---
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show($"Error copying image file: {ioEx.Message}", "File I/O Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Revert to default or clear image if copying fails
                    ProductImageEdit.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                    _originalImagePath = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Image Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Revert to default or clear image if loading fails
                    ProductImageEdit.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                    _originalImagePath = null;
                }
            }
        }
    }
}