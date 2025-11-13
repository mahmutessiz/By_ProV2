using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using By_ProV2.DataAccess;
using By_ProV2.Models;
using By_ProV2.Helpers;
using By_ProV2.Services;

namespace By_ProV2
{
    public partial class SutAlimFormu : Window
    {
        public ObservableCollection<SutKaydi> TedarikciListesi { get; set; }
        private readonly SutRepository _repo = new SutRepository();
        private readonly ParameterRepository _paramRepo = new ParameterRepository();
        private readonly SutEnvanteriService _envanterService = new SutEnvanteriService();
        private Parameter _latestParameters;
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
            _latestParameters = _paramRepo.GetLatestParametreler();
            // Only run for new record creation (Mode 3)
            if (currentEditRecord == null && !isDocumentViewMode)
            {
                rbDepoAlim.IsChecked = true;
                IslemTuru_Checked(null, null);
                txtBelgeNo.Text = DocumentNumberGenerator.GenerateSutAlimDocumentNumber();
            }
            UpdateCurrentDayInventoryDisplay(); // Update inventory display on load
        }

        private void UpdateCurrentDayInventoryDisplay()
        {
            // Ensure the day-change logic has run for today before trying to read the value
            _envanterService.HandleDayChange(DateTime.Today); 
            
            var todayInventory = _envanterService.GetEnvanterByTarih(DateTime.Today);
            if (todayInventory != null)
            {
                lblCurrentDayInventory.Text = todayInventory.KalanSut.ToString("N2");
            }
            else
            {
                lblCurrentDayInventory.Text = "0.00";
            }
        }

        private void dgTedarikciler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgTedarikciler.SelectedItem is SutKaydi selectedKayit)
            {
                PopulateFieldsFromKayit(selectedKayit);

                // Change button text to indicate update mode
                if (!isDocumentViewMode && currentEditRecord == null)
                {
                    btnListeyeEkle.Content = "✏️ Güncelle";
                }
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
                // If there's a selected item in the grid, update it; otherwise, add as new
                if (dgTedarikciler.SelectedItem != null)
                {
                    UpdateSelectedRecordInList();
                    // Reset button text after update
                    btnListeyeEkle.Content = "➕ Listeye Ekle";
                }
                else
                {
                    AddNewRecordToList();
                    // Ensure button shows "Add" text for new records
                    btnListeyeEkle.Content = "➕ Listeye Ekle";
                }
            }
        }

        private void AddNewRecordToList()
        {
            string islemTuru = rbDepoAlim.IsChecked == true ? "Depoya Alım"
                : rbDepodanSevk.IsChecked == true ? "Depodan Sevk"
                : "Direkt Sevk";

            // For "Depodan Sevk", tedarikçi is not required but müşteri is required
            // For "Depoya Alım", tedarikçi is required but müşteri is not required
            // For "Direkt Sevk", both tedarikçi and müşteri are required
            if (islemTuru == "Depoya Alım" || islemTuru == "Direkt Sevk")
            {
                if (string.IsNullOrWhiteSpace(txtTedarikciKod.Text))
                {
                    MessageBox.Show("Tedarikçi seçilmedi!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // For "Depodan Sevk" and "Direkt Sevk", müşteri is required
            if (islemTuru == "Depodan Sevk" || islemTuru == "Direkt Sevk")
            {
                if (string.IsNullOrWhiteSpace(txtMusteriKod.Text))
                {
                    MessageBox.Show("Müşteri seçilmedi!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var kayit = new SutKaydi();
            UpdateKayitFromFields(kayit);
            TedarikciListesi.Add(kayit);

            dgTedarikciler.SelectedItem = kayit;
            dgTedarikciler.ScrollIntoView(kayit);
            ClearAnalysisFields();

            // Reset button text after adding new record
            btnListeyeEkle.Content = "➕ Listeye Ekle";
        }

        private void UpdateSelectedRecordInList()
        {
            if (dgTedarikciler.SelectedItem is SutKaydi selectedKayit)
            {
                // Store the original ID to know if this is an existing record
                int originalId = selectedKayit.SutKayitId;

                UpdateKayitFromFields(selectedKayit);

                // If this was an existing record in document view mode, or we're in edit mode for single record
                if (isDocumentViewMode || currentEditRecord != null)
                {
                    dgTedarikciler.Items.Refresh();
                    MessageBox.Show("Kayıt listede güncellendi. Kaydetmek için ana 'Kaydet' butonunu kullanın.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // For regular mode, we remove the old item and add the updated one
                    int index = TedarikciListesi.IndexOf(selectedKayit);
                    if (index >= 0)
                    {
                        TedarikciListesi.RemoveAt(index);
                        TedarikciListesi.Insert(index, selectedKayit);
                        dgTedarikciler.SelectedItem = selectedKayit;
                    }
                }

                // Clear the input fields after updating
                ClearAnalysisFields();
            }
            else
            {
                MessageBox.Show("Lütfen listeden güncellenecek bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgTedarikciler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If no item is selected, reset the button text to default
            if (dgTedarikciler.SelectedItem == null && !isDocumentViewMode && currentEditRecord == null)
            {
                btnListeyeEkle.Content = "➕ Listeye Ekle";
            }
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Mode 1: Single Record Edit
                if (currentEditRecord != null && !isDocumentViewMode)
                {
                    if (MessageBox.Show("Değişiklikleri kaydetmek istiyor musunuz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                    var originalRecord = _repo.GetSutKaydiById(currentEditRecord.SutKayitId);
                    UpdateKayitFromFields(currentEditRecord);
                    _repo.GuncelleSutKaydi(currentEditRecord);

                    if (originalRecord != null)
                    {
                        _envanterService.UpdateInventoryForTransactionChange(
                            originalRecord.Tarih.Date,
                            originalRecord.IslemTuru,
                            currentEditRecord.IslemTuru,
                            originalRecord.NetMiktar,
                            currentEditRecord.NetMiktar);
                    }
                    MessageBox.Show("Süt kaydı başarıyla güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // Mode 2 & 3: Document-based operations (New or Edit)
                else
                {
                    if (!TedarikciListesi.Any() && !_deletedRecords.Any())
                    {
                        MessageBox.Show("Kaydedilecek bir değişiklik bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (MessageBox.Show("Belgedeki tüm değişiklikleri kaydetmek istiyor musunuz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                    // Step 1: Process Deletions
                    foreach (var deletedRecord in _deletedRecords)
                    {
                        if (deletedRecord.SutKayitId > 0)
                        {
                            _repo.SilSutKaydi(deletedRecord.SutKayitId);
                        }
                    }

                    // Step 2: Process Additions and Updates
                    DateTime tarih = dpTarih.SelectedDate ?? DateTime.Now;
                    string islemTuru = rbDepoAlim.IsChecked == true ? "Depoya Alım" : rbDepodanSevk.IsChecked == true ? "Depodan Sevk" : "Direkt Sevk";

                    foreach (var kayit in TedarikciListesi)
                    {
                        kayit.Tarih = tarih;
                        kayit.IslemTuru = islemTuru;
                        kayit.BelgeNo = txtBelgeNo.Text;

                        // If ID is 0, it's a new record
                        if (kayit.SutKayitId == 0)
                        {
                            _repo.KaydetSutKaydi(kayit);
                            _envanterService.UpdateInventoryForTransaction(kayit.Tarih.Date, kayit.IslemTuru, kayit.NetMiktar);
                        }
                        // Otherwise, it's an existing record to update
                        else
                        {
                            // We need the original state to calculate the inventory difference
                            var originalRecord = _repo.GetSutKaydiById(kayit.SutKayitId);
                            _repo.GuncelleSutKaydi(kayit);

                            if (originalRecord != null)
                            {
                                _envanterService.UpdateInventoryForTransactionChange(
                                    originalRecord.Tarih.Date,
                                    originalRecord.IslemTuru,
                                    kayit.IslemTuru,
                                    originalRecord.NetMiktar,
                                    kayit.NetMiktar);
                            }
                        }
                    }
                    MessageBox.Show("Belgedeki tüm kayıtlar başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _deletedRecords.Clear();
                TedarikciListesi.Clear();
                UpdateCurrentDayInventoryDisplay(); // Refresh inventory display after save
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında bir hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
            txtNetMiktar.Text = kayit.NetMiktar.ToString(); // Display stored value
            txtYag.Text = kayit.Yag?.ToString();
            txtProtein.Text = kayit.Protein?.ToString();
            txtLaktoz.Text = kayit.Laktoz?.ToString();
            txtTKM.Text = kayit.TKM?.ToString();
            txtYKM.Text = kayit.YKM?.ToString();
            txtpH.Text = kayit.pH?.ToString();
            txtIletkenlik.Text = kayit.Iletkenlik?.ToString();
            txtSicaklik.Text = kayit.Sicaklik?.ToString();
            txtYogunluk.Text = kayit.Yogunluk?.ToString();
            txtKesinti.Text = kayit.Kesinti.ToString(); // Display calculated/loaded value
            cmbAntibiyotik.SelectedItem = cmbAntibiyotik.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == kayit.Antibiyotik);
            cmbAracTemizlik.SelectedItem = cmbAracTemizlik.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == kayit.AracTemizlik);
            txtPlaka.Text = kayit.Plaka;
            cmbDurumu.SelectedItem = cmbDurumu.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == kayit.Durumu);
            txtDonma.Text = kayit.DonmaN?.ToString("F3"); // Show with 3 decimal places to maintain precision
            txtBakteri.Text = kayit.Bakteri?.ToString();
            txtSomatik.Text = kayit.Somatik?.ToString();
            txtAciklama.Text = kayit.Aciklama;
            
            // After populating all values, recalculate to ensure consistency
            // (especially important when editing existing records)
            CalculateAndDisplayNetMiktar();
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
            
            // Calculate NetMiktar based on all quality parameters (automatic calculation)
            kayit.NetMiktar = CalculateNetMiktar(miktar,
                decimal.TryParse(txtDonma.Text, NumberStyles.Any, tr, out decimal donmaValue) ? donmaValue : (decimal?)null,
                decimal.TryParse(txtYag.Text, NumberStyles.Any, tr, out decimal yagValue) ? yagValue : (decimal?)null,
                decimal.TryParse(txtProtein.Text, NumberStyles.Any, tr, out decimal proteinValue) ? proteinValue : (decimal?)null,
                decimal.TryParse(txtSomatik.Text, NumberStyles.Any, tr, out decimal somatikValue) ? somatikValue : (decimal?)null,
                decimal.TryParse(txtBakteri.Text, NumberStyles.Any, tr, out decimal bakteriValue) ? bakteriValue : (decimal?)null,
                decimal.TryParse(txtpH.Text, NumberStyles.Any, tr, out decimal phValue) ? phValue : (decimal?)null,
                decimal.TryParse(txtYogunluk.Text, NumberStyles.Any, tr, out decimal yogunlukValue) ? yogunlukValue : (decimal?)null);
            
            // Calculate total kesinti automatically based on quality parameters
            kayit.Kesinti = CalculateKesinti(miktar,
                decimal.TryParse(txtDonma.Text, NumberStyles.Any, tr, out decimal donmaValue2) ? donmaValue2 : (decimal?)null,
                decimal.TryParse(txtYag.Text, NumberStyles.Any, tr, out decimal yagValue2) ? yagValue2 : (decimal?)null,
                decimal.TryParse(txtProtein.Text, NumberStyles.Any, tr, out decimal proteinValue2) ? proteinValue2 : (decimal?)null,
                decimal.TryParse(txtSomatik.Text, NumberStyles.Any, tr, out decimal somatikValue2) ? somatikValue2 : (decimal?)null,
                decimal.TryParse(txtBakteri.Text, NumberStyles.Any, tr, out decimal bakteriValue2) ? bakteriValue2 : (decimal?)null,
                decimal.TryParse(txtpH.Text, NumberStyles.Any, tr, out decimal phValue2) ? phValue2 : (decimal?)null,
                decimal.TryParse(txtYogunluk.Text, NumberStyles.Any, tr, out decimal yogunlukValue2) ? yogunlukValue2 : (decimal?)null);

            kayit.Yag = yagValue; // Already parsed above
            kayit.Protein = proteinValue; // Already parsed above
            kayit.Laktoz = decimal.TryParse(txtLaktoz.Text, NumberStyles.Any, tr, out decimal laktozValue) ? laktozValue : (decimal?)null;
            kayit.TKM = decimal.TryParse(txtTKM.Text, NumberStyles.Any, tr, out decimal tkmValue) ? tkmValue : (decimal?)null;
            kayit.YKM = decimal.TryParse(txtYKM.Text, NumberStyles.Any, tr, out decimal ykmValue) ? ykmValue : (decimal?)null;
            kayit.pH = phValue; // Already parsed above
            kayit.Iletkenlik = decimal.TryParse(txtIletkenlik.Text, NumberStyles.Any, tr, out decimal iletkenlikValue) ? iletkenlikValue : (decimal?)null;
            kayit.Sicaklik = decimal.TryParse(txtSicaklik.Text, NumberStyles.Any, tr, out decimal sicaklikValue) ? sicaklikValue : (decimal?)null;
            kayit.Yogunluk = yogunlukValue; // Already parsed above
            kayit.DonmaN = donmaValue; // Already parsed above
            kayit.Bakteri = bakteriValue; // Already parsed above
            kayit.Somatik = somatikValue; // Already parsed above

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
            txtNetMiktar.Clear();
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
                    // If it's an existing record, revert the inventory transaction immediately
                    if (secili.SutKayitId > 0)
                    {
                        _envanterService.RevertTransaction(secili.Tarih.Date, secili.IslemTuru, secili.NetMiktar);
                        _deletedRecords.Add(secili); // Track for final DB deletion on save
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

        private decimal CalculateNetMiktar(decimal brütMiktar, decimal? donmaNoktasi, decimal? yag, decimal? protein, decimal? somatik, decimal? bakteri, decimal? pH, decimal? yogunluk)
        {
            // Calculate based on Turkish dairy standards
            decimal netMiktar = brütMiktar;

            // Calculate total deductions based on quality parameters
            decimal totalKesinti = CalculateKesinti(brütMiktar, donmaNoktasi, yag, protein, somatik, bakteri, pH, yogunluk);

            // Apply all deductions to brüt miktar
            netMiktar -= totalKesinti;

            // Ensure net quantity is not negative
            return netMiktar < 0 ? 0 : netMiktar;
        }

        private decimal CalculateKesinti(decimal brütMiktar, decimal? donmaNoktasi, decimal? yag, decimal? protein, decimal? somatik, decimal? bakteri, decimal? pH, decimal? yogunluk)
        {
            decimal totalKesinti = 0;

            if (donmaNoktasi.HasValue)
            {
                decimal referansDeger = _latestParameters?.DonmaNoktasiReferansDegeri ?? -0.520m;
                decimal kesintiBaslangicLimiti = _latestParameters?.DonmaNoktasiKesintiAltLimit ?? -0.515m;
                
                // Apply deductions only if the measured freezing point is higher than the threshold (indicating possible dilution)
                if (donmaNoktasi > kesintiBaslangicLimiti)
                {
                    // Calculate how much the measured freezing point deviates from the reference value
                    // A positive difference indicates dilution (freezing point is higher than reference)
                    decimal donmaNoktasiFarki = donmaNoktasi.Value - referansDeger;
                    
                    // Only apply deductions if there is actual dilution (measured value is higher than reference)
                    if (donmaNoktasiFarki > 0)
                    {
                        // Calculate the percentage deviation based on the reference value
                        decimal yuzdeOrani = Math.Abs(donmaNoktasiFarki / referansDeger) * 100;
                        
                        // Apply percentage-based deduction to gross amount
                        decimal dilusyonMiktari = Math.Round(brütMiktar * yuzdeOrani / 100);
                        
                        // Only add positive deductions
                        if (dilusyonMiktari > 0)
                        {
                            totalKesinti += dilusyonMiktari;
                        }
                    }
                }
            }

            // Only freezing point is considered for deductions - other parameters do not affect net miktar
            // This simplifies the calculation to only the essential parameter for dairy dilution detection

            return totalKesinti;
        }

        private void txtMiktar_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-calculate NetMiktar when key parameters change
            // This will update the display in real-time
            CalculateAndDisplayNetMiktar();
        }

        private void txtDonma_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-calculate NetMiktar when key parameters change
            CalculateAndDisplayNetMiktar();
        }

        private void CalculateAndDisplayNetMiktar()
        {
            if (IsLoaded) // Only if the form is loaded
            {
                var tr = new CultureInfo("tr-TR");
                
                // Parse the current values from the form
                decimal miktar = decimal.TryParse(txtMiktar.Text, NumberStyles.Any, tr, out decimal parsedMiktar) ? parsedMiktar : 0;
                decimal? donmaN = decimal.TryParse(txtDonma.Text, NumberStyles.Any, tr, out decimal parsedDonmaN) ? parsedDonmaN : (decimal?)null;
                decimal? yag = decimal.TryParse(txtYag.Text, NumberStyles.Any, tr, out decimal parsedYag) ? parsedYag : (decimal?)null;
                decimal? protein = decimal.TryParse(txtProtein.Text, NumberStyles.Any, tr, out decimal parsedProtein) ? parsedProtein : (decimal?)null;
                decimal? somatik = decimal.TryParse(txtSomatik.Text, NumberStyles.Any, tr, out decimal parsedSomatik) ? parsedSomatik : (decimal?)null;
                decimal? bakteri = decimal.TryParse(txtBakteri.Text, NumberStyles.Any, tr, out decimal parsedBakteri) ? parsedBakteri : (decimal?)null;
                decimal? ph = decimal.TryParse(txtpH.Text, NumberStyles.Any, tr, out decimal parsedPh) ? parsedPh : (decimal?)null;
                decimal? yogunluk = decimal.TryParse(txtYogunluk.Text, NumberStyles.Any, tr, out decimal parsedYogunluk) ? parsedYogunluk : (decimal?)null;

                // Calculate net miktar based on parameters
                decimal netMiktar = CalculateNetMiktar(miktar, donmaN, yag, protein, somatik, bakteri, ph, yogunluk);
                
                // Calculate total kesinti for display
                decimal totalKesinti = CalculateKesinti(miktar, donmaN, yag, protein, somatik, bakteri, ph, yogunluk);
                
                // Update the displays
                txtNetMiktar.Text = netMiktar.ToString("F2"); // Format to 2 decimal places
                txtKesinti.Text = totalKesinti.ToString("F2"); // Show calculated deduction
            }
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
            txtNetMiktar.Clear(); // Will be auto-calculated when needed
            txtYag.Clear();
            txtProtein.Clear();
            txtLaktoz.Clear();
            txtTKM.Clear();
            txtYKM.Clear();
            txtpH.Clear();
            txtIletkenlik.Clear();
            txtSicaklik.Clear();
            txtYogunluk.Clear();
            txtKesinti.Clear(); // Will be auto-calculated when needed
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

            UpdateCurrentDayInventoryDisplay(); // Refresh inventory display after new form
        }
    }
}