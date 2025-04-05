using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleLoginWPF
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        public UserProfile()
        {
            InitializeComponent();
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            // This would typically come from a database or user session
            // For now, we'll use static data

            // Basic user details
            UserNameText.Text = "John Smith";
            UserRoleText.Text = "Inventory Manager";
            UserEmailText.Text = "john.smith@alif.com";
            UserIdText.Text = "EMP-1234";
            DepartmentText.Text = "Supply Chain";
            PhoneText.Text = "+1-555-123-4567";
            JoinDateText.Text = "January 15, 2023";

            try
            {
                // Try to load the profile image
                // In a real application, this would come from a database or file system
                // For demo purposes, using a default image from Assets folder
                BitmapImage profileBitmap = new BitmapImage(new Uri("/Assets/default-profile.png", UriKind.Relative));
                ProfileImage.Source = profileBitmap;
            }
            catch (Exception ex)
            {
                // If image fails to load, just log it - don't crash the app
                Console.WriteLine($"Error loading profile image: {ex.Message}");
                // Could set a default placeholder image here
            }

            // If you had a list of notifications from a database, you would add them here
            // We already have static notifications in the XAML
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Profile editing functionality would be implemented here.",
                "Edit Profile", MessageBoxButton.OK, MessageBoxImage.Information);

            // In a real application, you would open a form to edit profile details
            // For this demo, we'll just show a message
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}