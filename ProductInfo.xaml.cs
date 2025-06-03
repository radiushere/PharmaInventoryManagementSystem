using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Media; 
using System.Windows.Media.Imaging;
using System.Configuration;
using System.IO; 

namespace SimpleLoginWPF
{
    public partial class ProductInfo : Window
    {
        private int _productId; 
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        // Data Models for DataGrids
        public class SalesRecord
        {
            public int SaleID { get; set; }
            public DateTime SaleDate { get; set; }
            public int Quantity { get; set; }
            public string PaymentMethod { get; set; }
            public decimal TotalAmount { get; set; }
            public string Partner { get; set; } 
        }

        public class PurchaseRecord
        {
            public int PurchaseID { get; set; }
            public string SupplierName { get; set; }
            public DateTime PurchaseDate { get; set; }
            public string Status { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class OrderRecord
        {
            public int OrderID { get; set; }
            public string DistributorName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? DeliveryDate { get; set; } 
            public string Status { get; set; }
        }


        public ProductInfo(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductDetails(_productId);
            LoadSalesHistory(_productId);      
            LoadPurchasesHistory(_productId);
            LoadOrdersHistory(_productId);
        }

        private void LoadProductDetails(int productId)
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
                            s.supplier_name
                        FROM
                            products p
                        LEFT JOIN
                            suppliers s ON p.supplier_id = s.supplier_id
                        WHERE
                            p.product_id = @productId;
                    ";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@productId", productId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Basic Info
                            ProductName.Text = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : "N/A";
                            ProductCategory.Text = reader["category"] != DBNull.Value ? reader["category"].ToString() : "N/A";
                            ProductId.Text = reader["product_id"] != DBNull.Value ? reader["product_id"].ToString() : "N/A";

                            // Stock Status Logic
                            int currentStock = reader["quantity"] != DBNull.Value ? Convert.ToInt32(reader["quantity"]) : 0;
                            if (currentStock < 1)
                            {
                                StockStatus.Text = "Out of Stock";
                                StockStatusBorder.Background = new SolidColorBrush(Colors.Red);
                            }
                            else if (currentStock <= 10) 
                            {
                                StockStatus.Text = "Low Stock";
                                StockStatusBorder.Background = new SolidColorBrush(Colors.Orange);
                            }
                            else
                            {
                                StockStatus.Text = "Available";
                                StockStatusBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); 
                            }

                            ProductNameDisplay.Text = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : "N/A";
                            BatchNumberDisplay.Text = reader["batch_num"] != DBNull.Value ? reader["batch_num"].ToString() : "N/A";
                            UnitPriceDisplay.Text = reader["price"] != DBNull.Value ? $"Rs. {Convert.ToDecimal(reader["price"]):N2}" : "N/A";
                            ExpiryDateDisplay.Text = reader["expiry_date"] != DBNull.Value ? Convert.ToDateTime(reader["expiry_date"]).ToString("MMMM dd, yyyy") : "N/A";
                            StockQuantityDisplay.Text = reader["quantity"] != DBNull.Value ? $"{reader["quantity"]} Units" : "N/A";
                            SuppliersDisplay.Text = reader["supplier_name"] != DBNull.Value ? reader["supplier_name"].ToString() : "N/A";

                            ProductDescription.Text = reader["description"] != DBNull.Value ? reader["description"].ToString() : "No description available.";

                            string productImagePath = reader["product_image"] != DBNull.Value ? reader["product_image"].ToString() : string.Empty;
                            if (!string.IsNullOrEmpty(productImagePath))
                            {
                                try
                                {
                                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, productImagePath);
                                    if (File.Exists(fullPath))
                                    {
                                        BitmapImage bitmap = new BitmapImage();
                                        bitmap.BeginInit();
                                        bitmap.UriSource = new Uri(fullPath);
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        bitmap.EndInit();
                                        ProductImage.Source = bitmap;
                                    }
                                    else
                                    {
                                        ProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                                    }
                                }
                                catch (Exception)
                                {
                                    ProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                                }
                            }
                            else
                            {
                                ProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                            }
                        }
                        else
                        {
                            MessageBox.Show("Product details not found for this product ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
      
                            ProductName.Text = "Product Not Found";
                            ProductCategory.Text = "N/A";
                            ProductId.Text = "N/A";
                            StockStatus.Text = "N/A";
                            StockStatusBorder.Background = new SolidColorBrush(Colors.Gray);
                            ProductNameDisplay.Text = "N/A";
                            BatchNumberDisplay.Text = "N/A";
                            UnitPriceDisplay.Text = "N/A";
                            ExpiryDateDisplay.Text = "N/A";
                            StockQuantityDisplay.Text = "N/A";
                            SuppliersDisplay.Text = "N/A";
                            ProductDescription.Text = "N/A";
                            ProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/MedicineSample.png"));
                        }
                    }
                }
            }
            catch (MySqlException mex)
            {
                MessageBox.Show($"Database error loading product details: {mex.Message}\nError Code: {mex.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading product details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSalesHistory(int productId)
        {
            List<SalesRecord> sales = new List<SalesRecord>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT
                            s.sales_id,
                            s.sale_date,
                            s.quantity,
                            s.payment_method,
                            s.total_amount,
                            d.name AS PartnerName
                        FROM
                            sales s
                        LEFT JOIN
                            products p ON s.product_id = p.product_id
                        LEFT JOIN
                            orders o ON s.product_id = o.product_id AND s.quantity = o.quantity -- This join is highly speculative without a direct link (e.g., sales.order_id)
                        LEFT JOIN
                            distributors d ON o.dist_id = d.dist_id
                        WHERE
                            s.product_id = @productId
                        ORDER BY
                            s.sale_date DESC;
                    ";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@productId", productId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sales.Add(new SalesRecord
                            {
                                SaleID = reader.GetInt32("sales_id"),
                                SaleDate = reader.GetDateTime("sale_date"),
                                Quantity = reader.GetInt32("quantity"),
                                PaymentMethod = reader["payment_method"] != DBNull.Value ? reader.GetString("payment_method") : "N/A",
                                TotalAmount = reader.GetDecimal("total_amount"),
                                Partner = reader["PartnerName"] != DBNull.Value ? reader.GetString("PartnerName") : "Direct Sale / Unknown"
                            });
                        }
                    }
                }
                SalesDataGrid.ItemsSource = sales;
            }
            catch (MySqlException mex)
            {
                MessageBox.Show($"Database error loading sales history: {mex.Message}\nError Code: {mex.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading sales history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPurchasesHistory(int productId)
        {
            List<PurchaseRecord> purchases = new List<PurchaseRecord>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT
                            pp.purchase_id,
                            pp.date AS purchase_date,
                            pp.status,
                            pp.total_amount,
                            s.supplier_name
                        FROM
                            product_purchases pp
                        JOIN
                            suppliers s ON pp.supplier_id = s.supplier_id
                        WHERE
                            pp.product_id = @productId
                        ORDER BY
                            pp.date DESC;
                    ";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@productId", productId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            purchases.Add(new PurchaseRecord
                            {
                                PurchaseID = reader.GetInt32("purchase_id"),
                                SupplierName = reader.GetString("supplier_name"),
                                PurchaseDate = reader.GetDateTime("purchase_date"),
                                Status = reader["status"] != DBNull.Value ? reader.GetString("status") : "N/A",
                                TotalAmount = reader.GetDecimal("total_amount")
                            });
                        }
                    }
                }
                PurchasesDataGrid.ItemsSource = purchases;
            }
            catch (MySqlException mex)
            {
                MessageBox.Show($"Database error loading purchase history: {mex.Message}\nError Code: {mex.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading purchase history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrdersHistory(int productId)
        {
            List<OrderRecord> orders = new List<OrderRecord>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT
                            o.order_id,
                            o.quantity,
                            o.price,
                            o.order_date,
                            o.delivery_date,
                            o.status,
                            d.name AS DistributorName
                        FROM
                            orders o
                        LEFT JOIN
                            distributors d ON o.dist_id = d.dist_id
                        WHERE
                            o.product_id = @productId
                        ORDER BY
                            o.order_date DESC;
                    ";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@productId", productId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderRecord
                            {
                                OrderID = reader.GetInt32("order_id"),
                                Quantity = reader.GetInt32("quantity"),
                                Price = reader.GetDecimal("price"), 
                                OrderDate = reader.GetDateTime("order_date"),
                                DeliveryDate = reader["delivery_date"] != DBNull.Value ? (DateTime?)reader.GetDateTime("delivery_date") : null,
                                Status = reader["status"] != DBNull.Value ? reader.GetString("status") : "N/A",
                                DistributorName = reader["DistributorName"] != DBNull.Value ? reader.GetString("DistributorName") : "N/A"
                            });
                        }
                    }
                }
                OrdersDataGrid.ItemsSource = orders;
            }
            catch (MySqlException mex)
            {
                MessageBox.Show($"Database error loading order history: {mex.Message}\nError Code: {mex.ErrorCode}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading order history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            EditProductPopup editPopup = new EditProductPopup(_productId); 
            editPopup.Owner = this;

            bool? result = editPopup.ShowDialog();

            if (result == true)
            {
                LoadProductDetails(_productId);     
                LoadSalesHistory(_productId);       
                LoadPurchasesHistory(_productId);
                LoadOrdersHistory(_productId);
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}