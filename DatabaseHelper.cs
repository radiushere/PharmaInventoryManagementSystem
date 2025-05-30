using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

public class DatabaseHelper
{
    private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

    public static bool RegisterUser(string username, string password, string role)
    {
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO users (username, password, role) VALUES (@username, @password, @role)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@role", role);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Registration error: " + ex.Message);
            return false;
        }
    }

    public static string AuthenticateUser(string username, string password)
    {
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, role FROM users WHERE username = @username AND password = @password";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserSession.UserID = reader.GetInt32("id");
                            UserSession.Username = username;
                            UserSession.Role = reader.GetString("role");
                            return UserSession.Role;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Database error: " + ex.Message);
        }

        return null;
    }
}
