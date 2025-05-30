using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace SimpleLoginWPF
{
    public class SupplierDisplayItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Display => $"{Id} - {Name}";
    }

    public partial class NewProductWindow : Window
    {
        private readonly MainWindow _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private string _selectedImagePath = null;

        public NewProductWindow(MainWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            dpExpiryDate.SelectedDate = DateTime.Today.AddYears(1);

            LoadCategories();
            LoadSuppliers();
        }

        private void LoadCategories()
        {
            if (cmbCategory.Items.Count > 0)
            {
                cmbCategory.SelectedIndex = 0;
            }
        }


        private void LoadSuppliers()
        {
            List<SupplierDisplayItem> suppliers = new List<SupplierDisplayItem>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT supplier_id, supplier_name FROM suppliers ORDER BY supplier_name;";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suppliers.Add(new SupplierDisplayItem
                                {
                                    Id = reader.GetInt32("supplier_id"),
                                    Name = reader.GetString("supplier_name")
                                });
                            }
                        }
                    }
                }

                cmbSupplier.ItemsSource = suppliers;
                cmbSupplier.DisplayMemberPath = "Display";
                cmbSupplier.SelectedValuePath = "Id";

                if (suppliers.Any())
                {
                    cmbSupplier.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter a valid non-negative quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrice.Text) || !decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid non-negative price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtBatchNo.Text))
            {
                MessageBox.Show("Batch number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpExpiryDate.SelectedDate == null)
            {
                MessageBox.Show("Expiry date is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpExpiryDate.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Expiry date cannot be in the past.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Product category is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Supplier is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
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
                _selectedImagePath = openFileDialog.FileName;
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgProduct.Source = bitmap;
                    txtNoImage.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Image Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _selectedImagePath = null;
                    imgProduct.Source = null;
                    txtNoImage.Visibility = Visibility.Visible;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                string imagePathInDb = null;
                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    // To release the file handle before copying
                    imgProduct.Source = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    string fileName = Path.GetFileName(_selectedImagePath);
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    string destinationPath = Path.Combine(targetDirectory, fileName);

                    File.Copy(_selectedImagePath, destinationPath, true);
                    imagePathInDb = $"Images/{fileName}";
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var productQuery = @"INSERT INTO products
                                           (product_name, description, price, batch_num, expiry_date, quantity, product_image, category, supplier_id)
                                           VALUES
                                           (@name, @description, @price, @batchNumber, @expiryDate, @quantity, @imagePath, @category, @supplierId);
                                           SELECT LAST_INSERT_ID();";

                            int productId;
                            using (var cmd = new MySqlCommand(productQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                                cmd.Parameters.AddWithValue("@description", txtDescription.Text.Trim());
                                cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                                cmd.Parameters.AddWithValue("@batchNumber", txtBatchNo.Text.Trim());
                                cmd.Parameters.AddWithValue("@expiryDate", dpExpiryDate.SelectedDate);
                                cmd.Parameters.AddWithValue("@quantity", int.Parse(txtQuantity.Text));
                                cmd.Parameters.AddWithValue("@imagePath", (object)imagePathInDb ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@category", ((ComboBoxItem)cmbCategory.SelectedItem).Content.ToString());
                                cmd.Parameters.AddWithValue("@supplierId", (int)cmbSupplier.SelectedValue);

                                productId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            transaction.Commit();
                            MessageBox.Show("Product saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Database error: {ex.Message}\n\nDetails: {ex.ToString()}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"An unexpected error occurred: {ex.Message}\n\nDetails: {ex.ToString()}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                _parentWindow.LoadMedicinesData();
                _parentWindow.SetupStats();
                this.Close();
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Error copying image file: {ioEx.Message}", "File I/O Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Operation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}