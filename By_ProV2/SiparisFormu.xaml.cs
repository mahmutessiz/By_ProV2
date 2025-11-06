using System;
using System.Collections;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using By_ProV2.Helpers;
using System.Collections.ObjectModel;           // ObservableCollection
using System.ComponentModel;                    // INotifyPropertyChanged
using By_ProV2.ViewModels; 
using By_ProV2.Models;
using System.Linq;
using Microsoft.Win32; // SaveFileDialog için
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace By_ProV2
{
    /// <summary>
    /// SiparisFormu.xaml etkileşim mantığı
    /// </summary>
    public partial class SiparisFormu : Window
    {
        public MainViewModel ViewModel { get; set; }

        public SiparisFormu()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            this.DataContext = ViewModel;
            _kaydederekKapaniyor = false;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            dtSiparisTarihi.SelectedDate = DateTime.Now;
            // Eğer düzenleme modundaysa (örneğin ViewModel'de SiparisId doluysa)
            if (ViewModel.SiparisNo != "000000000")
            {
                // Numara zaten atanmış, UI sadece göstersin
                return;
            }

            // Yeni sipariş formuysa hiçbir şey yapma, kaydette üretilecek

        }

        private string GenerateIncrementalCode(string type) // "S" veya "P"
        {
            string year = DateTime.Now.ToString("yy");
            int nextNumber = GetNextNumberFromDb(year, type);
            return $"{year}{type}{nextNumber:D5}";
        }
        private int GetNextNumberFromDb(string year, string type)
        {
            int nextNumber = 1;

            string connStr = ConfigurationHelper.GetConnectionString("db");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(@"
                    SELECT SonNumara FROM Numarator 
                    WITH (UPDLOCK, ROWLOCK)
                    WHERE Yil = @Yil AND Tip = @Tip", conn, tran);

                        cmd.Parameters.AddWithValue("@Yil", year);
                        cmd.Parameters.AddWithValue("@Tip", type);

                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            nextNumber = (int)result + 1;

                            SqlCommand updateCmd = new SqlCommand(@"
                        UPDATE Numarator 
                        SET SonNumara = @NextNumber 
                        WHERE Yil = @Yil AND Tip = @Tip", conn, tran);

                            updateCmd.Parameters.AddWithValue("@NextNumber", nextNumber);
                            updateCmd.Parameters.AddWithValue("@Yil", year);
                            updateCmd.Parameters.AddWithValue("@Tip", type);

                            updateCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            SqlCommand insertCmd = new SqlCommand(@"
                        INSERT INTO Numarator (Yil, Tip, SonNumara)
                        VALUES (@Yil, @Tip, @NextNumber)", conn, tran);

                            insertCmd.Parameters.AddWithValue("@Yil", year);
                            insertCmd.Parameters.AddWithValue("@Tip", type);
                            insertCmd.Parameters.AddWithValue("@NextNumber", nextNumber);

                            insertCmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("Numara üretim hatası: " + ex.Message);
                    }
                }
            }

            return nextNumber;
        }
        private void EnsureSiparisNoGenerated()
        {
            if (txtSiparisNo.Text == "000000000")
            {
                txtSiparisNo.Text = GenerateIncrementalCode("S");
            }
        }

        private void EnsureProformaNoGenerated()
        {
            if (txtSatisSiparisNo.Text == "000000000")
            {
                txtSatisSiparisNo.Text = GenerateIncrementalCode("P");
            }
        }

        private void chkNakit_Checked(object sender, RoutedEventArgs e)
        {
            chkKrediKarti.IsEnabled = false;
            cmbOdemeSekli.IsEnabled = false;
            ViewModel.IsAlisOnOdeme = true;
        }

        private void chkNakit_Unchecked(object sender, RoutedEventArgs e)
        {
            chkKrediKarti.IsEnabled = true;
            cmbOdemeSekli.IsEnabled = true;
            ViewModel.IsAlisOnOdeme = false;
        }

        private void chkKrediKarti_Checked(object sender, RoutedEventArgs e)
        {
            chkNakit.IsEnabled = false;
            cmbOdemeSekli.IsEnabled = false;
            ViewModel.IsAlisKrediKartiOdeme = true;
        }

        private void chkKrediKarti_Unchecked(object sender, RoutedEventArgs e)
        {
            chkNakit.IsEnabled = true;
            cmbOdemeSekli.IsEnabled = true;
            ViewModel.IsAlisKrediKartiOdeme = false;
        }

        private void cmbOdemeSekli_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || ViewModel == null) return;

            if (cmbOdemeSekli.SelectedItem != null && cmbOdemeSekli.SelectedIndex > 0)
            {
                chkNakit.IsEnabled = false;
                chkKrediKarti.IsEnabled = false;

                var selectedItem = cmbOdemeSekli.SelectedItem as ComboBoxItem;
                ViewModel.AlisVade = selectedItem?.Content?.ToString() ?? "Belirtilmedi";
            }
            else
            {
                if (chkNakit.IsChecked == false && chkKrediKarti.IsChecked == false)
                {
                    chkNakit.IsEnabled = true;
                    chkKrediKarti.IsEnabled = true;
                }

                ViewModel.AlisVade = null;
            }
        }


        private void chkSatisNakit_Checked(object sender, RoutedEventArgs e)
        {
            chkSatisKrediKarti.IsEnabled = false;
            cmbSatisOdemeSekli.IsEnabled = false;
            ViewModel.IsSatisOnOdeme = true;
        }

        private void chkSatisNakit_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSatisKrediKarti.IsEnabled = true;
            cmbSatisOdemeSekli.IsEnabled = true;
            ViewModel.IsSatisOnOdeme = false;
        }

        private void chkSatisKrediKarti_Checked(object sender, RoutedEventArgs e)
        {
            chkSatisNakit.IsEnabled = false;
            cmbSatisOdemeSekli.IsEnabled = false;
            ViewModel.IsSatisKrediKartiOdeme = true;
        }

        private void chkSatisKrediKarti_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSatisNakit.IsEnabled = true;
            cmbSatisOdemeSekli.IsEnabled = true;
            ViewModel.IsSatisKrediKartiOdeme = false;
        }

        private void cmbSatisOdemeSekli_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || ViewModel == null) return;

            if (cmbSatisOdemeSekli.SelectedItem != null && cmbSatisOdemeSekli.SelectedIndex > 0)
            {
                chkSatisNakit.IsEnabled = false;
                chkSatisKrediKarti.IsEnabled = false;

                // ⛔ Hatalı: ComboBoxItem nesnesinin .ToString()'i alınıyor
                // ViewModel.SatisVade = cmbSatisOdemeSekli.SelectedItem.ToString();

                // ✅ Doğru: ComboBoxItem'ın Content içeriği alınmalı
                var selectedItem = cmbSatisOdemeSekli.SelectedItem as ComboBoxItem;
                ViewModel.SatisVade = selectedItem?.Content?.ToString() ?? "Belirtilmedi";
            }
            else
            {
                if (chkSatisNakit.IsChecked == false && chkSatisKrediKarti.IsChecked == false)
                {
                    chkSatisNakit.IsEnabled = true;
                    chkSatisKrediKarti.IsEnabled = true;
                }

                ViewModel.SatisVade = null;
            }
        }


        private void GuncelleIskontoKutulari()
        {
            if (ViewModel.AlisCari != null)
            {
                if (ViewModel.IsAlisKrediKartiOdeme)
                {
                    txtIsk1.Text = ViewModel.AlisCari.KKIsk1.ToString("F2");
                    txtIsk2.Text = ViewModel.AlisCari.KKIsk2.ToString("F2");
                    txtIsk3.Text = ViewModel.AlisCari.KKIsk3.ToString("F2");
                    txtIsk4.Text = ViewModel.AlisCari.KKIsk4.ToString("F2");
                }
                else
                {
                    txtIsk1.Text = ViewModel.AlisCari.Isk1.ToString("F2");
                    txtIsk2.Text = ViewModel.AlisCari.Isk2.ToString("F2");
                    txtIsk3.Text = ViewModel.AlisCari.Isk3.ToString("F2");
                    txtIsk4.Text = ViewModel.AlisCari.Isk4.ToString("F2");
                }

                if (ViewModel.AlisCari != null && ViewModel.IsFabrikaTeslim)
                {
                    txtNakIsk.Text = ViewModel.AlisCari.NakliyeIskonto.ToString("F2");
                }
                else if (ViewModel.SatisCari != null && ViewModel.IsFabrikaTeslim)
                {
                    txtNakIsk.Text = ViewModel.SatisCari.NakliyeIskonto.ToString("F2");
                }
            }
        }

        private void btnCariListe_Click(object sender, RoutedEventArgs e)
        {
            CariListesiWindow cariListe = new CariListesiWindow();

            bool? result = cariListe.ShowDialog();

            if (result == true && cariListe.SecilenCari != null)
            {
                var cari = cariListe.SecilenCari;

                txtCariKod.Text = cari.CariKod;
                txtCariAdGosterim.Text = cari.CariAdi;
                txtCariAdresGosterim.Text = cari.Adres;
                txtVergiDairesiGosterim.Text = cari.VergiDairesi;
                txtVergiNoGosterim.Text = cari.VergiNo;
                txtTelefonGosterim.Text = cari.Telefon;

                // ViewModel'a ata
                ViewModel.AlisCari = cari;
                
                txtNakIsk.Text = cari.NakliyeIskonto.ToString("F2");
                // İskontoları da otomatik ata
                if (ViewModel.IsAlisKrediKartiOdeme)
                {
                    txtIsk1.Text = cari.KKIsk1.ToString("F2");
                    txtIsk2.Text = cari.KKIsk2.ToString("F2");
                    txtIsk3.Text = cari.KKIsk3.ToString("F2");
                    txtIsk4.Text = cari.KKIsk4.ToString("F2");
                }
                else
                {
                    txtIsk1.Text = cari.Isk1.ToString("F2");
                    txtIsk2.Text = cari.Isk2.ToString("F2");
                    txtIsk3.Text = cari.Isk3.ToString("F2");
                    txtIsk4.Text = cari.Isk4.ToString("F2");
                }

                GuncelleIskontoKutulari();
            }

        }

        private void btnSatisCariListe_Click(object sender, RoutedEventArgs e)
        {
            // Buraya butona tıklandığında yapılacak işlemleri yazın.
            // Örnek olarak başka bir cari listesi penceresi açmak gibi:
            CariListesiWindow cariListe = new CariListesiWindow();
            bool? result = cariListe.ShowDialog();

            if (result == true && cariListe.SecilenCari != null)
            {
                var cari = cariListe.SecilenCari;

                // Örneğin, satış carisi bilgilerini doldur:
                txtSatisCariKod.Text = cari.CariKod;
                txtProCariAdi.Text = cari.CariAdi;
                txtProCariAdres.Text = cari.Adres;
                txtProVergiDairesi.Text = cari.VergiDairesi;
                txtProVergiNo.Text = cari.VergiNo;
                txtProTelefon.Text = cari.Telefon;
                txtProAciklama1.Text = cari.SoforAdSoyad;
                txtProAciklama2.Text = cari.Plaka1;
                txtProAciklama3.Text = cari.Plaka2;



                // ViewModel satış carisini de güncelle
                ViewModel.SatisCari = cari;

                txtNakIsk.Text = cari.NakliyeIskonto.ToString("F2");

                if (ViewModel.IsSatisKrediKartiOdeme)
                {
                    txtIsk1.Text = cari.KKIsk1.ToString("F2");
                    txtIsk2.Text = cari.KKIsk2.ToString("F2");
                    txtIsk3.Text = cari.KKIsk3.ToString("F2");
                    txtIsk4.Text = cari.KKIsk4.ToString("F2");
                }
                else
                {
                    txtIsk1.Text = cari.Isk1.ToString("F2");
                    txtIsk2.Text = cari.Isk2.ToString("F2");
                    txtIsk3.Text = cari.Isk3.ToString("F2");
                    txtIsk4.Text = cari.Isk4.ToString("F2");
                }

                GuncelleIskontoKutulari();

            }
        }

        private void btnTeslimCariListe_Click(object sender, RoutedEventArgs e)
        {
            CariListesiWindow cariListe = new CariListesiWindow();

            bool? result = cariListe.ShowDialog();

            if (result == true && cariListe.SecilenCari != null)
            {
                var cari = cariListe.SecilenCari;

                // Teslimat bilgilerini doldur
                txtTeslimKod.Text = cari.CariKod;
                txtTeslimIsim.Text = cari.CariAdi;
                txtTeslimAdres.Text = cari.Adres;
                txtTeslimTelefon.Text = cari.Telefon;
                txtYetkiliKisi.Text = cari.Yetkili;


                // 🧠 ViewModel'a ata
                ViewModel.TeslimatCari = cari;
            }
        }
        private void btnCariListesi_Click(object sender, RoutedEventArgs e)
        {
            CariListesiWindow cariListe = new CariListesiWindow();
            bool? result = cariListe.ShowDialog();

            if (result == true && cariListe.SecilenCari != null)
            {
                var cari = cariListe.SecilenCari;

                txtCariKod.Text = cari.CariKod;
                txtCariAdGosterim.Text = cari.CariAdi;
                txtCariAdresGosterim.Text = cari.Adres;

                // 🧠 ViewModel'a ata
                ViewModel.AlisCari = cari;
                ViewModel.SatisCari = cari;
            }
        }


        private void btnStokSec_Click(object sender, RoutedEventArgs e)
        {
            StokListeWindow stokPencere = new StokListeWindow();
            bool? result = stokPencere.ShowDialog();

            if (result == true && stokPencere.SecilenStok != null)
            {
                var stok = stokPencere.SecilenStok;

                if (ViewModel.SecilenKalem == null)
                    ViewModel.SecilenKalem = new KalemModel();

                // Stok bilgilerini kaleme aktar
                ViewModel.SecilenKalem.StokKodu = stok.StokKodu;
                ViewModel.SecilenKalem.StokAdi = stok.StokAdi;
                ViewModel.SecilenKalem.Birim = stok.Birim;
                ViewModel.SecilenKalem.KDV = stok.KdvOrani ?? 0;
                ViewModel.SecilenKalem.BirimFiyat = GetAlisFiyat(stok);

                // Stok listesini güncelle (önce boşalt sonra stokları yükle)
                ViewModel.YukleStoklari(stokPencere.TumStoklar);

                // UI güncelle
                txtStokKodu.Text = ViewModel.SecilenKalem.StokKodu;
                txtStokAdi.Text = ViewModel.SecilenKalem.StokAdi;
                txtBirim.Text = ViewModel.SecilenKalem.Birim;
                txtKDV.Text = ViewModel.SecilenKalem.KDV.ToString();
                txtFiyat.Text = ViewModel.SecilenKalem.BirimFiyat.ToString("F2");
                

                HesaplaVeGosterTutar();
                txtMiktar.Focus();
            }
        }

        private int alisKalemIdSayaci = 1;  // Tek sayılar için sayaç
        private int satisKalemIdSayaci = 2; // Çift sayılar için sayaç

        private void btnKalemEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStokKodu.Text))
                return;

            string stokKodu = txtStokKodu.Text;
            string stokAdi = txtStokAdi.Text;
            string birim = txtBirim.Text;

            decimal.TryParse(txtMiktar.Text, out decimal miktar);
            decimal.TryParse(txtKDV.Text, out decimal kdv);
            decimal.TryParse(txtNakIsk.Text, out decimal nakliyeIskontoInput);

            var stok = ViewModel.StokListesi.FirstOrDefault(s => s.StokKodu == stokKodu);
            if (stok == null)
            {
                MessageBox.Show("Stok bilgisi bulunamadı.");
                return;
            }

            // ALIŞ FİYATI
            decimal alisFiyat;
            if (ViewModel.DuzenlenenKalemId == null)
            {
                alisFiyat = GetAlisFiyat(stok);
            }
            else
            {
                decimal.TryParse(txtFiyat.Text, out alisFiyat);
            }

            // SATIŞ FİYATI
            decimal satisFiyat;
            if (ViewModel.DuzenlenenKalemId == null)
            {
                satisFiyat = GetSatisFiyat(stok);
            }
            else
            {
                decimal.TryParse(txtFiyat.Text, out satisFiyat);
            }

            // İskontolar
            decimal.TryParse(txtIsk1.Text, out decimal isk1);
            decimal.TryParse(txtIsk2.Text, out decimal isk2);
            decimal.TryParse(txtIsk3.Text, out decimal isk3);
            decimal.TryParse(txtIsk4.Text, out decimal isk4);

            decimal alisIsk1 = string.IsNullOrWhiteSpace(txtIsk1.Text)
                ? (ViewModel.IsAlisKrediKartiOdeme ? ViewModel.AlisCari?.KKIsk1 ?? 0 : ViewModel.AlisCari?.Isk1 ?? 0)
                : isk1;

            decimal alisIsk2 = string.IsNullOrWhiteSpace(txtIsk2.Text)
                ? (ViewModel.IsAlisKrediKartiOdeme ? ViewModel.AlisCari?.KKIsk2 ?? 0 : ViewModel.AlisCari?.Isk2 ?? 0)
                : isk2;

            decimal alisIsk3 = string.IsNullOrWhiteSpace(txtIsk3.Text)
                ? (ViewModel.IsAlisKrediKartiOdeme ? ViewModel.AlisCari?.KKIsk3 ?? 0 : ViewModel.AlisCari?.Isk3 ?? 0)
                : isk3;

            decimal alisIsk4 = string.IsNullOrWhiteSpace(txtIsk4.Text)
                ? (ViewModel.IsAlisKrediKartiOdeme ? ViewModel.AlisCari?.KKIsk4 ?? 0 : ViewModel.AlisCari?.Isk4 ?? 0)
                : isk4;

            decimal satisIsk1 = string.IsNullOrWhiteSpace(txtIsk1.Text)
                ? (ViewModel.IsSatisKrediKartiOdeme ? ViewModel.SatisCari?.KKIsk1 ?? 0 : ViewModel.SatisCari?.Isk1 ?? 0)
                : isk1;

            decimal satisIsk2 = string.IsNullOrWhiteSpace(txtIsk2.Text)
                ? (ViewModel.IsSatisKrediKartiOdeme ? ViewModel.SatisCari?.KKIsk2 ?? 0 : ViewModel.SatisCari?.Isk2 ?? 0)
                : isk2;

            decimal satisIsk3 = string.IsNullOrWhiteSpace(txtIsk3.Text)
                ? (ViewModel.IsSatisKrediKartiOdeme ? ViewModel.SatisCari?.KKIsk3 ?? 0 : ViewModel.SatisCari?.Isk3 ?? 0)
                : isk3;

            decimal satisIsk4 = string.IsNullOrWhiteSpace(txtIsk4.Text)
                ? (ViewModel.IsSatisKrediKartiOdeme ? ViewModel.SatisCari?.KKIsk4 ?? 0 : ViewModel.SatisCari?.Isk4 ?? 0)
                : isk4;

            // Checkbox durumlarını oku
            bool isAlisFabrikaTeslim = ViewModel.IsAlisFabrikaTeslim;
            bool isSatisFabrikaTeslim = ViewModel.IsSatisFabrikaTeslim;

            // Nakliye iskonto ayarları - DUZENLENEN KALEM YOKSA cari karttan al, varsa inputtan al
            decimal alisNakliyeIskonto = 0;
            decimal satisNakliyeIskonto = 0;

            // Eğer düzenleme modundaysa textbox'tan al
            if (ViewModel.DuzenlenenKalemId != null)
            {
                alisNakliyeIskonto = nakliyeIskontoInput;
                satisNakliyeIskonto = nakliyeIskontoInput;
            }
            else
            {
                alisNakliyeIskonto = ViewModel.IsAlisFabrikaTeslim
                    ? ViewModel.AlisCari?.NakliyeIskonto ?? 0
                    : 0;

                satisNakliyeIskonto = ViewModel.IsSatisFabrikaTeslim
                    ? ViewModel.SatisCari?.NakliyeIskonto ?? 0
                    : 0;
            }


            // Kalem Ekle (ViewModel tarafında ayrı alış ve satış kalemlerine ayırıyorsan buna göre ekle)
            ViewModel.KalemEkle(
                stokKodu,
                stokAdi,
                birim,
                miktar,
                kdv,
                alisIsk1,
                alisIsk2,
                alisIsk3,
                alisIsk4,
                alisFiyat,
                satisFiyat,
                alisNakliyeIskonto,
                satisNakliyeIskonto,
                isAlisFabrikaTeslim,   // Yeni eklendi
                isSatisFabrikaTeslim   // Yeni eklendi
            );

            ClearKalemInputFields();
        }


        private void dgAlisKalemler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(dgAlisKalemler, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null) return;

            if (dgAlisKalemler.SelectedItem is KalemModel kalem)
            {
                ViewModel.SecilenKalem = kalem;
                ViewModel.DuzenlenenKalemId = kalem.Id;
                LoadKalemToInputs(kalem);
            }
        }

        private void dgSatisKalemler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(dgSatisKalemler, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null) return;

            if (dgSatisKalemler.SelectedItem is KalemModel kalem)
            {
                ViewModel.SecilenKalem = kalem;
                ViewModel.DuzenlenenKalemId = kalem.Id;
                LoadKalemToInputs(kalem);
            }
        }


        private void LoadKalemToInputs(KalemModel kalem)
        {
            txtStokKodu.Text = kalem.StokKodu;
            txtStokAdi.Text = kalem.StokAdi;
            txtBirim.Text = kalem.Birim;
            txtMiktar.Text = kalem.Miktar.ToString("F2");
            txtFiyat.Text = kalem.BirimFiyat.ToString("F2");
            txtKDV.Text = kalem.KDV.ToString("F2");

            

            txtNakIsk.Text = kalem.NakliyeIskonto.ToString("F2");


            txtIsk1.Text = kalem.Isk1.ToString("F2");
            txtIsk2.Text = kalem.Isk2.ToString("F2");
            txtIsk3.Text = kalem.Isk3.ToString("F2");
            txtIsk4.Text = kalem.Isk4.ToString("F2");

            txtTutar.Text = kalem.Tutar.ToString("F2");
        }


        private void txtTutar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                btnKalemEkle_Click(null, null);         // Kalem ekle
                e.Handled = true;                       // Tuş işlenmiş say
                ClearKalemInputFields();                // Kutuları temizle
                txtStokKodu.Focus();                    // İlk kutuya odaklan
            }
        }

        private void ClearKalemInputFields()
        {
            txtStokKodu.Text = "";
            txtStokAdi.Text = "";
            txtBirim.Text = "";
            txtMiktar.Text = "";
            txtFiyat.Text = "";
            txtKDV.Text = "";

            if (chkFabrikaTeslim.IsChecked == true && ViewModel.AlisCari != null)
            {
                txtNakIsk.Text = ViewModel.AlisCari.NakliyeIskonto.ToString("F2");
            }
            else
            {
                txtNakIsk.Text = "0";
            }

            if (ViewModel.AlisCari != null)
            {
                if (ViewModel.IsAlisKrediKartiOdeme)
                {
                    txtIsk1.Text = ViewModel.AlisCari.KKIsk1.ToString("F2");
                    txtIsk2.Text = ViewModel.AlisCari.KKIsk2.ToString("F2");
                    txtIsk3.Text = ViewModel.AlisCari.KKIsk3.ToString("F2");
                    txtIsk4.Text = ViewModel.AlisCari.KKIsk4.ToString("F2");
                }
                else
                {
                    txtIsk1.Text = ViewModel.AlisCari.Isk1.ToString("F2");
                    txtIsk2.Text = ViewModel.AlisCari.Isk2.ToString("F2");
                    txtIsk3.Text = ViewModel.AlisCari.Isk3.ToString("F2");
                    txtIsk4.Text = ViewModel.AlisCari.Isk4.ToString("F2");
                }
            }
            else
            {
                txtIsk1.Text = "";
                txtIsk2.Text = "";
                txtIsk3.Text = "";
                txtIsk4.Text = "";
            }

            txtTutar.Text = "";
        }




        private void txtMiktar_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }

        private void txtFiyat_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }

        private void txtIsk1_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }

        private void txtIsk2_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }
        private void txtIsk3_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }
        private void txtIsk4_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }
        private void txtKDV_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }

        private void HesaplaVeGosterTutar()
        {
            if (decimal.TryParse(txtMiktar.Text, out decimal miktar) &&
                decimal.TryParse(txtFiyat.Text, out decimal fiyat) &&
                decimal.TryParse(txtIsk1.Text, out decimal isk1) &&
                decimal.TryParse(txtIsk2.Text, out decimal isk2) &&
                decimal.TryParse(txtIsk3.Text, out decimal isk3) &&
                decimal.TryParse(txtIsk4.Text, out decimal isk4) &&
                decimal.TryParse(txtKDV.Text, out decimal kdv))
            {
                decimal tutar = miktar * fiyat;
                tutar *= (1 - isk1 / 100);
                tutar *= (1 - isk2 / 100);
                tutar *= (1 - isk3 / 100);
                tutar *= (1 - isk4 / 100);
                tutar *= (1 + kdv / 100);
                txtTutar.Text = tutar.ToString("F2");
            }
            else
            {
                txtTutar.Text = "0";
            }
        }

        private void txtCommon_TextChanged(object sender, TextChangedEventArgs e)
        {
            HesaplaVeGosterTutar();
        }

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(((TextBox)sender).Text + e.Text, out _);
        }

        private decimal GetAlisFiyat(StokModel stok)
        {
            if (chkNakit.IsChecked == true)
                return stok.AlisFiyat ?? 0;

            if (chkKrediKarti.IsChecked == true)
                return stok.AlisFiyat2 ?? 0;

            string vade = "";
            if (cmbOdemeSekli.SelectedItem is ComboBoxItem selectedItem)
                vade = selectedItem.Content.ToString();

            switch (vade)
            {
                case "Süt Vade": return stok.AlisFiyat2 ?? 0;
                case "30 Gün": return stok.AlisFiyat3 ?? 0;
                case "45 Gün": return stok.AlisFiyat4 ?? 0;
                case "60 Gün": return stok.AlisFiyat5 ?? 0;
                default: return stok.AlisFiyat ?? 0;
            }
        }

        private decimal GetSatisFiyat(StokModel stok)
        {
            if (chkSatisNakit.IsChecked == true)
                return stok.AlisFiyat ?? 0;
            
            if (chkSatisKrediKarti.IsChecked == true)
                return stok.AlisFiyat2 ?? 0;

            string vade = "";
            if (cmbSatisOdemeSekli.SelectedItem is ComboBoxItem selectedItem)
                vade = selectedItem.Content.ToString();

            switch (vade)
            {
                case "Süt Vade": return stok.AlisFiyat2 ?? 0;
                case "30 Gün": return stok.AlisFiyat3 ?? 0;
                case "45 Gün": return stok.AlisFiyat4 ?? 0;
                case "60 Gün": return stok.AlisFiyat5 ?? 0;
                default: return stok.AlisFiyat ?? 0;
            }
        }
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;

            var grid = sender as DataGrid;
            if (grid == null) return;

            // Seçili satırı al
            var seciliKalem = grid.SelectedItem;
            if (seciliKalem == null) return;

            // Hangi DataGrid olduğunu kontrol et ve ilgili listeyi al
            IList liste = null;
            if (grid == dgAlisKalemler)
                liste = (DataContext as MainViewModel)?.AlisKalemListesi;
            else if (grid == dgSatisKalemler)
                liste = (DataContext as MainViewModel)?.SatisKalemListesi;

            if (liste == null) return;

            // Onay sor
            var sonuc = MessageBox.Show(
                "Seçili kalemi silmek istediğinize emin misiniz?",
                "Onay",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (sonuc == MessageBoxResult.Yes)
            {
                liste.Remove(seciliKalem);
                e.Handled = true; // tuşun başka yere gitmesini önler
            }
        }




        public string GetPdfSavePath(string suggestedFileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Dosyası (*.pdf)|*.pdf",
                FileName = suggestedFileName, // önerilen isim
                Title = "PDF Kaydet"
            };

            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }
        
        public void OlusturSiparisFormuPdf()
        {
            string siparisNo = ViewModel.SiparisNo;
            string dosyaYolu = GetPdfSavePath($"SiparisFormu_{siparisNo}.pdf");
            string belgeKodu = ViewModel.BelgeKodu;

            
            if (string.IsNullOrEmpty(dosyaYolu))
                return; // kullanıcı iptal ettiyse çık

            // Burada PDF’i oluşturacağız (bir sonraki adımda)
            MessageBox.Show("PDF buraya kaydedilecek: " + dosyaYolu);
        }

        public void OlusturProformaFaturaPdf()
        {
            string proformaNo = ViewModel.ProformaNo;
            string dosyaYolu = GetPdfSavePath($"ProformaFatura_{proformaNo}.pdf");
            string belgeKodu = ViewModel.BelgeKodu;

            if (string.IsNullOrEmpty(dosyaYolu))
                return;

            MessageBox.Show("PDF buraya kaydedilecek: " + dosyaYolu);
        }


        private void SiparisFormuPdf_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SiparisNo == "000000000")
            {
                MessageBox.Show("Lütfen önce önizleme yaparak sipariş numarası oluşturun.",
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string siparisNo = ViewModel.SiparisNo;
            string dosyaYolu = GetPdfSavePath($"SiparisFormu_{siparisNo}.pdf");
            string belgeKodu = ViewModel.BelgeKodu;

            if (string.IsNullOrEmpty(dosyaYolu))
                return;

            if (DataContext is MainViewModel vm)
            {
                var kalemler = vm.AlisKalemListesi.ToList();
                var cari = vm.AlisCari;

                var teslimatCari = new CariModel
                {
                    TeslimatAdi = txtTeslimIsim.Text,
                    TeslimatAdres = txtTeslimAdres.Text,
                    TeslimatTelefon = txtTeslimTelefon.Text,
                    TeslimatYetkili = txtYetkiliKisi.Text
                };

                if (!kalemler.Any())
                {
                    MessageBox.Show("PDF oluşturmak için en az bir kalem eklenmelidir.");
                    return;
                }

                if (cari == null)
                {
                    MessageBox.Show("Lütfen geçerli bir cari seçiniz.");
                    return;
                }

                // ViewModel bilgilerini doldur
                vm.SiparisTarihi = dtSiparisTarihi.SelectedDate ?? DateTime.Now;
                vm.SevkTarihi = dtSevkTarihi.SelectedDate ?? DateTime.Now.AddDays(3);
                vm.Aciklama1 = txtAciklama1.Text;
                vm.Aciklama2 = txtAciklama2.Text;
                vm.Aciklama3 = txtAciklama3.Text;
                vm.Aciklama4 = txtProAciklama1.Text;
                vm.Aciklama5 = txtProAciklama2.Text;
                vm.Aciklama6 = txtProAciklama3.Text;
                vm.IsFabrikaTeslim = chkproFabrikaTeslim.IsChecked == true;

                if (chkNakit.IsChecked == true)
                    vm.OdemeYontemi = "Ön Ödeme";
                else if (chkKrediKarti.IsChecked == true)
                    vm.OdemeYontemi = "Kredi Kartı";
                else if (cmbOdemeSekli.SelectedItem != null)
                    vm.OdemeYontemi = cmbOdemeSekli.Text;
                else
                    vm.OdemeYontemi = "Belirtilmedi";

                try
                {
                    PdfGenerator.OlusturSiparisFormu(kalemler, cari, teslimatCari, dosyaYolu, siparisNo, belgeKodu, vm);
                    MessageBox.Show($"PDF başarıyla kaydedildi:\n{dosyaYolu}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturulurken hata oluştu:\n{ex.Message}");
                }
            }
        }



        private void ProformaFaturaPdf_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ProformaNo == "000000000")
            {
                MessageBox.Show("Lütfen önce önizleme yaparak proforma numarası oluşturun.",
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string proformaNo = ViewModel.ProformaNo;
            string dosyaYolu = GetPdfSavePath($"ProformaFatura_{proformaNo}.pdf");
            string belgeKodu = ViewModel.BelgeKodu;

            if (string.IsNullOrEmpty(dosyaYolu))
                return;

            if (DataContext is MainViewModel vm)
            {
                var kalemler = vm.SatisKalemListesi.ToList();
                var cari = vm.SatisCari;

                var teslimatCari = new CariModel
                {
                    TeslimatAdi = txtTeslimIsim.Text,
                    TeslimatAdres = txtTeslimAdres.Text,
                    TeslimatTelefon = txtTeslimTelefon.Text,
                    TeslimatYetkili = txtYetkiliKisi.Text
                };

                if (!kalemler.Any())
                {
                    MessageBox.Show("PDF oluşturmak için en az bir kalem eklenmelidir.");
                    return;
                }

                if (cari == null)
                {
                    MessageBox.Show("Lütfen geçerli bir cari seçiniz.");
                    return;
                }

                vm.IsFabrikaTeslim = chkproFabrikaTeslim.IsChecked == true;

                if (chkSatisNakit.IsChecked == true)
                    vm.ProformaOdemeYontemi = "Ön Ödeme";
                else if (chkSatisKrediKarti.IsChecked == true)
                    vm.ProformaOdemeYontemi = "Kredi Kartı";
                else if (cmbSatisOdemeSekli.SelectedItem != null)
                    vm.ProformaOdemeYontemi = cmbSatisOdemeSekli.Text;
                else
                    vm.ProformaOdemeYontemi = "Belirtilmedi";

                try
                {
                    PdfGenerator.OlusturProformaFatura(kalemler, cari, teslimatCari, dosyaYolu, proformaNo, belgeKodu, vm);
                    MessageBox.Show($"PDF başarıyla kaydedildi:\n{dosyaYolu}");                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturulurken hata oluştu:\n{ex.Message}");
                }
            }
        }

        private void Onizleme_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var alisKalemler = vm.AlisKalemListesi.ToList();
                var satisKalemler = vm.SatisKalemListesi.ToList();

                var alisCari = vm.AlisCari;
                var satisCari = vm.SatisCari;

                if (!alisKalemler.Any() || alisCari == null ||
                    !satisKalemler.Any() || satisCari == null)
                {
                    MessageBox.Show("Lütfen hem alış hem satış bilgilerini doldurunuz.");
                    return;
                }

                var teslimatCari = new CariModel
                {
                    TeslimKod = txtTeslimKod.Text,
                    TeslimatAdi = txtTeslimIsim.Text,
                    TeslimatAdres = txtTeslimAdres.Text,
                    TeslimatTelefon = txtTeslimTelefon.Text,
                    TeslimatYetkili = txtYetkiliKisi.Text
                };

                vm.SiparisTarihi = dtSiparisTarihi.SelectedDate ?? DateTime.Now;
                vm.SevkTarihi = dtSevkTarihi.SelectedDate ?? DateTime.Now.AddDays(3);
                vm.Aciklama1 = txtAciklama1.Text;
                vm.Aciklama2 = txtAciklama2.Text;
                vm.Aciklama3 = txtAciklama3.Text;
                vm.Aciklama4 = txtProAciklama1.Text;
                vm.Aciklama5 = txtProAciklama2.Text;
                vm.Aciklama6 = txtProAciklama3.Text;


                vm.OdemeYontemi = chkNakit.IsChecked == true ? "Ön Ödeme" :
                                  chkKrediKarti.IsChecked == true ? "Kredi Kartı" :
                                  cmbOdemeSekli.SelectedItem != null ? cmbOdemeSekli.Text : "Belirtilmedi";

                vm.ProformaOdemeYontemi = chkSatisNakit.IsChecked == true ? "Ön Ödeme" :
                                          chkSatisKrediKarti.IsChecked == true ? "Kredi Kartı" :
                                          cmbSatisOdemeSekli.SelectedItem != null ? cmbSatisOdemeSekli.Text : "Belirtilmedi";

                // ✅ Önce numaraları üret
                if (ViewModel.SiparisNo == "000000000")
                    ViewModel.SiparisNo = NumaratorService.GenerateIncrementalCode("S");

                if (ViewModel.ProformaNo == "000000000")
                    ViewModel.ProformaNo = NumaratorService.GenerateIncrementalCode("P");
                
                if (vm.BelgeKodu == "BELGE_YOK")
                    vm.BelgeKodu = BelgeKodUretici.GenerateBelgeKodu();

                // ✅ Artık güncellenmiş numaraları kullan
                string siparisNo = ViewModel.SiparisNo;
                string proformaNo = ViewModel.ProformaNo;
                string belgekodu = ViewModel.BelgeKodu;
               
                if (vm.BelgeKodu == "BELGE_YOK")
                    vm.BelgeKodu = BelgeKodUretici.GenerateBelgeKodu();
                
                try
                {
                    PdfGenerator.OlusturOnizlemeBelgesi(alisKalemler, alisCari, satisKalemler, satisCari,
                                                        teslimatCari, siparisNo, proformaNo, belgekodu, vm);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Önizleme sırasında hata oluştu:\n" + ex.Message);
                }
            }
        }

        private bool _kaydederekKapaniyor = false;

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (vm != null)
            {
                // Gerekli kontroller
                if (vm.SiparisNo == "000000000" || vm.ProformaNo == "000000000")
                {
                    MessageBox.Show("Lütfen önce önizleme yaparak numaraları oluşturun.",
                                    "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kullanıcıya "Emin misiniz?" sorusu
                var sonuc = MessageBox.Show("Siparişi kaydetmek istediğinizden emin misiniz?",
                                            "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (sonuc != MessageBoxResult.Yes)
                {
                    return;
                }

                // Kayıt işlemi
                bool basarili = await vm.SiparisiKaydetAsync();

                if (basarili)
                {
                    MessageBox.Show("Sipariş başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

                    _kaydederekKapaniyor = true; // ❗ Kaydetme nedeniyle kapanıyor                                                
                   
                    this.Close(); // Bu pencereyi kapatır


                    // Yeni, boş bir pencere aç
                    var yeniForm = new SiparisFormu(); // Sipariş formunun class adı
                    yeniForm.Show();
                }
                else
                {
                    MessageBox.Show("Sipariş kaydedilemedi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Eğer kaydetme nedeniyle kapanıyorsa uyarı gösterme
            if (_kaydederekKapaniyor)
            {
                return;
            }

            var vm = DataContext as MainViewModel;

            if (vm != null && FormDegistiMi(vm))
            {
                var result = MessageBox.Show(
                    "Girmiş olduğunuz bilgiler kaydedilmedi.\n" +
                    "Çıkmak istediğinize emin misiniz?",
                    "Uyarı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true; // Çıkışı iptal et
                }
            }
        }
        private bool FormDegistiMi(MainViewModel vm)
        {
            bool alisCariGirildi = vm.AlisCari != null &&
                (!string.IsNullOrWhiteSpace(vm.AlisCari.CariKod) ||
                 !string.IsNullOrWhiteSpace(vm.AlisCari.CariAdi));

            bool satisCariGirildi = vm.SatisCari != null &&
                (!string.IsNullOrWhiteSpace(vm.SatisCari.CariKod) ||
                 !string.IsNullOrWhiteSpace(vm.SatisCari.CariAdi));

            return alisCariGirildi || satisCariGirildi;
        }


        public static class BelgeKodUretici
        {
            public static string GenerateBelgeKodu()
            {
                return $"B{DateTime.Now:yyMMddHHmmssfff}";
                // Örnek çıktı: B2510091659312
            }
        }



    }
}