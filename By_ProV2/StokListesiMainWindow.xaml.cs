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
    public partial class StokListesiMainWindow : Window
    {
        private ObservableCollection<StokModel> tumStoklar;
        private ObservableCollection<StokModel> goruntulenenStoklar;

        public StokListesiMainWindow()
        {
            InitializeComponent();
            tumStoklar = new ObservableCollection<StokModel>();
            goruntulenenStoklar = new ObservableCollection<StokModel>();
            dataGridStoklar.ItemsSource = goruntulenenStoklar;
            YukleStoklar();
        }

        private void YukleStoklar()
        {
            try
            {
                string connStr = ConfigurationHelper.GetConnectionString("db");
                string query = @"
                    SELECT
                        S.STOKID,
                        S.STOKKODU,
                        S.STOKADI,
                        S.BIRIM,
                        S.AGIRLIK,
                        S.PROTEIN,
                        S.ENERJI,
                        S.NEM,
                        S.BARKOD,
                        S.YEMOZELLIGI AS YemOzelligi,
                        S.ACIKLAMA,
                        S.MENSEI,
                        S.AKTIF,
                        S.OLUSTURMATARIHI,

                        F.ALISFIYAT1 AS AlisFiyat,
                        F.ALISFIYAT2 AS AlisFiyat2,
                        F.ALISFIYAT3 AS AlisFiyat3,
                        F.ALISFIYAT4 AS AlisFiyat4,
                        F.ALISFIYAT5 AS AlisFiyat5,
                        F.KDVORANI AS KdvOrani,
                        F.PARABIRIMI AS ParaBirimi,
                        F.LISTETARIHI AS ListeTarihi,

                        B.DOSYAYOLU AS DosyaYolu,

                        H.MIKTAR AS Miktar,
                        H.DEPOID AS DepoId,
                        H.ISLEMTARIHI AS IslemTarihi

                    FROM STOKSABITKART S

                    LEFT JOIN (
                        SELECT STOKID, ALISFIYAT1, ALISFIYAT2, ALISFIYAT3, ALISFIYAT4, ALISFIYAT5, KDVORANI, PARABIRIMI, LISTETARIHI 
                        FROM STOKSABITFIYAT F1 
                        WHERE LISTETARIHI = (
                            SELECT MAX(LISTETARIHI) 
                            FROM STOKSABITFIYAT F2 
                            WHERE F2.STOKID = F1.STOKID
                        )
                    ) F ON F.STOKID = S.STOKID

                    LEFT JOIN (
                        SELECT * FROM (
                            SELECT STOKID, DOSYAYOLU, ROW_NUMBER() OVER (PARTITION BY STOKID ORDER BY BELGEID DESC) AS RN 
                            FROM STOKSABITBELGE
                        ) Belge 
                        WHERE RN = 1
                    ) B ON B.STOKID = S.STOKID 

                    LEFT JOIN (
                        SELECT STOKID, MIKTAR, DEPOID, ISLEMTARIHI 
                        FROM STOKSABITHAREKET H1 
                        WHERE H1.ISLEMTARIHI = (
                            SELECT MAX(ISLEMTARIHI) 
                            FROM STOKSABITHAREKET H2 
                            WHERE H2.STOKID = H1.STOKID
                        )
                    ) H ON H.STOKID = S.STOKID
                    ORDER BY S.STOKKODU";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        tumStoklar.Clear();
                        while (reader.Read())
                        {
                            var stok = new StokModel
                            {
                                STOKID = reader.GetInt32(reader.GetOrdinal("STOKID")),
                                StokKodu = reader["STOKKODU"]?.ToString(),
                                StokAdi = reader["STOKADI"]?.ToString(),
                                Birim = reader["BIRIM"]?.ToString(),
                                Agirlik = reader.IsDBNull(reader.GetOrdinal("AGIRLIK")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AGIRLIK")),
                                Protein = reader.IsDBNull(reader.GetOrdinal("PROTEIN")) ? 0 : reader.GetDecimal(reader.GetOrdinal("PROTEIN")),
                                Enerji = reader.IsDBNull(reader.GetOrdinal("ENERJI")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ENERJI")),
                                Nem = reader.IsDBNull(reader.GetOrdinal("NEM")) ? 0 : reader.GetDecimal(reader.GetOrdinal("NEM")),
                                Barkod = reader["BARKOD"]?.ToString(),
                                YemOzelligi = reader["YemOzelligi"]?.ToString(),
                                Aciklama = reader["ACIKLAMA"]?.ToString(),
                                Mensei = reader["MENSEI"]?.ToString(),
                                Aktif = reader.GetBoolean(reader.GetOrdinal("AKTIF")),
                                OlusturmaTarihi = reader.GetDateTime(reader.GetOrdinal("OLUSTURMATARIHI")),

                                AlisFiyat = reader.IsDBNull(reader.GetOrdinal("AlisFiyat")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AlisFiyat")),
                                AlisFiyat2 = reader.IsDBNull(reader.GetOrdinal("AlisFiyat2")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AlisFiyat2")),
                                AlisFiyat3 = reader.IsDBNull(reader.GetOrdinal("AlisFiyat3")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AlisFiyat3")),
                                AlisFiyat4 = reader.IsDBNull(reader.GetOrdinal("AlisFiyat4")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AlisFiyat4")),
                                AlisFiyat5 = reader.IsDBNull(reader.GetOrdinal("AlisFiyat5")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AlisFiyat5")),
                                KdvOrani = reader.IsDBNull(reader.GetOrdinal("KdvOrani")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("KdvOrani")),
                                ParaBirimi = reader["ParaBirimi"]?.ToString(),
                                ListeTarihi = reader.IsDBNull(reader.GetOrdinal("ListeTarihi")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ListeTarihi")),

                                DosyaYolu = reader["DosyaYolu"]?.ToString(),
                                Miktar = reader.IsDBNull(reader.GetOrdinal("Miktar")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Miktar")),
                                DepoId = reader.IsDBNull(reader.GetOrdinal("DepoId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("DepoId")),
                                IslemTarihi = reader.IsDBNull(reader.GetOrdinal("IslemTarihi")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("IslemTarihi"))
                            };
                            tumStoklar.Add(stok);
                        }
                    }
                }
                
                // Tüm stokları göster
                goruntulenenStoklar.Clear();
                foreach (var stok in tumStoklar)
                {
                    goruntulenenStoklar.Add(stok);
                }
                
                GuncelleToplamKayit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stok listesi yüklenirken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAra_Click(object sender, RoutedEventArgs e)
        {
            FiltreleStokListesi();
        }

        private void BtnTemizle_Click(object sender, RoutedEventArgs e)
        {
            // Tüm textbox'ları temizle
            txtStokKodu.Text = "";
            txtStokAdi.Text = "";
            txtBarkod.Text = "";
            txtBirim.Text = "";
            txtAgirlik.Text = "";
            txtProtein.Text = "";
            txtAciklama.Text = "";
            txtMensei.Text = "";
            
            // Yeniden filtrele (tüm listeyi göster)
            FiltreleStokListesi();
        }

        private void FiltreleStokListesi()
        {
            string stokKodu = txtStokKodu.Text.ToLower().Trim();
            string stokAdi = txtStokAdi.Text.ToLower().Trim();
            string barkod = txtBarkod.Text.ToLower().Trim();
            string birim = txtBirim.Text.ToLower().Trim();
            string agirlik = txtAgirlik.Text.ToLower().Trim();
            string protein = txtProtein.Text.ToLower().Trim();
            string aciklama = txtAciklama.Text.ToLower().Trim();
            string mensei = txtMensei.Text.ToLower().Trim();

            goruntulenenStoklar.Clear();

            foreach (var stok in tumStoklar)
            {
                bool eslesme = true;

                if (!string.IsNullOrEmpty(stokKodu) && 
                    !stok.StokKodu.ToLower().Contains(stokKodu))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(stokAdi) && 
                    !stok.StokAdi.ToLower().Contains(stokAdi))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(barkod) && 
                    !stok.Barkod.ToLower().Contains(barkod))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(birim) && 
                    !stok.Birim.ToLower().Contains(birim))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(agirlik) && 
                    !stok.Agirlik.ToString().ToLower().Contains(agirlik))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(protein) && 
                    !stok.Protein.ToString().ToLower().Contains(protein))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(aciklama) && 
                    !stok.Aciklama.ToLower().Contains(aciklama))
                    eslesme = false;
                    
                if (!string.IsNullOrEmpty(mensei) && 
                    !stok.Mensei.ToLower().Contains(mensei))
                    eslesme = false;

                if (eslesme)
                {
                    goruntulenenStoklar.Add(stok);
                }
            }

            GuncelleToplamKayit();
        }

        private void GuncelleToplamKayit()
        {
            lblToplamKayit.Text = $"Toplam: {goruntulenenStoklar.Count} Kayıt";
        }

        private void DataGridStoklar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridStoklar.SelectedItem is StokModel secilenStok)
            {
                StokKayitWindow stokDuzenlemeForm = new StokKayitWindow();
                stokDuzenlemeForm.LoadStokData(secilenStok);
                stokDuzenlemeForm.ShowDialog();
                
                // Liste güncellensin diye yeniden yükle
                YukleStoklar();
            }
        }

        private void DataGridStoklar_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseDoubleClick += DataGridRow_MouseDoubleClick;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridStoklar.SelectedItem is StokModel secilenStok)
            {
                StokKayitWindow stokDuzenlemeForm = new StokKayitWindow();
                stokDuzenlemeForm.LoadStokData(secilenStok);
                stokDuzenlemeForm.ShowDialog();
                
                // Liste güncellensin diye yeniden yükle
                YukleStoklar();
            }
        }

        private void MenuItemDetaylariGoster_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridStoklar.SelectedItem is StokModel secilenStok)
            {
                // Burada detay penceresi açılabilir
                // Şimdilik sadece bir mesaj gösterelim
                MessageBox.Show($"Seçilen Stok: {secilenStok.StokKodu} - {secilenStok.StokAdi}\n" +
                               $"Birim: {secilenStok.Birim}\n" +
                               $"Ağırlık: {secilenStok.Agirlik}\n" +
                               $"Barkod: {secilenStok.Barkod}\n" +
                               $"Açıklama: {secilenStok.Aciklama}", 
                               "Stok Detay", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItemKopyala_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridStoklar.SelectedItem is StokModel secilenStok)
            {
                string kopyalamaMetni = $"{secilenStok.StokKodu}\t{secilenStok.StokAdi}\t{secilenStok.Birim}";
                Clipboard.SetText(kopyalamaMetni);
                MessageBox.Show("Stok bilgileri panoya kopyalandı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}