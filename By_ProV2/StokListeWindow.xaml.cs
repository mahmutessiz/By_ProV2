using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using By_ProV2.Helpers;

namespace By_ProV2
{
    public partial class StokListeWindow : Window
    {
        private List<StokModel> tumStoklar;
        public List<StokModel> TumStoklar => tumStoklar;

        // Seçilen stok bilgisi dışarıdan erişilebilir olacak
        public StokModel SecilenStok { get; set; }

        public StokListeWindow()
        {
            InitializeComponent();
            VerileriYukle();
        }

        private void VerileriYukle()
        {
            string connStr = ConfigurationHelper.GetConnectionString("db");
            tumStoklar = new List<StokModel>();

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
                ) H ON H.STOKID = S.STOKID;";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
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

            dataGridStoklar.ItemsSource = tumStoklar;
        }

        private void txtAraStokKod_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrele();
        }

        private void txtAraStokAdi_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrele();
        }

        private void Filtrele()
        {
            string kod = txtAraStokKod.Text.ToLower();
            string adi = txtAraStokAdi.Text.ToLower();

            var filtreli = tumStoklar.Where(s =>
                (string.IsNullOrEmpty(kod) || s.StokKodu.ToLower().Contains(kod)) &&
                (string.IsNullOrEmpty(adi) || s.StokAdi.ToLower().Contains(adi))
            ).ToList();

            dataGridStoklar.ItemsSource = filtreli;
        }

        private void dataGridStoklar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridStoklar.SelectedItem is StokModel secilen)
            {
                SecilenStok = secilen;
                DialogResult = true;
                Close();
            }
        }

        private void dataGridStoklar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && dataGridStoklar.SelectedItem is StokModel secilen)
            {
                SecilenStok = secilen;
                DialogResult = true;
                Close();
            }
        }
    }

    public class StokModel
    {
        public int STOKID { get; set; }
        public string StokKodu { get; set; }
        public string StokAdi { get; set; }
        public string Birim { get; set; }
        public decimal Agirlik { get; set; }
        public decimal Protein { get; set; }
        public decimal Enerji { get; set; }
        public decimal Nem { get; set; }
        public string Barkod { get; set; }
        public string YemOzelligi { get; set; }
        public string Aciklama { get; set; }
        public string Mensei { get; set; }
        public bool Aktif { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        // STOKSABITFIYAT
        public decimal? AlisFiyat { get; set; }
        public decimal? AlisFiyat2 { get; set; }
        public decimal? AlisFiyat3 { get; set; }
        public decimal? AlisFiyat4 { get; set; }
        public decimal? AlisFiyat5 { get; set; }
        public decimal? KdvOrani { get; set; }
        public string ParaBirimi { get; set; }
        public DateTime? ListeTarihi { get; set; }

        // STOKSABITBELGE
        public string DosyaYolu { get; set; }

        // STOKSABITHAREKET
        public decimal? Miktar { get; set; }
        public int? DepoId { get; set; }
        public DateTime? IslemTarihi { get; set; }
    }
}