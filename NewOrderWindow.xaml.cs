using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Linq;

namespace SimpleLoginWPF
{
    public partial class NewOrderWindow : Window
    {
        private readonly Orders _parentWindow;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private List<Medicine> _medicines;
        private List<Supplier> _suppliers;

        public class OrderItem
        {
            public Medicine Medicine { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        public NewOrderWindow(Orders parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            LoadSuppliers();
            LoadMedicines();
            dpOrderDate.SelectedDate = DateTime.Today;
            dpDeliveryDate.SelectedDate = DateTime.Today.AddDays(7);
            cmbStatus.SelectedIndex = 0;
            dgOrderItems.ItemsSource = new List<OrderItem>();
        }

        private void LoadSuppliers()
        {
            _suppliers = new List<Supplier>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT supplier_id, name FROM suppliers";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _suppliers.Add(new Supplier
                            {
                                SupplierId = reader.GetInt32("supplier_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
            }
            cmbSuppliers.ItemsSource = _suppliers;
            cmbSuppliers.DisplayMemberPath = "Name";
        }

        private void LoadMedicines()
        {
            _medicines = new List<Medicine>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT id, name FROM medicines";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _medicines.Add(new Medicine
                            {
                                MedicineId = reader.GetInt32("id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
            }
            var medicineColumn = dgOrderItems.Columns[0] as DataGridComboBoxColumn;
            medicineColumn.ItemsSource = _medicines;
        }

        private bool ValidateInputs()
        {
            if (cmbSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Please select a supplier", "Validation Error",
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

            if (dgOrderItems.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one order item", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            foreach (OrderItem item in dgOrderItems.Items)
            {
                if (item.Medicine == null || item.Quantity <= 0 || item.UnitPrice <= 0)
                {
                    MessageBox.Show("Invalid item data", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            AddItemDialog addItemDialog = new AddItemDialog();
            addItemDialog.ShowDialog();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrderItems.SelectedItem is OrderItem item)
            {
                dgOrderItems.Items.Remove(item);
                UpdateTotalAmount();
            }
        }

        private void UpdateTotalAmount()
        {
            decimal total = 0;
            foreach (OrderItem item in dgOrderItems.Items)
            {
                total += item.Total;
            }
            txtTotalAmount.Text = $"Total Amount: {total:C}";
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
                            // Insert order
                            var orderQuery = @"INSERT INTO orders 
                                            (supplier_id, order_date, delivery_date, status, total_amount)
                                            VALUES (@supplierId, @orderDate, @deliveryDate, @status, @totalAmount);
                                            SELECT LAST_INSERT_ID();";

                            decimal totalAmount = ((IEnumerable<OrderItem>)dgOrderItems.ItemsSource).Sum(item => item.Total);

                            var orderCmd = new MySqlCommand(orderQuery, conn, transaction);
                            orderCmd.Parameters.AddWithValue("@supplierId", ((Supplier)cmbSuppliers.SelectedItem).SupplierId);
                            orderCmd.Parameters.AddWithValue("@orderDate", dpOrderDate.SelectedDate);
                            orderCmd.Parameters.AddWithValue("@deliveryDate", dpDeliveryDate.SelectedDate);
                            orderCmd.Parameters.AddWithValue("@status", ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString());
                            orderCmd.Parameters.AddWithValue("@totalAmount", totalAmount);

                            int orderId = Convert.ToInt32(orderCmd.ExecuteScalar());

                            // Insert order items
                            foreach (OrderItem item in dgOrderItems.Items)
                            {
                                var itemQuery = @"INSERT INTO order_items 
                                                (order_id, medicine_id, quantity, unit_price)
                                                VALUES (@orderId, @medicineId, @quantity, @unitPrice)";

                                var itemCmd = new MySqlCommand(itemQuery, conn, transaction);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@medicineId", item.Medicine.MedicineId);
                                itemCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                                itemCmd.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                                itemCmd.ExecuteNonQuery();
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

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }

    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
    }

    public class Medicine
    {
        public int MedicineId { get; set; }
        public string Name { get; set; }
    }



}