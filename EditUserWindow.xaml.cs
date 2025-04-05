using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginWPF
{
    public partial class EditUserWindow : Window
    {
        public EditUserWindow()
        {
            InitializeComponent();
            InitializePasswordControls();
        }

        private void InitializePasswordControls()
        {
            chkChangePassword.Checked += (s, e) =>
            {
                txtPassword.Visibility = Visibility.Visible;
                txtConfirmPassword.Visibility = Visibility.Visible;
            };

            chkChangePassword.Unchecked += (s, e) =>
            {
                txtPassword.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
            };
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Static implementation - no actual update logic
            this.Close();
        }
    }
}