using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Net;
using System.Net.Mail;
using QuestPDF.Infrastructure;

namespace SimpleLoginWPF
{
    public partial class OrderDetails : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private int orderId;

        private InvoiceData _currentInvoiceData;

        public OrderDetails(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            QuestPDF.Settings.License = LicenseType.Community;
            LoadProducts();
            LoadOrderData();
        }

        private class ProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private class InvoiceProductItem
        {
            public string ProductName { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            public string BatchNumber { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string Category { get; set; }
            public string SupplierName { get; set; }
        }

        private class InvoiceDistributor
        {
            public string Name { get; set; }
            public string Region { get; set; }
            public string ServiceArea { get; set; }
            public string Type { get; set; }
            public string Email { get; set; }
            public List<string> PhoneNumbers { get; set; } = new List<string>();
        }

        private class InvoiceData
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime DeliveryDate { get; set; }
            public string Status { get; set; }
            public decimal OverallTotalAmount { get; set; }

            public InvoiceProductItem Product { get; set; }
            public InvoiceDistributor Distributor { get; set; }
        }

        private static class InvoiceColors
        {
            public static string PrimaryRed => "#B30000";
            public static string SecondaryGray => "#F0F0F0";
            public static string TextColor => "#333333";
        }

        //C:\\Users\\DELL\\source\\repos\\SimpleLoginWPF\\Assets\\aliflogo-removebg-preview.png

        private class OrderInvoiceDocument : IDocument
        {
            public InvoiceData Model { get; }

            public OrderInvoiceDocument(InvoiceData model)
            {
                Model = model;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container
                    .Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                        page.Header().Row(row =>
                        {
                            // Logo, adjust path or handle if missing
                            try
                            {
                                row.ConstantColumn(100).Image("C:\\Users\\DELL\\source\\repos\\SimpleLoginWPF\\Assets\\aliflogo-removebg-preview.png");
                            }
                            catch
                            {
                                // Logo missing? Just skip it
                                row.ConstantColumn(100).Text("");
                            }

                            row.RelativeColumn().Column(header =>
                            {
                                header.Item().Text("ALF PHARMACEUTICAL")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(InvoiceColors.PrimaryRed);

                                header.Item().Text("INVOICE")
                                    .FontSize(18)
                                    .SemiBold()
                                    .FontColor(InvoiceColors.PrimaryRed);

                                header.Item().PaddingBottom(10)
                                    .LineHorizontal(1)
                                    .LineColor(InvoiceColors.PrimaryRed);
                            });
                        });

                        page.Content().Column(column =>
                        {
                            column.Item().PaddingVertical(10).Column(subColumn =>
                            {
                                subColumn.Item().Row(row =>
                                {
                                    row.ConstantColumn(200).Column(infoColumn =>
                                    {
                                        infoColumn.Item().Text($"Invoice Date: {DateTime.Now:yyyy-MM-dd}").FontSize(11);
                                        infoColumn.Item().Text($"Order ID: {Model.OrderId}").FontSize(11);
                                        infoColumn.Item().Text($"Order Date: {(Model.OrderDate != default ? Model.OrderDate.ToString("yyyy-MM-dd") : "N/A")}").FontSize(11);
                                        infoColumn.Item().Text($"Delivery Date: {(Model.DeliveryDate != default ? Model.DeliveryDate.ToString("yyyy-MM-dd") : "N/A")}").FontSize(11);
                                        infoColumn.Item().PaddingBottom(5).Text($"Status: {Model.Status ?? "N/A"}").FontSize(11);
                                    });
                                    row.RelativeColumn().ExtendHorizontal();
                                });
                            });

                            column.Item().PaddingVertical(10).Column(subColumn =>
                            {
                                subColumn.Item().Text("DISTRIBUTOR DETAILS")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(InvoiceColors.PrimaryRed);

                                subColumn.Item().PaddingBottom(5)
                                    .LineHorizontal(0.5f)
                                    .LineColor(InvoiceColors.PrimaryRed);

                                subColumn.Item().Text(Model.Distributor?.Name ?? "N/A").SemiBold();

                                if (!string.IsNullOrEmpty(Model.Distributor?.Type))
                                    subColumn.Item().Text($"Type: {Model.Distributor.Type}");

                                if (!string.IsNullOrEmpty(Model.Distributor?.Email))
                                    subColumn.Item().Text($"Email: {Model.Distributor.Email}");

                                if (Model.Distributor?.PhoneNumbers != null && Model.Distributor.PhoneNumbers.Any())
                                    subColumn.Item().Text($"Phone: {string.Join(", ", Model.Distributor.PhoneNumbers)}");

                                if (!string.IsNullOrEmpty(Model.Distributor?.Region) || !string.IsNullOrEmpty(Model.Distributor?.ServiceArea))
                                    subColumn.Item().Text($"Location: {(Model.Distributor?.Region ?? "N/A")}, {(Model.Distributor?.ServiceArea ?? "N/A")}");
                            });

                            column.Item().PaddingVertical(10).Column(subColumn =>
                            {
                                subColumn.Item().Text("PRODUCT DETAILS")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(InvoiceColors.PrimaryRed);

                                subColumn.Item().PaddingBottom(5)
                                    .LineHorizontal(0.5f)
                                    .LineColor(InvoiceColors.PrimaryRed);

                                subColumn.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(1).BorderColor(InvoiceColors.PrimaryRed).PaddingBottom(5)
                                            .Text("Product Name").Bold().FontColor(InvoiceColors.PrimaryRed);
                                        header.Cell().BorderBottom(1).BorderColor(InvoiceColors.PrimaryRed).PaddingBottom(5)
                                            .AlignRight().Text("Qty").Bold().FontColor(InvoiceColors.PrimaryRed);
                                        header.Cell().BorderBottom(1).BorderColor(InvoiceColors.PrimaryRed).PaddingBottom(5)
                                            .AlignRight().Text("Total").Bold().FontColor(InvoiceColors.PrimaryRed);
                                    });

                                    table.Cell().PaddingVertical(2).Text(Model.Product?.ProductName ?? "N/A");
                                    table.Cell().PaddingVertical(2).AlignRight().Text(Model.Product?.Quantity.ToString() ?? "0");
                                    table.Cell().PaddingVertical(2).AlignRight().Text(Model.Product?.TotalPrice.ToString("N2") ?? "0.00");

                                    table.Cell().ColumnSpan(4).PaddingTop(5).Text(text =>
                                    {
                                        text.Span("Description: ").SemiBold();
                                        text.Span(Model.Product?.Description ?? "N/A").FontSize(9);
                                    });
                                    table.Cell().ColumnSpan(4).Text(text =>
                                    {
                                        text.Span("Category: ").SemiBold();
                                        text.Span(Model.Product?.Category ?? "N/A").FontSize(9);
                                    });
                                    table.Cell().ColumnSpan(4).Text(text =>
                                    {
                                        text.Span("Batch No: ").SemiBold();
                                        text.Span(Model.Product?.BatchNumber ?? "N/A").FontSize(9);
                                    });
                                    table.Cell().ColumnSpan(4).Text(text =>
                                    {
                                        text.Span("Expiry Date: ").SemiBold();
                                        text.Span(Model.Product?.ExpiryDate != default ? Model.Product.ExpiryDate.ToString("yyyy-MM-dd") : "N/A").FontSize(9);
                                    });
                                    table.Cell().ColumnSpan(4).Text(text =>
                                    {
                                        text.Span("Supplier: ").SemiBold();
                                        text.Span(Model.Product?.SupplierName ?? "N/A").FontSize(9);
                                    });
                                });
                            });

                            // Signature space at the bottom
                            column.Item().PaddingTop(30).Row(row =>
                            {
                                row.RelativeColumn().Text("Authorized Signature: ______________________").FontSize(11);
                                row.RelativeColumn().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(11).AlignRight();
                            });
                        });

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
            }
        }



        private void LoadOrderData()
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var query = @"
            SELECT
                o.order_id, o.quantity, o.price, o.status, o.order_date, o.delivery_date,
                p.product_id, p.product_name, p.description, p.category, p.batch_num, p.expiry_date,
                s.supplier_name,
                d.name AS distributor_name, d.region, d.service_area, d.distributor_type, d.email,
                GROUP_CONCAT(dp.phone SEPARATOR ', ') AS phone_numbers
            FROM orders o
            JOIN products p ON o.product_id = p.product_id
            LEFT JOIN suppliers s ON p.supplier_id = s.supplier_id
            LEFT JOIN distributors d ON o.dist_id = d.dist_id
            LEFT JOIN distributor_phones dp ON d.dist_id = dp.dist_id
            WHERE o.order_id = @orderId
            GROUP BY o.order_id";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    OrderIdText.Text = reader["order_id"].ToString();
                    decimal price = reader.GetDecimal("price");
                    int quantity = reader.GetInt32("quantity");
                    decimal totalAmount = price;

                    TotalAmountText.Text = $"Rs. {totalAmount:F2}";
                    StatusText.Text = reader["status"].ToString();

                    OrderItemsGrid.ItemsSource = new[]
                    {
                new
                {
                    ProductName = reader["product_name"].ToString(),
                    Quantity = quantity,
                    Price = price,
                    Total = totalAmount
                }
            };

                    cmbEditProducts.SelectedValue = reader.GetInt32("product_id");
                    QuantityTextBox.Text = quantity.ToString();
                    PriceTextBox.Text = price.ToString();
                    dpEditOrderDate.SelectedDate = reader.GetDateTime("order_date");
                    dpEditDeliveryDate.SelectedDate = reader.GetDateTime("delivery_date");
                    cmbEditStatus.Text = reader["status"].ToString();

                    _currentInvoiceData = new InvoiceData
                    {
                        OrderId = reader.GetInt32("order_id"),
                        OrderDate = reader.GetDateTime("order_date"),
                        DeliveryDate = reader.GetDateTime("delivery_date"),
                        Status = reader.GetString("status"),
                        OverallTotalAmount = totalAmount,
                        Product = new InvoiceProductItem
                        {
                            ProductName = reader.GetString("product_name"),
                            Description = reader["description"] as string,
                            Quantity = quantity,
                            UnitPrice = price,
                            BatchNumber = reader["batch_num"] as string,
                            ExpiryDate = reader["expiry_date"] != DBNull.Value ? reader.GetDateTime("expiry_date") : default,
                            Category = reader["category"] as string,
                            SupplierName = reader["supplier_name"] as string
                        },
                        Distributor = new InvoiceDistributor
                        {
                            Name = reader["distributor_name"] as string,
                            Region = reader["region"] as string,
                            ServiceArea = reader["service_area"] as string,
                            Type = reader["distributor_type"] as string,
                            Email = reader["email"] as string,
                            PhoneNumbers = (reader["phone_numbers"] as string)?.Split(',').Select(p => p.Trim()).ToList()
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            var productList = new List<ProductItem>();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var query = "SELECT product_id, product_name, price FROM products";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productList.Add(new ProductItem
                    {
                        Id = reader.GetInt32("product_id"),
                        Name = reader.GetString("product_name"),
                        Price = reader.GetDecimal("price")
                    });
                }

                cmbEditProducts.ItemsSource = productList;
                cmbEditProducts.DisplayMemberPath = "Name";
                cmbEditProducts.SelectedValuePath = "Id";
                cmbEditProducts.SelectionChanged += CmbEditProducts_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbEditProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEditProducts.SelectedItem is ProductItem selectedProduct)
            {
                PriceTextBox.Text = selectedProduct.Price.ToString("F2");
                CalculateTotalAmount();
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e) => CalculateTotalAmount();

        private void CalculateTotalAmount()
        {
            if (cmbEditProducts.SelectedItem is ProductItem selectedProduct &&
                int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                decimal total = selectedProduct.Price * quantity;
                PriceTextBox.Text = total.ToString("F2");
            }
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e) => EditOrderPopup.Visibility = Visibility.Visible;

        private void CloseEditPopup_Click(object sender, RoutedEventArgs e) => EditOrderPopup.Visibility = Visibility.Collapsed;

        private void SaveOrderChanges_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEditProducts.SelectedItem is not ProductItem selectedProduct ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) ||
                !decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                MessageBox.Show("Invalid input data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                var status = cmbEditStatus.Text;

                var updateQuery = @"UPDATE orders
                                    SET product_id = @productId,
                                        quantity = @quantity,
                                        price = @price,
                                        order_date = @orderDate,
                                        delivery_date = @deliveryDate,
                                        status = @status
                                    WHERE order_id = @orderId";

                using var updateCmd = new MySqlCommand(updateQuery, conn, transaction);
                updateCmd.Parameters.AddWithValue("@orderId", orderId);
                updateCmd.Parameters.AddWithValue("@productId", selectedProduct.Id);
                updateCmd.Parameters.AddWithValue("@quantity", quantity);
                updateCmd.Parameters.AddWithValue("@price", price);
                updateCmd.Parameters.AddWithValue("@orderDate", dpEditOrderDate.SelectedDate);
                updateCmd.Parameters.AddWithValue("@deliveryDate", dpEditDeliveryDate.SelectedDate);
                updateCmd.Parameters.AddWithValue("@status", status);
                updateCmd.ExecuteNonQuery();

                if (status == "Completed")
                {
                    SaveOrderToSales(orderId, selectedProduct.Id, quantity, price, dpEditDeliveryDate.SelectedDate.Value);
                }

                transaction.Commit();
                MessageBox.Show("Order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                EditOrderPopup.Visibility = Visibility.Collapsed;
                LoadOrderData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveOrderToSales(int orderId, int productId, int quantity, decimal price, DateTime deliveryDate)
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM sales WHERE order_id = @orderId", conn);
                checkCmd.Parameters.AddWithValue("@orderId", orderId);
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0) return;

                var insertCmd = new MySqlCommand(@"INSERT INTO sales (order_id, product_id, quantity, payment_method, total_amount, sale_date)
                                                 VALUES (@orderId, @productId, @quantity, 'unspecified', @totalAmount, @saleDate)", conn);
                insertCmd.Parameters.AddWithValue("@orderId", orderId);
                insertCmd.Parameters.AddWithValue("@productId", productId);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@totalAmount", price);
                insertCmd.Parameters.AddWithValue("@saleDate", deliveryDate);
                insertCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            InvoicePopup.Visibility = Visibility.Visible;
        }

        private void DownloadInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Invoice_Order_{OrderIdText.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = ".pdf",
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    if (_currentInvoiceData != null)
                    {
                        var document = new OrderInvoiceDocument(_currentInvoiceData);
                        document.GeneratePdf(filePath);
                        MessageBox.Show($"Invoice downloaded to:\n{filePath}", "Invoice Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Invoice data not available. Please load order data first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating invoice download: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CloseInvoicePopup_Click(sender, e);
        }

        private void CloseInvoicePopup_Click(object sender, RoutedEventArgs e) => InvoicePopup.Visibility = Visibility.Collapsed;
    }
}