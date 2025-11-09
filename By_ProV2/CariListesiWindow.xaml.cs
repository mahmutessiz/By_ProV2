using System;
using System.Collections.Generic;
using By_ProV2.Helpers;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using By_ProV2.Models;


namespace By_ProV2
{
    public partial class CariListesiWindow : Window
    {
        private List<CariModel> tumCariler;

        // Seçilen cari bilgisi dışarıdan erişilebilir olacak
        public CariModel SecilenCari { get; set; }

        public CariListesiWindow()
        {
            InitializeComponent();
            VerileriYukle();
        }

        private void VerileriYukle()
        {
            try
            {
                string connStr = ConfigurationHelper.GetConnectionString("db");
                tumCariler = new List<CariModel>();

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT CARIKOD, CARIADI, ADRES, TELEFON, YETKILIKISI, BAGLICARIKOD, VERGIDAIRESI, VERGINO, ISK1, ISK2, ISK3, ISK4, KKISK1, KKISK2, KKISK3, KKISK4, NAKISK, PLAKA1, PLAKA2, PLAKA3, SOFORADSOYAD, KAYITTARIHI, SUTFIYATI, NAKFIYATI FROM CASABIT"; // Tablo adını kendi yapına göre düzelt

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tumCariler.Add(new CariModel
                            {
                                CariKod = reader["CARIKOD"].ToString(),
                                CariAdi = reader["CARIADI"].ToString(),
                                Adres = reader["ADRES"].ToString(),
                                Telefon = reader["TELEFON"].ToString(),
                                Yetkili = reader["YETKILIKISI"].ToString(),
                                BagliCariKod = reader["BAGLICARIKOD"].ToString(),
                                VergiDairesi = reader["VERGIDAIRESI"].ToString(),
                                VergiNo = reader["VERGINO"].ToString(),
                                Isk1 = Convert.ToDecimal(reader["ISK1"]),
                                Isk2 = Convert.ToDecimal(reader["ISK2"]),
                                Isk3 = Convert.ToDecimal(reader["ISK3"]),
                                Isk4 = Convert.ToDecimal(reader["ISK4"]),
                                KKIsk1 = Convert.ToDecimal(reader["KKISK1"]),
                                KKIsk2 = Convert.ToDecimal(reader["KKISK2"]),
                                KKIsk3 = Convert.ToDecimal(reader["KKISK3"]),
                                KKIsk4 = Convert.ToDecimal(reader["KKISK4"]),
                                NakliyeIskonto = Convert.ToDecimal(reader["NAKISK"]),
                                Plaka1 = reader["PLAKA1"].ToString(),
                                Plaka2 = reader["PLAKA2"].ToString(),
                                Plaka3 = reader["PLAKA3"].ToString(),
                                SoforAdSoyad = reader["SOFORADSOYAD"].ToString(),
                                KayitTarihi = Convert.ToDateTime(reader["KAYITTARIHI"]),
                                SutFiyati = reader["SUTFIYATI"] != DBNull.Value ? Convert.ToDecimal(reader["SUTFIYATI"]) : 0,
                                NakliyeFiyati = reader["NAKFIYATI"] != DBNull.Value ? Convert.ToDecimal(reader["NAKFIYATI"]) : 0
                            });

                        }
                    }
                }

                dataGridCariler.ItemsSource = tumCariler;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken bir hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void txtAraCariKod_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrele();
        }

        private void txtAraCariAdi_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrele();
        }

        private void Filtrele()
        {
            string kod = txtAraCariKod.Text.ToLower();
            string adi = txtAraCariAdi.Text.ToLower();

            var filtreli = tumCariler.Where(c =>
                (string.IsNullOrEmpty(kod) || c.CariKod.ToLower().Contains(kod)) &&
                (string.IsNullOrEmpty(adi) || c.CariAdi.ToLower().Contains(adi))
            ).ToList();

            dataGridCariler.ItemsSource = filtreli;
        }

        private void dataGridCariler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridCariler.SelectedItem is CariModel secilen)
            {
                SecilenCari = secilen;
                this.DialogResult = true;
                this.Close();
            }
        }
        private void dataGridCariler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && dataGridCariler.SelectedItem is CariModel secilen)
            {
                SecilenCari = secilen;
                this.DialogResult = true;
                this.Close();
            }
        }

    }

}