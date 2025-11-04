using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System;
using System.Globalization;
using By_ProV2.DataAccess;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2
{
    public partial class SutAlimFormu : Window
    {
        public ObservableCollection<SutKaydi> TedarikciListesi { get; set; }
        private readonly SutRepository _repo = new SutRepository();
        private SutKaydi currentEditRecord = null; // For single-record edit mode
        private bool isDocumentViewMode = false; // For multi-record document view/edit mode
        private ObservableCollection<SutKaydi> _deletedRecords = new ObservableCollection<SutKaydi>(); // Track deleted records for database deletion

        public SutAlimFormu()
        {
            InitializeComponent();
            TedarikciListesi = new ObservableCollection<SutKaydi>();
            dgTedarikciler.ItemsSource = TedarikciListesi;
        }

        // Mode 1: Called from Sorgulama for a single record update
        public void SetEditMode(SutKaydi sutKaydi)
        {
            currentEditRecord = sutKaydi;
            isDocumentViewMode = false;
            btnListeyeEkle.IsEnabled = false; // Can't add/update list in single edit mode
            TedarikciListesi.Add(sutKaydi);
            PopulateFieldsFromKayit(sutKaydi);
        }

        // Mode 2: Called from Sorgulama for a full document view/edit
        public void LoadDocumentForViewing(string belgeNo)
        {
            isDocumentViewMode = true;
            currentEditRecord = null;
            btnListeyeEkle.Content = "Kaydı Güncelle";
            
            // Clear any previously tracked deleted records when loading a new document
            _deletedRecords.Clear();

            var kayitlar = _repo.GetSutKayitlariByBelgeNo(belgeNo);
            TedarikciListesi.Clear();
            foreach (var kayit in kayitlar)
            {
                TedarikciListesi.Add(kayit);
            }

            if (TedarikciListesi.Any())
            {
                // Populate top-level fields from the first record
                var firstKayit = TedarikciListesi.First();
                txtBelgeNo.Text = firstKayit.BelgeNo;
                dpTarih.SelectedDate = firstKayit.Tarih;
                switch (firstKayit.IslemTuru)
                {
                    case "Depoya Alım": rbDepoAlim.IsChecked = true; break;
                    case "Depodan Sevk": rbDepodanSevk.IsChecked = true; break;
                    case "Direkt Sevk": rbDirekSevk.IsChecked = true; break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Only run for new record creation (Mode 3)
            if (currentEditRecord == null && !isDocumentViewMode)
            {
                rbDepoAlim.IsChecked = true;
                IslemTuru_Checked(null, null);
                txtBelgeNo.Text = DocumentNumberGenerator.GenerateSutAlimDocumentNumber();
            }
        }

        private void dgTedarikciler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (isDocumentViewMode && dgTedarikciler.SelectedItem is SutKaydi selectedKayit)
            {
                PopulateFieldsFromKayit(selectedKayit);
            }
        }

        private void btnListeyeEkle_Click(object sender, RoutedEventArgs e)
        {
            if (isDocumentViewMode)
            {
                UpdateSelectedRecordInList();
            }
            else
            {
                AddNewRecordToList();
            }
        }

        private void AddNewRecordToList()
        {
            if (string.IsNullOrWhiteSpace(txtTedarikciKod.Text))
            {
                MessageBox.Show("Tedarikçi seçilmedi!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var kayit = new SutKaydi();
            UpdateKayitFromFields(kayit);
            TedarikciListesi.Add(kayit);

            dgTedarikciler.SelectedItem = kayit;
            dgTedarikciler.ScrollIntoView(kayit);
            ClearAnalysisFields();
        }

        private void UpdateSelectedRecordInList()
        {
            if (dgTedarikciler.SelectedItem is SutKaydi selectedKayit)
            {
                UpdateKayitFromFields(selectedKayit);
                dgTedarikciler.Items.Refresh();
                MessageBox.Show("Kayıt listede güncellendi. Kaydetmek için ana 'Kaydet' butonunu kullanın.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Lütfen listeden güncellenecek bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isDocumentViewMode)
                {
                    // Check if there's anything to save (either remaining records or deletions)
                    if ((TedarikciListesi == null || !TedarikciListesi.Any()) && !_deletedRecords.Any())
                    {
                        MessageBox.Show("Kaydedilecek süt kaydı bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show("Belgedeki tüm değişiklikleri kaydetmek istiyor musunuz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) return;

                    // First, delete the records that were marked for deletion
                    foreach (var deletedRecord in _deletedRecords)
                    {
                        _repo.SilSutKaydi(deletedRecord.SutKayitId);
                    }

                    // Then, update the remaining records (if any exist)
                    foreach (var kayit in TedarikciListesi)
                    {
                        _repo.GuncelleSutKaydi(kayit);
                    }
                    
                    // Clear the deleted records collection after successful save
                    _deletedRecords.Clear();
                    
                    MessageBox.Show("Belgedeki tüm kayıtlar başarıyla güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (currentEditRecord != null)
                {
                    var result = MessageBox.Show("Güncellemek istiyor musunuz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) return;

                    UpdateKayitFromFields(currentEditRecord);
                    _repo.GuncelleSutKaydi(currentEditRecord);
                    MessageBox.Show("Süt kaydı başarıyla güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (TedarikciListesi == null || !TedarikciListesi.Any())
                    {
                        MessageBox.Show("Kaydedilecek süt kaydı bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    DateTime tarih = dpTarih.SelectedDate ?? DateTime.Now;
                    string islemTuru = rbDepoAlim.IsChecked == true ? "Depoya Alım" : rbDepodanSevk.IsChecked == true ? "Depodan Sevk" : "Direkt Sevk";
                    foreach (var kayit in TedarikciListesi)
                    {
                        kayit.Tarih = tarih;
                        kayit.IslemTuru = islemTuru;
                        kayit.BelgeNo = txtBelgeNo.Text;
                        _repo.KaydetSutKaydi(kayit);
                    }
                    MessageBox.Show("Tüm süt kayıtları başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    TedarikciListesi.Clear();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private void PopulateFieldsFromKayit(SutKaydi kayit)
        {
            if (kayit == null) return;

            txtBelgeNo.Text = kayit.BelgeNo;
            dpTarih.SelectedDate = kayit.Tarih;

            switch (kayit.IslemTuru)
            {
                case "Depoya Alım": rbDepoAlim.IsChecked = true; break;
                case "Depodan Sevk": rbDepodanSevk.IsChecked = true; break;
                case "Direkt Sevk": rbDirekSevk.IsChecked = true; break;
            }

            txtTedarikciKod.Text = kayit.TedarikciKod;
            txtTedarikciAdi.Text = kayit.TedarikciAdi;
            txtMusteriKod.Text = kayit.MusteriKod;
            txtMusteriAdi.Text = kayit.MusteriAdi;
            txtMiktar.Text = kayit.Miktar.ToString();
            txtFiyat.Text = kayit.Fiyat.ToString();
            txtYag.Text = kayit.Yag?.ToString();
            txtProtein.Text = kayit.Protein?.ToString();
            txtLaktoz.Text = kayit.Laktoz?.ToString();
            txtTKM.Text = kayit.TKM?.ToString();
            txtYKM.Text = kayit.YKM?.ToString();
            txtpH.Text = kayit.pH?.ToString();
            txtIletkenlik.Text = kayit.Iletkenlik?.ToString();
            txtSicaklik.Text = kayit.Sicaklik?.ToString();
            txtYogunluk.Text = kayit.Yogunluk?.ToString();
            txtKesinti.Text = kayit.Kesinti.ToString();
            cmbAntibiyotik.SelectedItem = cmbAntibiyotik.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == kayit.Antibiyotik);
            cmbAracTemizlik.SelectedItem = cmbAracTemizlik.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == kayit.AracTemizlik);
            txtPlaka.Text = kayit.Plaka;
            cmbDurumu.SelectedItem = cmbDurumu.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == kayit.Durumu);
            txtDonma.Text = kayit.DonmaN?.ToString();
            txtBakteri.Text = kayit.Bakteri?.ToString();
            txtSomatik.Text = kayit.Somatik?.ToString();
            txtAciklama.Text = kayit.Aciklama;
        }

        private void UpdateKayitFromFields(SutKaydi kayit)
        {
            var tr = new CultureInfo("tr-TR");
            kayit.BelgeNo = txtBelgeNo.Text;
            kayit.Tarih = dpTarih.SelectedDate ?? DateTime.Now;
            kayit.IslemTuru = rbDepoAlim.IsChecked == true ? "Depoya Alım" : rbDepodanSevk.IsChecked == true ? "Depodan Sevk" : "Direkt Sevk";
            kayit.TedarikciKod = txtTedarikciKod.Text;
            kayit.TedarikciAdi = txtTedarikciAdi.Text;
            kayit.MusteriKod = txtMusteriKod.Text;
            kayit.MusteriAdi = txtMusteriAdi.Text;

            kayit.Miktar = decimal.TryParse(txtMiktar.Text, NumberStyles.Any, tr, out decimal miktar) ? miktar : 0;
            kayit.Fiyat = decimal.TryParse(txtFiyat.Text, NumberStyles.Any, tr, out decimal fiyat) ? fiyat : 0;
            kayit.Kesinti = decimal.TryParse(txtKesinti.Text, NumberStyles.Any, tr, out decimal kesinti) ? kesinti : 0;

            kayit.Yag = decimal.TryParse(txtYag.Text, NumberStyles.Any, tr, out decimal yagValue) ? yagValue : (decimal?)null;
            kayit.Protein = decimal.TryParse(txtProtein.Text, NumberStyles.Any, tr, out decimal proteinValue) ? proteinValue : (decimal?)null;
            kayit.Laktoz = decimal.TryParse(txtLaktoz.Text, NumberStyles.Any, tr, out decimal laktozValue) ? laktozValue : (decimal?)null;
            kayit.TKM = decimal.TryParse(txtTKM.Text, NumberStyles.Any, tr, out decimal tkmValue) ? tkmValue : (decimal?)null;
            kayit.YKM = decimal.TryParse(txtYKM.Text, NumberStyles.Any, tr, out decimal ykmValue) ? ykmValue : (decimal?)null;
            kayit.pH = decimal.TryParse(txtpH.Text, NumberStyles.Any, tr, out decimal phValue) ? phValue : (decimal?)null;
            kayit.Iletkenlik = decimal.TryParse(txtIletkenlik.Text, NumberStyles.Any, tr, out decimal iletkenlikValue) ? iletkenlikValue : (decimal?)null;
            kayit.Sicaklik = decimal.TryParse(txtSicaklik.Text, NumberStyles.Any, tr, out decimal sicaklikValue) ? sicaklikValue : (decimal?)null;
            kayit.Yogunluk = decimal.TryParse(txtYogunluk.Text, NumberStyles.Any, tr, out decimal yogunlukValue) ? yogunlukValue : (decimal?)null;
            kayit.DonmaN = decimal.TryParse(txtDonma.Text, NumberStyles.Any, tr, out decimal donmaValue) ? donmaValue : (decimal?)null;
            kayit.Bakteri = decimal.TryParse(txtBakteri.Text, NumberStyles.Any, tr, out decimal bakteriValue) ? bakteriValue : (decimal?)null;
            kayit.Somatik = decimal.TryParse(txtSomatik.Text, NumberStyles.Any, tr, out decimal somatikValue) ? somatikValue : (decimal?)null;

            kayit.Antibiyotik = (cmbAntibiyotik.SelectedItem as ComboBoxItem)?.Content.ToString();
            kayit.AracTemizlik = (cmbAracTemizlik.SelectedItem as ComboBoxItem)?.Content.ToString();
            kayit.Plaka = txtPlaka.Text;
            kayit.Durumu = (cmbDurumu.SelectedItem as ComboBoxItem)?.Content.ToString();
            kayit.Aciklama = txtAciklama.Text;
        }

        private void ClearAnalysisFields()
        {
            txtTedarikciKod.Clear();
            txtTedarikciAdi.Clear();
            txtMusteriKod.Clear();
            txtMusteriAdi.Clear();
            txtMiktar.Clear();
            txtFiyat.Clear();
            txtYag.Clear();
            txtProtein.Clear();
            txtLaktoz.Clear();
            txtTKM.Clear();
            txtYKM.Clear();
            txtpH.Clear();
            txtIletkenlik.Clear();
            txtSicaklik.Clear();
            txtYogunluk.Clear();
            txtKesinti.Clear();
            txtDonma.Clear();
            txtBakteri.Clear();
            txtSomatik.Clear();
            txtAciklama.Clear();
        }

        private void btnSecileniSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgTedarikciler.SelectedItem is SutKaydi secili)
            {
                if (MessageBox.Show("Seçili satırı silmek istediğine emin misin?",
                                    "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Add to deleted records collection to be removed from database on save
                    if (secili.SutKayitId > 0) // Only if it's an existing record (has been saved to DB)
                    {
                        _deletedRecords.Add(secili);
                    }
                    
                    // Remove from the display list
                    TedarikciListesi.Remove(secili);
                }
            }
            else
            {
                MessageBox.Show("Silinecek satır seçilmedi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void IslemTuru_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            if (rbDepoAlim.IsChecked == true)
            {
                SetTedarikciEnabled(true);
                SetMusteriEnabled(false);
                txtMusteriAdi.Text = null;
                txtMusteriKod.Text = null;
            }
            else if (rbDirekSevk.IsChecked == true)
            {
                SetTedarikciEnabled(true);
                SetMusteriEnabled(true);
            }
            else if (rbDepodanSevk.IsChecked == true)
            {
                SetTedarikciEnabled(false);
                SetMusteriEnabled(true);
                txtTedarikciAdi.Text = null;
                txtTedarikciKod.Text = null;
            }
        }

        private void SetTedarikciEnabled(bool durum)
        {
            btnTedarikciSec.IsEnabled = durum;
            txtTedarikciKod.IsEnabled = durum;
            txtTedarikciAdi.IsEnabled = durum;
            txtTedarikciKod.Background = durum ? Brushes.White : Brushes.LightGray;
            txtTedarikciAdi.Background = durum ? Brushes.White : Brushes.LightGray;
        }

        private void SetMusteriEnabled(bool durum)
        {
            btnMusteriSec.IsEnabled = durum;
            txtMusteriKod.IsEnabled = durum;
            txtMusteriAdi.IsEnabled = durum;
            txtMusteriKod.Background = durum ? Brushes.White : Brushes.LightGray;
            txtMusteriAdi.Background = durum ? Brushes.White : Brushes.LightGray;
        }

        private void btnTedarikciSec_Click(object sender, RoutedEventArgs e)
        {
            var cariWindow = new CariListesiWindow { Owner = this };
            if (cariWindow.ShowDialog() == true && cariWindow.SecilenCari != null)
            {
                txtTedarikciKod.Text = cariWindow.SecilenCari.CariKod;
                txtTedarikciAdi.Text = cariWindow.SecilenCari.CariAdi;
            }
        }

        private void btnMusteriSec_Click(object sender, RoutedEventArgs e)
        {
            var cariWindow = new CariListesiWindow { Owner = this };
            if (cariWindow.ShowDialog() == true && cariWindow.SecilenCari != null)
            {
                txtMusteriKod.Text = cariWindow.SecilenCari.CariKod;
                txtMusteriAdi.Text = cariWindow.SecilenCari.CariAdi;
            }
        }

        private void btnKapat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnYeni_Click(object sender, RoutedEventArgs e)
        {
            // Confirm if user wants to clear the current form
            if (TedarikciListesi.Any())
            {
                var result = MessageBox.Show("Mevcut veriler silinecek. Yeni bir işlem başlatmak istediğinize emin misiniz?", 
                                           "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }

            // Clear the data grid
            TedarikciListesi.Clear();
            
            // Reset form fields to initial state
            txtTedarikciKod.Clear();
            txtTedarikciAdi.Clear();
            txtMusteriKod.Clear();
            txtMusteriAdi.Clear();
            
            // Reset analysis fields
            txtMiktar.Clear();
            txtFiyat.Clear();
            txtYag.Clear();
            txtProtein.Clear();
            txtLaktoz.Clear();
            txtTKM.Clear();
            txtYKM.Clear();
            txtpH.Clear();
            txtIletkenlik.Clear();
            txtSicaklik.Clear();
            txtYogunluk.Clear();
            txtKesinti.Clear();
            txtDonma.Clear();
            txtBakteri.Clear();
            txtSomatik.Clear();
            txtAciklama.Clear();
            txtPlaka.Clear();
            
            // Reset checkboxes/selections
            cmbAntibiyotik.SelectedIndex = 0; // "Negatif"
            cmbAracTemizlik.SelectedIndex = 0; // "Temiz"
            cmbDurumu.SelectedIndex = 0; // "Kabul"
            
            // Reset date to today
            dpTarih.SelectedDate = DateTime.Today;
            
            // Reset operation type to default
            rbDepoAlim.IsChecked = true;
            IslemTuru_Checked(null, null); // Update UI based on selection
            
            // Generate new document number
            txtBelgeNo.Text = DocumentNumberGenerator.GenerateSutAlimDocumentNumber();
            
            // Reset edit mode flags
            currentEditRecord = null;
            isDocumentViewMode = false;
            btnListeyeEkle.IsEnabled = true;
            btnListeyeEkle.Content = "➕ Listeye Ekle"; // Reset button text
        }
    }
}