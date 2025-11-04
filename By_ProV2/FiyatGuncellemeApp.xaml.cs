using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using ClosedXML.Excel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using By_ProV2.Models;
using Microsoft.Win32;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Configuration;



namespace By_ProV2
{
    /// <summary>
    /// FiyatGuncellemeApp.xaml etkileşim mantığı
    /// </summary>
    public partial class FiyatGuncellemeApp : Window
    {
        private string connStr;
        public FiyatGuncellemeApp()
        {
            InitializeComponent();
            connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
        }

        private List<FiyatGuncelleModel> ExcelOku(string path)
        {
            var liste = new List<FiyatGuncelleModel>();

            using (var workbook = new XLWorkbook(path))
            {
                var worksheet = workbook.Worksheet(1); // İlk sayfa
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Başlığı atla

                foreach (var row in rows)
                {
                    try
                    {
                        var dto = new FiyatGuncelleModel
                        {
                            STOKID = int.Parse(row.Cell(1).GetValue<string>()),
                            STOKKODU = row.Cell(2).GetValue<string>(),
                            STOKADI = row.Cell(3).GetValue<string>(),
                            LISTEADI = row.Cell(4).GetValue<string>(),
                            LISTETARIHI = DateTime.Parse(row.Cell(5).GetValue<string>()),
                            ALISFIYAT1 = decimal.Parse(row.Cell(6).GetValue<string>()),
                            ALISFIYAT2 = decimal.Parse(row.Cell(7).GetValue<string>()),
                            ALISFIYAT3 = decimal.Parse(row.Cell(8).GetValue<string>()),
                            ALISFIYAT4 = decimal.Parse(row.Cell(9).GetValue<string>()),
                            ALISFIYAT5 = decimal.Parse(row.Cell(10).GetValue<string>()),
                            KDVORANI = decimal.Parse(row.Cell(11).GetValue<string>()),
                            PARABIRIMI = row.Cell(12).GetValue<string>()
                        };

                        liste.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Satır okunamadı:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            return liste;
        }


        private List<FiyatGuncelleModel> AktifFiyatlariGetir()
        {
            var liste = new List<FiyatGuncelleModel>();
            string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT SF.STOKID, SS.STOKKODU, SS.STOKADI, SF.LISTEADI, SF.LISTETARIHI, 
                                SF.ALISFIYAT1, SF.ALISFIYAT2, SF.ALISFIYAT3, SF.ALISFIYAT4, SF.ALISFIYAT5, 
                                SF.KDVORANI, SF.PARABIRIMI
                                FROM STOKSABITFIYAT SF
                        JOIN STOKSABITKART SS ON SF.STOKID = SS.STOKID
                                WHERE SF.AKTIF = 1";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        liste.Add(new FiyatGuncelleModel
                        {
                            STOKID = reader.GetInt32(0),
                            STOKKODU = reader.GetString(1),
                            STOKADI = reader.GetString(2),
                            LISTEADI = reader.GetString(3),
                            LISTETARIHI = reader.GetDateTime(4),
                            ALISFIYAT1 = reader.GetDecimal(5),
                            ALISFIYAT2 = reader.GetDecimal(6),
                            ALISFIYAT3 = reader.GetDecimal(7),
                            ALISFIYAT4 = reader.GetDecimal(8),
                            ALISFIYAT5 = reader.GetDecimal(9),
                            KDVORANI = reader.GetDecimal(10),
                            PARABIRIMI = reader.GetString(11)
                        });


                    }
                }
            }

            return liste;
        }
        private List<FiyatGuncelleModel> FiyatlariKarsilastir(List<FiyatGuncelleModel> eski, List<FiyatGuncelleModel> yeni)
        {
            foreach (var yeniSatir in yeni)
            {
                var eskiSatir = eski.FirstOrDefault(x => x.STOKID == yeniSatir.STOKID);

                bool degisti = eskiSatir == null || (
                    eskiSatir.ALISFIYAT1 != yeniSatir.ALISFIYAT1 ||
                    eskiSatir.ALISFIYAT2 != yeniSatir.ALISFIYAT2 ||
                    eskiSatir.ALISFIYAT3 != yeniSatir.ALISFIYAT3 ||
                    eskiSatir.ALISFIYAT4 != yeniSatir.ALISFIYAT4 ||
                    eskiSatir.ALISFIYAT5 != yeniSatir.ALISFIYAT5 ||
                    eskiSatir.KDVORANI != yeniSatir.KDVORANI
                );

                yeniSatir.DegistiMi = degisti;
            }

            return yeni;
        }
        private void btnDosyaYukle_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                var yeniFiyatlar = ExcelOku(path);
                var eskiFiyatlar = AktifFiyatlariGetir();
                var karsilastirilmis = FiyatlariKarsilastir(eskiFiyatlar, yeniFiyatlar);

                dgSablon.ItemsSource = karsilastirilmis;
            }
        }


        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            var fiyatlar = dgSablon.ItemsSource as List<FiyatGuncelleModel>;
            if (fiyatlar == null)
            {
                MessageBox.Show("Güncellenecek veri bulunamadı.");
                return;
            }

            var result = MessageBox.Show("Değişiklik yapılan satırlardaki fiyatlar güncellenecektir.\n\nDevam etmek istiyor musunuz?",
                "Fiyat Güncelleme",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                // Kullanıcı vazgeçti
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var fiyat in fiyatlar.Where(f => f.DegistiMi))
                            {
                                var cmd = new SqlCommand(@"
                            UPDATE STOKSABITFIYAT
                            SET ALISFIYAT1 = @AF1,
                                ALISFIYAT2 = @AF2,
                                ALISFIYAT3 = @AF3,
                                ALISFIYAT4 = @AF4,
                                ALISFIYAT5 = @AF5,
                                KDVORANI = @KDV
                            WHERE STOKID = @STOKID", conn, trans);

                                cmd.Parameters.AddWithValue("@AF1", fiyat.ALISFIYAT1);
                                cmd.Parameters.AddWithValue("@AF2", fiyat.ALISFIYAT2);
                                cmd.Parameters.AddWithValue("@AF3", fiyat.ALISFIYAT3);
                                cmd.Parameters.AddWithValue("@AF4", fiyat.ALISFIYAT4);
                                cmd.Parameters.AddWithValue("@AF5", fiyat.ALISFIYAT5);
                                cmd.Parameters.AddWithValue("@KDV", fiyat.KDVORANI);
                                cmd.Parameters.AddWithValue("@STOKID", fiyat.STOKID);

                                cmd.ExecuteNonQuery();
                            }
                            trans.Commit();
                            MessageBox.Show("Fiyatlar başarıyla güncellendi.");
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Güncelleme sırasında hata oluştu: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantı hatası: " + ex.Message);
            }
        }
        private void BtnBaslangicStokSec_Click(object sender, RoutedEventArgs e)
        {
            var stokPenceresi = new StokListeWindow();
            if (stokPenceresi.ShowDialog() == true)
            {
                txtBaslangicStokKodu.Text = stokPenceresi.SecilenStok?.StokKodu;

                // Set text color to black
                txtBaslangicStokKodu.Foreground = Brushes.Black;
            }
        }


        private void BtnBitisStokSec_Click(object sender, RoutedEventArgs e)
        {
            var stokPenceresi = new StokListeWindow();
            if (stokPenceresi.ShowDialog() == true)
            {
                txtBitisStokKodu.Text = stokPenceresi.SecilenStok?.StokKodu;

                // Set text color to black
                txtBitisStokKodu.Foreground = Brushes.Black;
            }
        }


        private void BtnFiltrele_Click(object sender, RoutedEventArgs e)
        {
            string baslangicKod = txtBaslangicStokKodu.Text?.Trim();
            string bitisKod = txtBitisStokKodu.Text?.Trim();

            var liste = AktifFiyatlariGetir();

            // Eğer ikisi de boş değilse aralığa göre filtrele
            if (!string.IsNullOrEmpty(baslangicKod) && !string.IsNullOrEmpty(bitisKod))
            {
                liste = liste
                    .Where(x =>
                        string.Compare(x.STOKKODU, baslangicKod, StringComparison.OrdinalIgnoreCase) >= 0 &&
                        string.Compare(x.STOKKODU, bitisKod, StringComparison.OrdinalIgnoreCase) <= 0)
                    .ToList();
            }

            dgSablon.ItemsSource = liste;
        }

        private void BtnSablonOlustur_Click(object sender, RoutedEventArgs e)
        {
            var liste = dgSablon.ItemsSource as List<FiyatGuncelleModel>;

            if (liste == null || !liste.Any())
            {
                MessageBox.Show("Dışa aktarılacak veri bulunamadı.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                FileName = "FiyatSablon.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Fiyatlar");

                    // Başlıklar
                    worksheet.Cell(1, 1).Value = "STOKID";
                    worksheet.Cell(1, 2).Value = "STOKKODU";
                    worksheet.Cell(1, 3).Value = "STOKADI";
                    worksheet.Cell(1, 4).Value = "LISTEADI";
                    worksheet.Cell(1, 5).Value = "LISTETARIHI";
                    worksheet.Cell(1, 6).Value = "ALISFIYAT1";
                    worksheet.Cell(1, 7).Value = "ALISFIYAT2";
                    worksheet.Cell(1, 8).Value = "ALISFIYAT3";
                    worksheet.Cell(1, 9).Value = "ALISFIYAT4";
                    worksheet.Cell(1, 10).Value = "ALISFIYAT5";
                    worksheet.Cell(1, 11).Value = "KDVORANI";
                    worksheet.Cell(1, 12).Value = "PARABIRIMI";

                    // Veriler
                    int row = 2;
                    foreach (var item in liste)
                    {
                        worksheet.Cell(row, 1).Value = item.STOKID;
                        worksheet.Cell(row, 2).Value = item.STOKKODU;
                        worksheet.Cell(row, 3).Value = item.STOKADI;
                        worksheet.Cell(row, 4).Value = item.LISTEADI;
                        worksheet.Cell(row, 5).Value = item.LISTETARIHI.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 6).Value = item.ALISFIYAT1;
                        worksheet.Cell(row, 7).Value = item.ALISFIYAT2;
                        worksheet.Cell(row, 8).Value = item.ALISFIYAT3;
                        worksheet.Cell(row, 9).Value = item.ALISFIYAT4;
                        worksheet.Cell(row, 10).Value = item.ALISFIYAT5;
                        worksheet.Cell(row, 11).Value = item.KDVORANI;
                        worksheet.Cell(row, 12).Value = item.PARABIRIMI;

                        row++;
                    }

                    worksheet.Columns().AdjustToContents(); // Otomatik sütun genişliği
                    workbook.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Excel şablonu başarıyla oluşturuldu.");
            }
        }
        private void BtnMusteriFiyatListesi_Click(object sender, RoutedEventArgs e)
        {
            var liste = dgSablon.ItemsSource as List<FiyatGuncelleModel>;

            if (liste == null || !liste.Any())
            {
                MessageBox.Show("Liste boş. Önce veri yükleyin.");
                return;
            }

            var pencere = new IskontoGirWindow(liste);
            pencere.Owner = this;
            pencere.ShowDialog();
        }


    }
}