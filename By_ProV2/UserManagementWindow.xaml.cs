using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using By_ProV2.DataAccess;
using By_ProV2.Models;

namespace By_ProV2
{
    public partial class UserManagementWindow : Window
    {
        private readonly UserRepository _userRepository;

        public UserManagementWindow()
        {
            InitializeComponent();
            
            _userRepository = new UserRepository();
            
            if (App.AuthService.CurrentUser?.Role != "Admin")
            {
                // Disable editing functionality
                btnAddUser.IsEnabled = false;
                MessageBox.Show("Yalnızca admin kullanıcılar bu işlemi yapabilir!", 
                                "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            LoadUsers();
        }
        
        private void LoadUsers()
        {
            try
            {
                var users = _userRepository.GetAllUsers();
                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcılar yüklenirken hata oluştu: {ex.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (App.AuthService.CurrentUser?.Role != "Admin")
            {
                MessageBox.Show("Yalnızca admin kullanıcılar yeni kullanıcı oluşturabilir!", 
                                "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            string username = txtNewUsername.Text.Trim();
            string fullName = txtNewFullName.Text.Trim();
            string email = txtNewEmail.Text.Trim();
            string role = (cmbNewRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "User";
            string password = txtNewPassword.Password;
            string confirmPassword = txtNewPasswordConfirm.Password;

            if (string.IsNullOrEmpty(username) || 
                string.IsNullOrEmpty(fullName) || 
                string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Lütfen tüm zorunlu alanları doldurun.", 
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Şifreler eşleşmiyor.", 
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Şifre en az 6 karakter uzunluğunda olmalıdır.", 
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = App.AuthService.RegisterUser(username, password, fullName, email, role);
                
                if (success)
                {
                    MessageBox.Show("Kullanıcı başarıyla oluşturuldu.", 
                                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Clear the form
                    txtNewUsername.Clear();
                    txtNewFullName.Clear();
                    txtNewEmail.Clear();
                    txtNewPassword.Clear();
                    txtNewPasswordConfirm.Clear();
                    cmbNewRole.SelectedIndex = 0;
                    
                    LoadUsers(); // Refresh the list
                }
                else
                {
                    MessageBox.Show("Kullanıcı oluşturulamadı. Kullanıcı adı zaten mevcut olabilir.", 
                                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcı oluşturulurken hata oluştu: {ex.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}