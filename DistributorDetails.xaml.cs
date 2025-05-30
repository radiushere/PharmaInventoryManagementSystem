using System.Windows;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace SimpleLoginWPF
{
    public partial class DistributorDetails : Window
    {
        private int _currentDistributorId;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public DistributorDetails(int distributorId)
        {
            InitializeComponent();
            _currentDistributorId = distributorId;
            LoadDistributorDetails(distributorId);
        }

        public void LoadDistributorDetails(int distributorId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Fetch distributor details
                    var distributorQuery = @"SELECT name, region, distributor_type, email, service_area
                                            FROM distributors
                                            WHERE dist_id = @distributorId";

                    using (var distributorCmd = new MySqlCommand(distributorQuery, conn))
                    {
                        distributorCmd.Parameters.AddWithValue("@distributorId", distributorId);

                        using (var reader = distributorCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DistributorName.Text = reader["name"].ToString();
                                DistributorType.Text = reader["distributor_type"].ToString();
                                PrimaryRegion.Text = reader["region"].ToString();
                                ServiceAreas.Text = reader["service_area"].ToString();
                                ContactEmail.Text = reader["email"].ToString();
                            }
                        }
                    }

                    // Fetch distributor phone numbers
                    var phonesQuery = @"SELECT phone
                                       FROM distributor_phones
                                       WHERE dist_id = @distributorId";

                    using (var phonesCmd = new MySqlCommand(phonesQuery, conn))
                    {
                        phonesCmd.Parameters.AddWithValue("@distributorId", distributorId);

                        using (var reader = phonesCmd.ExecuteReader())
                        {
                            var phoneNumbers = new System.Collections.Generic.List<string>();
                            while (reader.Read())
                            {
                                phoneNumbers.Add(reader["phone"].ToString());
                            }
                            ContactPhone.Text = string.Join(", ", phoneNumbers);
                        }
                    }

                    // Fetch orders for the distributor
                    var ordersQuery = @"SELECT order_id AS OrderId, product_id AS ProductId, quantity AS Quantity,
                                              price AS Price, order_date AS OrderDate, delivery_date AS DeliveryDate,
                                              status AS Status
                                       FROM orders
                                       WHERE dist_id = @distributorId";

                    using (var ordersCmd = new MySqlCommand(ordersQuery, conn))
                    {
                        ordersCmd.Parameters.AddWithValue("@distributorId", distributorId);

                        var ordersAdapter = new MySqlDataAdapter(ordersCmd);
                        var ordersTable = new DataTable();
                        ordersAdapter.Fill(ordersTable);
                        OrdersDataGrid.ItemsSource = ordersTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading distributor details: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditDistributor_Click(object sender, RoutedEventArgs e)
        {
            EditDistributorPopup editPopup = new EditDistributorPopup(this, _currentDistributorId);
            editPopup.Owner = this;
            editPopup.ShowDialog();
        }

        private void ExitPopup_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
