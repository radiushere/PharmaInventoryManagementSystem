using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Configuration;
using System.IO;
using Microsoft.Win32;

namespace SimpleLoginWPF
{
    public partial class NewProductWindow : Window
    {
        private readonly MainWindow _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private string _selectedImagePath = null;

        public NewProductWindow(MainWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            dpExpiryDate.SelectedDate = DateTime.Today.AddYears(1); // Default expiry is one year from now
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter a valid quantity", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrice.Text) || !decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtBatchNo.Text))
            {
                MessageBox.Show("Batch number is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpExpiryDate.SelectedDate == null)
            {
                MessageBox.Show("Expiry date is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpExpiryDate.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Expiry date cannot be in the past", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png",
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
                    MessageBox.Show($"Error loading image: {ex.Message}", "Image Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    _selectedImagePath = null;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                byte[] imageData = null;
                string imagePathInDb = null;

                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    imageData = File.ReadAllBytes(_selectedImagePath);
                    string fileName = System.IO.Path.GetFileName(_selectedImagePath);

                    // Store the image in a fixed folder (optional)
                    string targetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    string destination = System.IO.Path.Combine(targetPath, fileName);
                    File.Copy(_selectedImagePath, destination, true);

                    // Set image path for DB (relative or absolute as needed)
                    imagePathInDb = $"Images/{fileName}";
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = @"INSERT INTO medicines 
                                (name, quantity, price, batch_number, expiry_date, description, image_path)
                                VALUES
                                (@name, @quantity, @price, @batchNumber, @expiryDate, @description, @imagePath)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@quantity", int.Parse(txtQuantity.Text));
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@batchNumber", txtBatchNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@expiryDate", dpExpiryDate.SelectedDate);
                        cmd.Parameters.AddWithValue("@description", txtDescription.Text.Trim());

                        if (!string.IsNullOrEmpty(imagePathInDb))
                            cmd.Parameters.AddWithValue("@imagePath", imagePathInDb);
                        else
                            cmd.Parameters.AddWithValue("@imagePath", DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                _parentWindow.LoadMedicinesData();
                _parentWindow.SetupStats();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
