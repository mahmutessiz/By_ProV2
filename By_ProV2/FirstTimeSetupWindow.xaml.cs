using System.Windows;

namespace By_ProV2
{
    public partial class FirstTimeSetupWindow : Window
    {
        public FirstTimeSetupWindow()
        {
            InitializeComponent();
        }

        private void btnCreateAdmin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtPasswordConfirm.Password;

            if (string.IsNullOrEmpty(username) || 
                string.IsNullOrEmpty(fullName) || 
                string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Lütfen tüm alanları doldurun.");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Şifreler eşleşmiyor.");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Şifre en az 6 karakter uzunluğunda olmalıdır.");
                return;
            }

            var authService = App.AuthService;
            bool success = authService.RegisterUser(username, password, fullName, email, "Admin");
            
            if (success)
            {
                MessageBox.Show("Admin hesabı başarıyla oluşturuldu. Lütfen giriş yapın.", 
                                "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Hesap oluşturulamadı. Kullanıcı adı zaten mevcut olabilir.");
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
    }
}