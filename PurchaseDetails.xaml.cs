using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
            QuestPDF.Settings.License = LicenseType.Community;
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
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Purchase_Details_{PurchaseId.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = ".pdf",
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    GeneratePdfPurchaseDetails(filePath);
                    MessageBox.Show($"Purchase details PDF exported successfully to:\n{filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting purchase details: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static class InvoiceColors
        {
            public static string PrimaryRed => "#B30000";
            public static string SecondaryGray => "#F0F0F0";
            public static string TextColor => "#333333";
        }


        private void GeneratePdfPurchaseDetails(string filePath)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(QuestPDF.Helpers.Colors.Black));

                    // Header
                    page.Header().Row(row =>
                    {
                        try
                        {
                            row.ConstantColumn(100).Image("C:\\Users\\DELL\\source\\repos\\SimpleLoginWPF\\Assets\\aliflogo-removebg-preview.png");
                        }
                        catch
                        {
                            row.ConstantColumn(100).Text("");
                        }

                        row.RelativeColumn().Column(header =>
                        {
                            header.Item().Text("ALF PHARMACEUTICAL")
                                .FontSize(24)
                                .Bold()
                                .FontColor(InvoiceColors.PrimaryRed);

                            header.Item().Text("PURCHASE DETAILS")
                                .FontSize(18)
                                .SemiBold()
                                .FontColor(InvoiceColors.PrimaryRed);

                            header.Item().PaddingBottom(10)
                                .LineHorizontal(1)
                                .LineColor(InvoiceColors.PrimaryRed);
                        });
                    });

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Item().PaddingVertical(10).Column(subColumn =>
                        {
                            subColumn.Item().Text("PURCHASE OVERVIEW")
                                .FontSize(13)
                                .Bold()
                                .FontColor(InvoiceColors.PrimaryRed);

                            subColumn.Item().PaddingBottom(5)
                                .LineHorizontal(0.5f)
                                .LineColor(InvoiceColors.PrimaryRed);

                            subColumn.Item().Text($"Purchase ID:    {PurchaseId.Text ?? "N/A"}");
                            subColumn.Item().Text($"Purchase Date:  {PurchaseDate.Text ?? "N/A"}");
                            subColumn.Item().Text($"Total Amount:   {TotalAmount.Text ?? "N/A"}");
                            subColumn.Item().Text($"Status:         {PurchaseStatus.Text ?? "N/A"}");
                        });

                        column.Item().PaddingVertical(10).Column(subColumn =>
                        {
                            subColumn.Item().Text("SUPPLIER INFORMATION")
                                .FontSize(13)
                                .Bold()
                                .FontColor(InvoiceColors.PrimaryRed);

                            subColumn.Item().PaddingBottom(5)
                                .LineHorizontal(0.5f)
                                .LineColor(InvoiceColors.PrimaryRed);

                            subColumn.Item().Text($"Supplier Name:    {SupplierName.Text ?? "N/A"}");
                            subColumn.Item().Text($"Contact Person:   {SupplierContactName.Text ?? "N/A"}");
                            subColumn.Item().Text($"Company Name:     {SupplierCompanyName.Text ?? "N/A"}");
                            subColumn.Item().Text($"Email:            {SupplierEmail.Text ?? "N/A"}");
                            subColumn.Item().Text($"Phone:            {SupplierPhone.Text ?? "N/A"}");
                        });

                        column.Item().PaddingVertical(10).Column(subColumn =>
                        {
                            subColumn.Item().Text("PRODUCT INFORMATION")
                                .FontSize(13)
                                .Bold()
                                .FontColor(InvoiceColors.PrimaryRed);

                            subColumn.Item().PaddingBottom(5)
                                .LineHorizontal(0.5f)
                                .LineColor(InvoiceColors.PrimaryRed);

                            subColumn.Item().Text($"Product Name:       {ProductNameText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Category:           {ProductCategoryText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Unit Price:         {ProductUnitPriceText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Purchased Quantity: {ProductPurchasedQuantityText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Stock Quantity:     {ProductStockQuantityText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Batch No.:          {ProductBatchNumText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Expiry Date:        {ProductExpiryDateText.Text ?? "N/A"}");
                            subColumn.Item().Text($"Description:        {ProductDescriptionText.Text ?? "N/A"}");
                        });

                        // Signature space
                        column.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeColumn().Text("Authorized Signature: ______________________");
                            row.RelativeColumn().Text($"Date: {DateTime.Now:yyyy-MM-dd}").AlignRight();
                        });
                    });

                    // Footer with page numbers
                    page.Footer()
                        .AlignRight()
                        .Text(x =>
                        {
                            x.Span("Page ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" of ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                });
            });

            document.GeneratePdf(filePath);
        }



        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            EditPurchaseDialog editPurchaseDialog = new EditPurchaseDialog(this);

            bool? dialogResult = editPurchaseDialog.ShowDialog();

            if (dialogResult == true)
            {
                LoadPurchaseData();
            }
        }

    }
}
