using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Media;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using Microsoft.Win32;

namespace SimpleLoginWPF
{
    public partial class PurchaseDetails : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private int purchaseId;

        public PurchaseDetails(int purchaseId)
        {
            InitializeComponent();
            this.purchaseId = purchaseId;
            LoadPurchaseData();
        }

        private void LoadPurchaseData()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    var purchaseQuery = @"SELECT
                                            pp.purchase_id,
                                            s.supplier_name,
                                            s.contact_name,
                                            s.email AS supplier_email,
                                            s.phone AS supplier_phone,
                                            s.company_name AS supplier_company_name,
                                            s.address AS supplier_address,
                                            pp.date,
                                            pp.status,
                                            pp.total_amount,
                                            pp.product_id
                                          FROM product_purchases pp
                                          JOIN suppliers s ON pp.supplier_id = s.supplier_id
                                          WHERE pp.purchase_id = @purchaseId";

                    int? productIdFromPurchase = null;

                    using (var purchaseCmd = new MySqlCommand(purchaseQuery, conn))
                    {
                        purchaseCmd.Parameters.AddWithValue("@purchaseId", this.purchaseId);

                        using (var reader = purchaseCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PurchaseId.Text = reader["purchase_id"]?.ToString() ?? "N/A";
                                PurchaseDate.Text = reader["date"] is DateTime dt ? dt.ToString("yyyy-MM-dd") : "N/A";

                                string status = reader["status"]?.ToString() ?? "N/A";
                                PurchaseStatus.Text = status;
                                SetPurchaseStatusColor(status);

                                decimal totalAmountValue = reader["total_amount"] != DBNull.Value ? Convert.ToDecimal(reader["total_amount"]) : 0;
                                TotalAmount.Text = $"Rs {totalAmountValue:F2}";

                                SupplierName.Text = reader["supplier_name"]?.ToString() ?? "N/A";
                                SupplierContactName.Text = reader["contact_name"]?.ToString() ?? "N/A";
                                SupplierEmail.Text = reader["supplier_email"]?.ToString() ?? "N/A";
                                SupplierPhone.Text = reader["supplier_phone"]?.ToString() ?? "N/A";
                                SupplierCompanyName.Text = reader["supplier_company_name"]?.ToString() ?? "N/A";

                                if (reader["product_id"] != DBNull.Value)
                                {
                                    productIdFromPurchase = Convert.ToInt32(reader["product_id"]);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Purchase not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                this.Close();
                                return;
                            }
                        }
                    }

                    if (productIdFromPurchase.HasValue)
                    {
                        var productDetailQuery = @"SELECT
                                                    p.product_name,
                                                    p.description,
                                                    p.price AS unit_price,
                                                    p.batch_num,
                                                    p.expiry_date,
                                                    p.quantity AS stock_quantity,
                                                    p.product_image,
                                                    p.category,
                                                    pp.total_amount
                                                  FROM products p
                                                  JOIN product_purchases pp ON p.product_id = pp.product_id
                                                  WHERE pp.purchase_id = @purchaseId AND p.product_id = @productId";

                        using (var productCmd = new MySqlCommand(productDetailQuery, conn))
                        {
                            productCmd.Parameters.AddWithValue("@purchaseId", this.purchaseId);
                            productCmd.Parameters.AddWithValue("@productId", productIdFromPurchase.Value);

                            using (var productReader = productCmd.ExecuteReader())
                            {
                                if (productReader.Read())
                                {
                                    decimal unitPrice = productReader["unit_price"] != DBNull.Value ? Convert.ToDecimal(productReader["unit_price"]) : 0;
                                    decimal totalPurchaseAmount = productReader["total_amount"] != DBNull.Value ? Convert.ToDecimal(productReader["total_amount"]) : 0;
                                    decimal calculatedQuantity = 0;

                                    if (unitPrice > 0)
                                    {
                                        calculatedQuantity = totalPurchaseAmount / unitPrice;
                                    }

                                    ProductNameText.Text = productReader["product_name"]?.ToString() ?? "N/A";
                                    ProductDescriptionText.Text = productReader["description"]?.ToString() ?? "N/A";
                                    ProductUnitPriceText.Text = $"Rs {unitPrice:F2}";
                                    ProductBatchNumText.Text = productReader["batch_num"]?.ToString() ?? "N/A";
                                    ProductCategoryText.Text = productReader["category"]?.ToString() ?? "N/A";
                                    ProductPurchasedQuantityText.Text = calculatedQuantity.ToString("0.##");
                                    ProductStockQuantityText.Text = productReader["stock_quantity"]?.ToString() ?? "N/A";

                                    if (productReader["expiry_date"] != DBNull.Value)
                                    {
                                        ProductExpiryDateText.Text = ((DateTime)productReader["expiry_date"]).ToString("yyyy-MM-dd");
                                    }
                                    else
                                    {
                                        ProductExpiryDateText.Text = "N/A";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ProductNameText.Text = "N/A";
                        ProductDescriptionText.Text = "N/A";
                        ProductUnitPriceText.Text = "N/A";
                        ProductBatchNumText.Text = "N/A";
                        ProductExpiryDateText.Text = "N/A";
                        ProductCategoryText.Text = "N/A";
                        ProductPurchasedQuantityText.Text = "N/A";
                        ProductStockQuantityText.Text = "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetPurchaseStatusColor(string status)
        {
            switch (status)
            {
                case "Pending":
                    PurchaseStatus.Foreground = Brushes.Orange;
                    break;
                case "Completed":
                    PurchaseStatus.Foreground = Brushes.Green;
                    break;
                case "Cancelled":
                    PurchaseStatus.Foreground = Brushes.Red;
                    break;
                default:
                    PurchaseStatus.Foreground = Brushes.Black;
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PrintDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use SaveFileDialog to let the user choose the save location
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Purchase_Details_{PurchaseId.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    DefaultExt = ".txt",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    GenerateTextPurchaseDetails(filePath); // Call the new text generation method
                    MessageBox.Show($"Purchase details exported successfully to:\n{filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting purchase details: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // If you have a popup to close after printing, call that method here.
            // Example: ClosePurchaseDetailsPopup();
        }

        // --- New Simplified Text Purchase Details Generation Logic ---
        private void GenerateTextPurchaseDetails(string filePath)
        {
            try
            {
                StringBuilder content = new StringBuilder();

                content.AppendLine("------------------------------------");
                content.AppendLine("         PURCHASE DETAILS         ");
                content.AppendLine("------------------------------------");
                content.AppendLine($"Generated On: {DateTime.Now}");
                content.AppendLine();

                // Purchase Overview
                content.AppendLine("PURCHASE OVERVIEW:");
                content.AppendLine("--------------------");
                content.AppendLine($"Purchase ID:    {PurchaseId.Text}");
                content.AppendLine($"Purchase Date:  {PurchaseDate.Text}");
                content.AppendLine($"Total Amount:   {TotalAmount.Text}");
                content.AppendLine($"Status:         {PurchaseStatus.Text}");
                content.AppendLine();

                // Supplier Information
                content.AppendLine("SUPPLIER INFORMATION:");
                content.AppendLine("-----------------------");
                content.AppendLine($"Supplier Name:    {SupplierName.Text}");
                content.AppendLine($"Contact Person:   {SupplierContactName.Text}");
                content.AppendLine($"Company Name:     {SupplierCompanyName.Text}");
                content.AppendLine($"Email:            {SupplierEmail.Text}");
                content.AppendLine($"Phone:            {SupplierPhone.Text}");
                content.AppendLine();

                // Product Information
                content.AppendLine("PRODUCT INFORMATION:");
                content.AppendLine("----------------------");
                content.AppendLine($"Product Name:       {ProductNameText.Text}");
                content.AppendLine($"Category:           {ProductCategoryText.Text}");
                content.AppendLine($"Unit Price:         {ProductUnitPriceText.Text}");
                content.AppendLine($"Purchased Quantity: {ProductPurchasedQuantityText.Text}");
                content.AppendLine($"Stock Quantity:     {ProductStockQuantityText.Text}");
                content.AppendLine($"Batch No.:          {ProductBatchNumText.Text}");
                content.AppendLine($"Expiry Date:        {ProductExpiryDateText.Text}");
                content.AppendLine($"Description:        {ProductDescriptionText.Text}");
                content.AppendLine();
                content.AppendLine("------------------------------------");

                // Write the content to the specified file
                File.WriteAllText(filePath, content.ToString());
            }
            catch (Exception ex)
            {
                // Rethrow the exception to be caught by the PrintDetails_Click handler
                throw new Exception($"Failed to generate purchase details content: {ex.Message}", ex);
            }
        }


        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the EditPurchaseDialog and pass the current window as the parent
            EditPurchaseDialog editPurchaseDialog = new EditPurchaseDialog(this);

            // Show the dialog
            bool? dialogResult = editPurchaseDialog.ShowDialog();

            // If the dialog was closed with a "Save" result, refresh the purchase data
            if (dialogResult == true)
            {
                LoadPurchaseData(); // Refresh the purchase data
            }
        }

    }
}
