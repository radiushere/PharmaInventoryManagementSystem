using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SimpleLoginWPF
{
    public partial class AddItemDialog : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public int SelectedProductId { get; private set; }
        public string SelectedProductName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        public AddItemDialog()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT product_id, product_name, price FROM products";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            DataTable productsTable = new DataTable();
                            productsTable.Load(reader);
                            cmbProducts.ItemsSource = productsTable.DefaultView;
                            cmbProducts.DisplayMemberPath = "product_name";
                            cmbProducts.SelectedValuePath = "product_id";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProducts.SelectedValue == null || string.IsNullOrEmpty(txtQuantity.Text))
            {
                MessageBox.Show("Please select a product and enter the quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedProductId = (int)cmbProducts.SelectedValue;
            SelectedProductName = cmbProducts.Text;
            UnitPrice = GetProductPrice(SelectedProductId);
            Quantity = int.Parse(txtQuantity.Text);

            DialogResult = true;
            this.Close();
        }

        private decimal GetProductPrice(int productId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = "SELECT price FROM products WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        return Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching product price: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
