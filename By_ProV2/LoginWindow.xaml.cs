using System.Windows;

namespace By_ProV2
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Lütfen kullanıcı adı ve şifre girin.");
                return;
            }

            try
            {
                var authService = App.AuthService;
                if (authService.Login(username, password))
                {
                    MessageBox.Show($"{authService.CurrentUser.FullName ?? username} olarak giriş yapıldı.", 
                                    "Giriş Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Geçersiz kullanıcı adı veya şifre.");
                    // Also show a messagebox to make it more obvious
                    MessageBox.Show("Geçersiz kullanıcı adı veya şifre. Lütfen bilgilerinizi kontrol edin.", 
                                    "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Giriş yapılırken bir hata oluştu: {ex.Message}");
                MessageBox.Show($"Giriş yapılırken bir hata oluştu: {ex.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }

        private void TxtUsername_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // Move focus to password field when Enter is pressed in username
                txtPassword.Focus();
            }
        }

        private void TxtPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // Trigger login when Enter is pressed in password field
                btnLogin_Click(sender, new RoutedEventArgs());
            }
        }
    }
}