using System;
using System.Collections;
using System.Configuration;
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
    public partial class EskiSiparisFormu : Window
    {
        public string BelgeKodu { get; set; }

        private MainViewModel ViewModel => DataContext as MainViewModel;

        public EskiSiparisFormu()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Loaded += EskiSiparisFormu_Loaded;

        }

        private async void EskiSiparisFormu_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(BelgeKodu))
            {
                await ViewModel.SiparisiYukleAsync(BelgeKodu);
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
            bool miktarOk = decimal.TryParse(txtMiktar.Text, out decimal miktar);
            bool fiyatOk = decimal.TryParse(txtFiyat.Text, out decimal fiyat);
            bool isk1Ok = decimal.TryParse(txtIsk1.Text, out decimal isk1);
            bool isk2Ok = decimal.TryParse(txtIsk2.Text, out decimal isk2);
            bool isk3Ok = decimal.TryParse(txtIsk3.Text, out decimal isk3);
            bool isk4Ok = decimal.TryParse(txtIsk4.Text, out decimal isk4);
            bool kdvOk = decimal.TryParse(txtKDV.Text, out decimal kdv);
            bool nakliyeOk = decimal.TryParse(txtNakIsk.Text, out decimal nakliyeIskonto);

            if (!miktarOk || !fiyatOk)
            {
                txtTutar.Text = "0,00";
                return;
            }

            // Varsayılanlar
            isk1 = isk1Ok ? isk1 : 0;
            isk2 = isk2Ok ? isk2 : 0;
            isk3 = isk3Ok ? isk3 : 0;
            isk4 = isk4Ok ? isk4 : 0;
            kdv = kdvOk ? kdv : 0;
            nakliyeIskonto = nakliyeOk ? nakliyeIskonto : 0;

            decimal tutar = miktar * fiyat;

            // İskontolar
            tutar *= (1 - isk1 / 100m);
            tutar *= (1 - isk2 / 100m);
            tutar *= (1 - isk3 / 100m);
            tutar *= (1 - isk4 / 100m);

            // ✔ Nakliye iskontosu yalnızca fabrika teslim ise uygulanır
            if (chkFabrikaTeslim.IsChecked == true || chkproFabrikaTeslim.IsChecked == true)
            {
                tutar *= (1 - nakliyeIskonto / 100m);
            }


            // KDV
            tutar *= (1 + kdv / 100m);

            // Sonuç gösterimi
            txtTutar.Text = Math.Round(tutar, 2).ToString("N2");
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

        private async void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (vm == null || vm.SeciliSiparisID <= 0)
            {
                MessageBox.Show("Güncellenecek sipariş bulunamadı.", "Uyarı",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool sonuc = await vm.SiparisiGuncelleAsync(vm.SeciliSiparisID);

            if (sonuc)
            {
                MessageBox.Show("Sipariş başarıyla güncellendi.");
            }
            else
            {
                MessageBox.Show("Sipariş güncellenemedi.");
            }
        }
        private async void btnSil_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (vm == null || vm.SeciliSiparisID <= 0)
            {
                MessageBox.Show("Silinecek sipariş bulunamadı.", "Uyarı",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sonuc = MessageBox.Show("Bu siparişi silmek istediğinize emin misiniz?",
                                        "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (sonuc != MessageBoxResult.Yes)
                return;

            bool silindi = await vm.SiparisiSilAsync(vm.SeciliSiparisID);

            if (silindi)
            {
                MessageBox.Show("Sipariş başarıyla silindi.");
                this.Close(); // veya listeye geri dön
            }
            else
            {
                MessageBox.Show("Sipariş silinemedi.");
            }
        }
        public string GetPdfSavePath(string suggestedFileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Dosyası (*.pdf)|*.pdf",
                FileName = suggestedFileName,
                Title = "PDF Kaydet"
            };

            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }
        private void SiparisFormuPdf_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                string siparisNo = vm.SiparisNo;
                string belgeKodu = vm.BelgeKodu;
                string dosyaYolu = GetPdfSavePath($"SiparisFormu_{siparisNo}.pdf");

                if (string.IsNullOrEmpty(dosyaYolu))
                    return;

                var kalemler = vm.AlisKalemListesi.ToList();
                var cari = vm.AlisCari;
                var teslimatCari = new CariModel
                {
                    TeslimatAdi = txtTeslimIsim.Text,
                    TeslimatAdres = txtTeslimAdres.Text,
                    TeslimatTelefon = txtTeslimTelefon.Text,
                    TeslimatYetkili = txtYetkiliKisi.Text
                };
                
                vm.IsFabrikaTeslim = chkproFabrikaTeslim.IsChecked == true;

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
            if (DataContext is MainViewModel vm)
            {
                string proformaNo = vm.ProformaNo;
                string belgeKodu = vm.BelgeKodu;
                string dosyaYolu = GetPdfSavePath($"ProformaFatura_{proformaNo}.pdf");

                if (string.IsNullOrEmpty(dosyaYolu))
                    return;

                var kalemler = vm.SatisKalemListesi.ToList();
                var cari = vm.SatisCari;
                var teslimatCari = new CariModel
                {
                    TeslimatAdi = txtTeslimIsim.Text,
                    TeslimatAdres = txtTeslimAdres.Text,
                    TeslimatTelefon = txtTeslimTelefon.Text,
                    TeslimatYetkili = txtYetkiliKisi.Text
                };

                vm.IsFabrikaTeslim = chkproFabrikaTeslim.IsChecked == true;

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
                var teslimatCari = new CariModel
                {
                    TeslimatAdi = txtTeslimIsim.Text,
                    TeslimatAdres = txtTeslimAdres.Text,
                    TeslimatTelefon = txtTeslimTelefon.Text,
                    TeslimatYetkili = txtYetkiliKisi.Text
                };

                if (!alisKalemler.Any() || alisCari == null ||
                    !satisKalemler.Any() || satisCari == null)
                {
                    MessageBox.Show("Lütfen hem alış hem satış bilgilerini doldurunuz.");
                    return;
                }
                
                vm.OdemeYontemi = chkNakit.IsChecked == true ? "Ön Ödeme" :
                                 chkKrediKarti.IsChecked == true ? "Kredi Kartı" :
                                 cmbOdemeSekli.SelectedItem != null ? cmbOdemeSekli.Text : "Belirtilmedi";

                vm.ProformaOdemeYontemi = chkSatisNakit.IsChecked == true ? "Ön Ödeme" :
                                          chkSatisKrediKarti.IsChecked == true ? "Kredi Kartı" :
                                          cmbSatisOdemeSekli.SelectedItem != null ? cmbSatisOdemeSekli.Text : "Belirtilmedi";
                try
                {
                    PdfGenerator.OlusturOnizlemeBelgesi(alisKalemler, alisCari, satisKalemler, satisCari,
                                                        teslimatCari, vm.SiparisNo, vm.ProformaNo, vm.BelgeKodu, vm);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Önizleme sırasında hata oluştu:\n" + ex.Message);
                }
            }
        }

    }

}