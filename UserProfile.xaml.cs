using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleLoginWPF
{
    public partial class UserProfile : Window
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public UserProfile()
        {
            InitializeComponent();
            LoadUserProfile();
            CheckAndCreateNotifications();
        }

        private void LoadUserProfile()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Fetch user details using UserSession.UserID
                    var userQuery = @"SELECT id, username, email, CONCAT(fname, ' ', lname) AS full_name, role, account_status, created_at
                                      FROM users
                                      WHERE id = @userId";

                    using (var userCmd = new MySqlCommand(userQuery, conn))
                    {
                        userCmd.Parameters.AddWithValue("@userId", UserSession.UserID);

                        using (var reader = userCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserIdText.Text = reader["id"].ToString();
                                UsernameText.Text = reader["username"].ToString(); // Ensure this line correctly sets the username
                                FullNameText.Text = reader["full_name"].ToString();
                                UserEmailText.Text = reader["email"].ToString();
                                UserRoleText.Text = reader["role"].ToString();
                                AccountStatusText.Text = reader["account_status"].ToString();
                                JoinDateText.Text = ((DateTime)reader["created_at"]).ToString("MMMM dd, yyyy");
                            }
                        }
                    }

                    // Fetch active notifications
                    var notificationsQuery = @"SELECT message, date
                                                FROM notifications
                                                WHERE status = 'Active'
                                                ORDER BY date DESC";

                    using (var notificationsCmd = new MySqlCommand(notificationsQuery, conn))
                    {
                        using (var reader = notificationsCmd.ExecuteReader())
                        {
                            NotificationsPanel.Children.Clear(); // Clear existing notifications

                            while (reader.Read())
                            {
                                var message = reader["message"].ToString();
                                var date = ((DateTime)reader["date"]).ToString("MMMM dd");

                                var notificationBorder = new Border
                                {
                                    Style = FindResource("NotificationItem") as Style
                                };

                                var notificationGrid = new Grid
                                {
                                    ColumnDefinitions =
                                    {
                                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                        new ColumnDefinition { Width = GridLength.Auto }
                                    }
                                };

                                var messageTextBlock = new TextBlock
                                {
                                    Text = message,
                                    VerticalAlignment = VerticalAlignment.Center
                                };

                                var dateTextBlock = new TextBlock
                                {
                                    Text = date,
                                    FontSize = 12,
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909090")),
                                    VerticalAlignment = VerticalAlignment.Center
                                };

                                notificationGrid.Children.Add(messageTextBlock);
                                notificationGrid.Children.Add(dateTextBlock);
                                Grid.SetColumn(dateTextBlock, 1);

                                notificationBorder.Child = notificationGrid;
                                NotificationsPanel.Children.Add(notificationBorder);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user profile: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckAndCreateNotifications()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Low quantity products
                    var lowQuantityQuery = @"SELECT product_id, product_name, quantity FROM products WHERE quantity < 5";
                    var lowStockList = new List<(int productId, string productName, int quantity)>();

                    using (var cmd = new MySqlCommand(lowQuantityQuery, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lowStockList.Add((
                                Convert.ToInt32(reader["product_id"]),
                                reader["product_name"].ToString(),
                                Convert.ToInt32(reader["quantity"])
                            ));
                        }
                    }

                    foreach (var item in lowStockList)
                    {
                        var checkQuery = @"SELECT COUNT(*) FROM notifications
                                   WHERE product_id = @productId AND message LIKE 'Low quantity for product%'";
                        using (var checkCmd = new MySqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@productId", item.productId);
                            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count == 0)
                            {
                                var insertQuery = @"INSERT INTO notifications (product_id, date, message, status)
                                            VALUES (@productId, @date, @message, 'Active')";
                                using (var insertCmd = new MySqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@productId", item.productId);
                                    insertCmd.Parameters.AddWithValue("@date", DateTime.Today);
                                    insertCmd.Parameters.AddWithValue("@message", $"Low quantity for product: {item.productName} (Quantity: {item.quantity})");
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // 2. Completed orders
                    var completedOrdersQuery = @"SELECT order_id, product_id FROM orders WHERE status = 'Completed'";
                    var completedOrderList = new List<int>();

                    using (var cmd = new MySqlCommand(completedOrdersQuery, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            completedOrderList.Add(Convert.ToInt32(reader["product_id"]));
                        }
                    }

                    foreach (var productId in completedOrderList)
                    {
                        var checkQuery = @"SELECT COUNT(*) FROM notifications
                                   WHERE product_id = @productId AND message LIKE 'Order completed for product%'";
                        using (var checkCmd = new MySqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@productId", productId);
                            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count == 0)
                            {
                                var insertQuery = @"INSERT INTO notifications (product_id, date, message, status)
                                            VALUES (@productId, @date, @message, 'Active')";
                                using (var insertCmd = new MySqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@productId", productId);
                                    insertCmd.Parameters.AddWithValue("@date", DateTime.Today);
                                    insertCmd.Parameters.AddWithValue("@message", $"Order completed for product: {productId}");
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // 3. Completed purchases
                    var completedPurchasesQuery = @"SELECT purchase_id, product_id FROM product_purchases WHERE status = 'Completed'";
                    var completedPurchaseList = new List<int>();

                    using (var cmd = new MySqlCommand(completedPurchasesQuery, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            completedPurchaseList.Add(Convert.ToInt32(reader["product_id"]));
                        }
                    }

                    foreach (var productId in completedPurchaseList)
                    {
                        var checkQuery = @"SELECT COUNT(*) FROM notifications
                                   WHERE product_id = @productId AND message LIKE 'Purchase completed for product%'";
                        using (var checkCmd = new MySqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@productId", productId);
                            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count == 0)
                            {
                                var insertQuery = @"INSERT INTO notifications (product_id, date, message, status)
                                            VALUES (@productId, @date, @message, 'Active')";
                                using (var insertCmd = new MySqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@productId", productId);
                                    insertCmd.Parameters.AddWithValue("@date", DateTime.Today);
                                    insertCmd.Parameters.AddWithValue("@message", $"Purchase completed for product: {productId}");
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking and creating notifications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Buttton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
