using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using By_ProV2.DataAccess;
using By_ProV2.Models;

namespace By_ProV2
{
    public partial class SutEnvanteriWindow : Window
    {
        private readonly SutEnvanteriRepository _repo = new SutEnvanteriRepository();
        public ObservableCollection<SutEnvanteri> EnvanterListesi { get; set; }
        private bool _isInitializing = false; // Flag to prevent auto-actions during initialization

        public SutEnvanteriWindow()
        {
            InitializeComponent();
            EnvanterListesi = new ObservableCollection<SutEnvanteri>();
            dgEnvanter.ItemsSource = EnvanterListesi;
            
            // Set initialization flag to prevent auto-actions during setup
            _isInitializing = true;
            
            // Set default date to today
            dpTarih.SelectedDate = DateTime.Today;
            
            // Reset the flag after initialization
            _isInitializing = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            YukleEnvanterleri();
        }

        private void YukleEnvanterleri()
        {
            try
            {
                var envanterler = _repo.GetAllEnvanter();
                EnvanterListesi.Clear();
                
                foreach (var envanter in envanterler)
                {
                    EnvanterListesi.Add(envanter);
                }
                
                GuncelleOzet();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Envanter verileri yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dpTarih.SelectedDate.HasValue)
                {
                    MessageBox.Show("Lütfen bir tarih seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existingEnvanter = _repo.GetEnvanterByTarih(dpTarih.SelectedDate.Value);
                if (existingEnvanter != null)
                {
                    MessageBox.Show("Bu tarih için zaten envanter kaydı mevcut. Lütfen Güncelle butonunu kullanın veya farklı bir tarih seçin.", 
                                    "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal devirSut = ParseDecimal(txtDevirSut.Text);
                decimal gunlukAlinanSut = ParseDecimal(txtGunlukAlinanSut.Text);
                decimal gunlukSatilanSut = ParseDecimal(txtGunlukSatilanSut.Text);

                // Calculate remaining milk: Devir + Alınan - Satılan
                decimal kalanSut = devirSut + gunlukAlinanSut - gunlukSatilanSut;

                var yeniEnvanter = new SutEnvanteri
                {
                    Tarih = dpTarih.SelectedDate.Value,
                    DevirSut = devirSut,
                    GunlukAlinanSut = gunlukAlinanSut,
                    GunlukSatilanSut = gunlukSatilanSut,
                    KalanSut = kalanSut,
                    CreatedBy = App.AuthService?.CurrentUser?.Id
                };

                _repo.KaydetEnvanter(yeniEnvanter);
                EnvanterListesi.Add(yeniEnvanter);
                
                GuncelleOzet();
                
                MessageBox.Show("Envanter kaydı başarıyla eklendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                TemizleAlanlari();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var seciliEnvanter = dgEnvanter.SelectedItem as SutEnvanteri;
                if (seciliEnvanter == null)
                {
                    MessageBox.Show("Lütfen güncellenecek bir envanter kaydı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpTarih.SelectedDate.HasValue)
                {
                    MessageBox.Show("Lütfen bir tarih seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal devirSut = ParseDecimal(txtDevirSut.Text);
                decimal gunlukAlinanSut = ParseDecimal(txtGunlukAlinanSut.Text);
                decimal gunlukSatilanSut = ParseDecimal(txtGunlukSatilanSut.Text);

                // Calculate remaining milk: Devir + Alınan - Satılan
                decimal kalanSut = devirSut + gunlukAlinanSut - gunlukSatilanSut;

                seciliEnvanter.Tarih = dpTarih.SelectedDate.Value;
                seciliEnvanter.DevirSut = devirSut;
                seciliEnvanter.GunlukAlinanSut = gunlukAlinanSut;
                seciliEnvanter.GunlukSatilanSut = gunlukSatilanSut;
                seciliEnvanter.KalanSut = kalanSut;
                seciliEnvanter.ModifiedBy = App.AuthService?.CurrentUser?.Id;
                seciliEnvanter.ModifiedAt = DateTime.Now;

                _repo.GuncelleEnvanter(seciliEnvanter);
                
                dgEnvanter.Items.Refresh();
                GuncelleOzet();
                
                MessageBox.Show("Envanter kaydı başarıyla güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var seciliEnvanter = dgEnvanter.SelectedItem as SutEnvanteri;
                if (seciliEnvanter == null)
                {
                    MessageBox.Show("Lütfen silinecek bir envanter kaydı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Seçili envanter kaydını silmek istediğinize emin misiniz?\nTarih: {seciliEnvanter.Tarih:dd.MM.yyyy}", 
                                            "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;

                _repo.SilEnvanter(seciliEnvanter.Id);
                EnvanterListesi.Remove(seciliEnvanter);
                
                GuncelleOzet();
                
                MessageBox.Show("Envanter kaydı başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                TemizleAlanlari();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnYeni_Click(object sender, RoutedEventArgs e)
        {
            TemizleAlanlari();
            dpTarih.SelectedDate = DateTime.Today;
        }

        private void btnYukle_Click(object sender, RoutedEventArgs e)
        {
            YukleEnvanterleri();
        }

        private void btnKapat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TemizleAlanlari()
        {
            txtDevirSut.Clear();
            txtGunlukAlinanSut.Clear();
            txtGunlukSatilanSut.Clear();
            txtKalanSut.Clear();
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal result))
            {
                return result;
            }
            
            return 0;
        }

        private void GuncelleKalanSut()
        {
            decimal devirSut = ParseDecimal(txtDevirSut.Text);
            decimal gunlukAlinanSut = ParseDecimal(txtGunlukAlinanSut.Text);
            decimal gunlukSatilanSut = ParseDecimal(txtGunlukSatilanSut.Text);

            decimal kalanSut = devirSut + gunlukAlinanSut - gunlukSatilanSut;
            txtKalanSut.Text = kalanSut.ToString("N2");
        }

        private void GuncelleOzet()
        {
            if (EnvanterListesi.Count == 0)
            {
                lblToplamAlinan.Text = "0.00";
                lblToplamSatilan.Text = "0.00";
                lblSonKalan.Text = "0.00";
                return;
            }

            decimal toplamAlinan = 0;
            decimal toplamSatilan = 0;

            foreach (var envanter in EnvanterListesi)
            {
                toplamAlinan += envanter.GunlukAlinanSut;
                toplamSatilan += envanter.GunlukSatilanSut;
            }

            lblToplamAlinan.Text = toplamAlinan.ToString("N2");
            lblToplamSatilan.Text = toplamSatilan.ToString("N2");

            // Get the last entry's remaining milk
            var sonEnvanter = EnvanterListesi[0]; // Since it's ordered by date descending
            lblSonKalan.Text = sonEnvanter.KalanSut.ToString("N2");
        }

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input including decimal point
            Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).CaretIndex, e.Text));
        }

        private void txtDevirSut_TextChanged(object sender, TextChangedEventArgs e)
        {
            GuncelleKalanSut();
        }

        private void txtGunlukAlinanSut_TextChanged(object sender, TextChangedEventArgs e)
        {
            GuncelleKalanSut();
        }

        private void txtGunlukSatilanSut_TextChanged(object sender, TextChangedEventArgs e)
        {
            GuncelleKalanSut();
        }

        private void dgEnvanter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seciliEnvanter = dgEnvanter.SelectedItem as SutEnvanteri;
            if (seciliEnvanter != null)
            {
                dpTarih.SelectedDate = seciliEnvanter.Tarih;
                txtDevirSut.Text = seciliEnvanter.DevirSut.ToString("N2");
                txtGunlukAlinanSut.Text = seciliEnvanter.GunlukAlinanSut.ToString("N2");
                txtGunlukSatilanSut.Text = seciliEnvanter.GunlukSatilanSut.ToString("N2");
                txtKalanSut.Text = seciliEnvanter.KalanSut.ToString("N2");
            }
        }

        private void dpTarih_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
            {
                // Don't perform auto-actions during initialization
                return;
            }

            if (dpTarih.SelectedDate.HasValue)
            {
                // Check if there's already an inventory for this date
                var existingEnvanter = _repo.GetEnvanterByTarih(dpTarih.SelectedDate.Value);
                if (existingEnvanter != null)
                {
                    // Don't auto-populate if there's already a record for this date
                    return;
                }
                
                // Only auto-populate Devir Süt from previous day if the selected date is today or in the future
                DateTime selectedDate = dpTarih.SelectedDate.Value.Date;
                DateTime today = DateTime.Today;
                
                if (selectedDate >= today)
                {
                    // Auto-populate Devir Süt from previous day's Kalan Sut
                    DateTime previousDay = selectedDate.AddDays(-1);
                    var previousDayEnvanter = _repo.GetEnvanterByTarih(previousDay);
                    
                    if (previousDayEnvanter != null)
                    {
                        txtDevirSut.Text = previousDayEnvanter.KalanSut.ToString("N2");
                    }
                    else
                    {
                        // If no previous day record exists, set to 0
                        txtDevirSut.Text = "0.00";
                    }
                    
                    // Clear daily transaction fields since they're for a different day
                    txtGunlukAlinanSut.Text = "0.00";
                    txtGunlukSatilanSut.Text = "0.00";
                    GuncelleKalanSut(); // Update the calculated remaining amount
                }
                // For past dates, don't auto-populate Devir Süt, let user enter manually
            }
        }
    }
}