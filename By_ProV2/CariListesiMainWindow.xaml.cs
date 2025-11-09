using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2
{
    public partial class CariListesiMainWindow : Window
    {
        private ObservableCollection<CariModel> tumCariler;
        private ObservableCollection<CariModel> goruntulenenCariler;

        public CariListesiMainWindow()
        {
            InitializeComponent();
            tumCariler = new ObservableCollection<CariModel>();
            goruntulenenCariler = new ObservableCollection<CariModel>();
            dataGridCariler.ItemsSource = goruntulenenCariler;
            YukleCariler();
        }

        private void YukleCariler()
        {
            try
            {
                string connStr = ConfigurationHelper.GetConnectionString("db");
                string query = @"SELECT CARIKOD, CARIADI, ADRES, TELEFON, YETKILIKISI, BAGLICARIKOD, 
                                VERGIDAIRESI, VERGINO, ISK1, ISK2, ISK3, ISK4, KKISK1, KKISK2, KKISK3, KKISK4, 
                                NAKISK, PLAKA1, PLAKA2, PLAKA3, SOFORADSOYAD, KAYITTARIHI, SUTFIYATI, NAKFIYATI 
                                FROM CASABIT ORDER BY CARIKOD";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        tumCariler.Clear();
                        while (reader.Read())
                        {
                            var cari = new CariModel
                            {
                                CariKod = reader["CARIKOD"]?.ToString(),
                                CariAdi = reader["CARIADI"]?.ToString(),
                                Adres = reader["ADRES"]?.ToString(),
                                Telefon = reader["TELEFON"]?.ToString(),
                                Yetkili = reader["YETKILIKISI"]?.ToString(),
                                BagliCariKod = reader["BAGLICARIKOD"]?.ToString(),
                                VergiDairesi = reader["VERGIDAIRESI"]?.ToString(),
                                VergiNo = reader["VERGINO"]?.ToString(),
                                Isk1 = reader["ISK1"] != DBNull.Value ? Convert.ToDecimal(reader["ISK1"]) : 0,
                                Isk2 = reader["ISK2"] != DBNull.Value ? Convert.ToDecimal(reader["ISK2"]) : 0,
                                Isk3 = reader["ISK3"] != DBNull.Value ? Convert.ToDecimal(reader["ISK3"]) : 0,
                                Isk4 = reader["ISK4"] != DBNull.Value ? Convert.ToDecimal(reader["ISK4"]) : 0,
                                KKIsk1 = reader["KKISK1"] != DBNull.Value ? Convert.ToDecimal(reader["KKISK1"]) : 0,
                                KKIsk2 = reader["KKISK2"] != DBNull.Value ? Convert.ToDecimal(reader["KKISK2"]) : 0,
                                KKIsk3 = reader["KKISK3"] != DBNull.Value ? Convert.ToDecimal(reader["KKISK3"]) : 0,
                                KKIsk4 = reader["KKISK4"] != DBNull.Value ? Convert.ToDecimal(reader["KKISK4"]) : 0,
                                NakliyeIskonto = reader["NAKISK"] != DBNull.Value ? Convert.ToDecimal(reader["NAKISK"]) : 0,
                                Plaka1 = reader["PLAKA1"]?.ToString(),
                                Plaka2 = reader["PLAKA2"]?.ToString(),
                                Plaka3 = reader["PLAKA3"]?.ToString(),
                                SoforAdSoyad = reader["SOFORADSOYAD"]?.ToString(),
                                KayitTarihi = reader["KAYITTARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["KAYITTARIHI"]) : DateTime.MinValue,
                                SutFiyati = reader["SUTFIYATI"] != DBNull.Value ? Convert.ToDecimal(reader["SUTFIYATI"]) : 0,
                                NakliyeFiyati = reader["NAKFIYATI"] != DBNull.Value ? Convert.ToDecimal(reader["NAKFIYATI"]) : 0
                            };
                            tumCariler.Add(cari);
                        }
                    }
                }
                
                // Tüm carileri göster
                goruntulenenCariler.Clear();
                foreach (var cari in tumCariler)
                {
                    goruntulenenCariler.Add(cari);
                }
                
                GuncelleToplamKayit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cari listesi yüklenirken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAra_Click(object sender, RoutedEventArgs e)
        {
            FiltreleCariListesi();
        }

        private void BtnTemizle_Click(object sender, RoutedEventArgs e)
        {
            // Tüm textbox'ları temizle
            txtCariKodu.Text = "";
            txtCariAdi.Text = "";
            txtVergiNo.Text = "";
            txtTelefon.Text = "";
            txtVergiDairesi.Text = "";
            txtYetkili.Text = "";
            txtAdres.Text = "";
            txtPlaka1.Text = "";
            
            // Yeniden filtrele (tüm listeyi göster)
            FiltreleCariListesi();
        }

        private void FiltreleCariListesi()
        {
            string cariKod = txtCariKodu.Text.ToLower().Trim();
            string cariAdi = txtCariAdi.Text.ToLower().Trim();
            string vergiNo = txtVergiNo.Text.ToLower().Trim();
            string telefon = txtTelefon.Text.ToLower().Trim();
            string vergiDairesi = txtVergiDairesi.Text.ToLower().Trim();
            string yetkili = txtYetkili.Text.ToLower().Trim();
            string adres = txtAdres.Text.ToLower().Trim();
            string plaka1 = txtPlaka1.Text.ToLower().Trim();

            goruntulenenCariler.Clear();

            foreach (var cari in tumCariler)
            {
                bool eslesme = true;

                if (!string.IsNullOrEmpty(cariKod) && 
                    !cari.CariKod.ToLower().Contains(cariKod))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(cariAdi) && 
                    !cari.CariAdi.ToLower().Contains(cariAdi))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(vergiNo) && 
                    !cari.VergiNo.ToLower().Contains(vergiNo))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(telefon) && 
                    !cari.Telefon.ToLower().Contains(telefon))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(vergiDairesi) && 
                    !cari.VergiDairesi.ToLower().Contains(vergiDairesi))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(yetkili) && 
                    !cari.Yetkili.ToLower().Contains(yetkili))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(adres) && 
                    !cari.Adres.ToLower().Contains(adres))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(plaka1) && 
                    !cari.Plaka1.ToLower().Contains(plaka1))
                    eslesme = false;

                if (eslesme)
                {
                    goruntulenenCariler.Add(cari);
                }
            }

            GuncelleToplamKayit();
        }

        private void GuncelleToplamKayit()
        {
            lblToplamKayit.Text = $"Toplam: {goruntulenenCariler.Count} Kayıt";
        }

        private void DataGridCariler_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridCariler.SelectedItem is CariModel secilenCari)
            {
                CariKayitWindow cariDuzenlemeForm = new CariKayitWindow();
                cariDuzenlemeForm.LoadCariData(secilenCari);
                cariDuzenlemeForm.ShowDialog();
                
                // Liste güncellensin diye yeniden yükle
                YukleCariler();
            }
        }

        private void DataGridCariler_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseDoubleClick += DataGridRow_MouseDoubleClick;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridCariler.SelectedItem is CariModel secilenCari)
            {
                CariKayitWindow cariDuzenlemeForm = new CariKayitWindow();
                cariDuzenlemeForm.LoadCariData(secilenCari);
                cariDuzenlemeForm.ShowDialog();
                
                // Liste güncellensin diye yeniden yükle
                YukleCariler();
            }
        }

        private void MenuItemDetaylariGoster_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridCariler.SelectedItem is CariModel secilenCari)
            {
                // Burada detay penceresi açılabilir
                // Şimdilik sadece bir mesaj gösterelim
                MessageBox.Show($"Seçilen Cari: {secilenCari.CariKod} - {secilenCari.CariAdi}\n" +
                               $"Vergi No: {secilenCari.VergiNo}\n" +
                               $"Telefon: {secilenCari.Telefon}\n" +
                               $"Adres: {secilenCari.Adres}", 
                               "Cari Detay", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItemKopyala_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridCariler.SelectedItem is CariModel secilenCari)
            {
                string kopyalamaMetni = $"{secilenCari.CariKod}\t{secilenCari.CariAdi}\t{secilenCari.Telefon}";
                Clipboard.SetText(kopyalamaMetni);
                MessageBox.Show("Cari bilgileri panoya kopyalandı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}