using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using By_ProV2.Helpers;
using By_ProV2.Models;

namespace By_ProV2
{
    public class AlisIcmalKaydi
    {
        public DateTime Tarih { get; set; }
        public string CariKod { get; set; }
        public decimal Miktar { get; set; }
        public decimal? Yag { get; set; }
        public decimal? Protein { get; set; }
    }

    public partial class AlisIcmalWindow : Window
    {
        private readonly string _connectionString;
        private ObservableCollection<AlisIcmalKaydi> _alisIcmalListesi;

        public AlisIcmalWindow()
        {
            InitializeComponent();
            _connectionString = ConfigurationHelper.GetConnectionString("db");
            _alisIcmalListesi = new ObservableCollection<AlisIcmalKaydi>();
            dgAlisIcmal.ItemsSource = _alisIcmalListesi;
            
            // Set default date range to last 30 days
            dpStartDate.SelectedDate = DateTime.Now.Date.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Now.Date;
            
            // Initialize summary labels with zeros
            lblToplamSut.Text = "Toplam Süt: 0";
            lblOrtalamaYag.Text = "Ortalama Yağ: 0";
            lblOrtalamaProtein.Text = "Ortalama Protein: 0";
            lblToplamKayit.Text = "Toplam Kayıt: 0";
        }

        private void BtnSorgula_Click(object sender, RoutedEventArgs e)
        {
            LoadAlisIcmal();
        }

        private void BtnTumunuListele_Click(object sender, RoutedEventArgs e)
        {
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtCariKodu.Text = "";
            LoadAlisIcmal();
        }

        private void LoadAlisIcmal()
        {
            _alisIcmalListesi.Clear();
            
            DateTime? startDate = dpStartDate.SelectedDate;
            DateTime? endDate = dpEndDate.SelectedDate;
            string cariKodu = txtCariKodu.Text.Trim();

            // Validate date range
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string sql = @"
                    SELECT 
                        sk.Tarih,
                        c.CariKod,
                        sk.Miktar,
                        sk.Yag,
                        sk.Protein
                    FROM SutKayit sk
                    LEFT JOIN Cari c ON sk.TedarikciId = c.CariId
                    WHERE sk.IslemTuru = 'Depoya Alım'
                    AND (@StartDate IS NULL OR sk.Tarih >= @StartDate)
                    AND (@EndDate IS NULL OR sk.Tarih <= @EndDate)
                    AND (@CariKod IS NULL OR @CariKod = '' OR c.CariKod LIKE '%' + @CariKod + '%')
                    ORDER BY sk.Tarih DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.HasValue ? (object)startDate.Value.Date : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndDate", endDate.HasValue ? (object)endDate.Value.Date.AddDays(1).AddSeconds(-1) : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CariKod", string.IsNullOrEmpty(cariKodu) ? (object)DBNull.Value : cariKodu);

                        using (var reader = cmd.ExecuteReader())
                        {
                            decimal toplamSut = 0;
                            decimal agirlikliYagToplami = 0;
                            decimal toplamSutForYag = 0;
                            decimal agirlikliProteinToplami = 0;
                            decimal toplamSutForProtein = 0;
                            int toplamKayit = 0;

                            while (reader.Read())
                            {
                                var newKayit = new AlisIcmalKaydi
                                {
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                    CariKod = reader["CariKod"] as string,
                                    Miktar = reader.GetDecimal(reader.GetOrdinal("Miktar")),
                                    Yag = reader.IsDBNull(reader.GetOrdinal("Yag")) ? null : reader.GetDecimal(reader.GetOrdinal("Yag")),
                                    Protein = reader.IsDBNull(reader.GetOrdinal("Protein")) ? null : reader.GetDecimal(reader.GetOrdinal("Protein"))
                                };

                                _alisIcmalListesi.Add(newKayit);

                                toplamSut += newKayit.Miktar;
                                toplamKayit++;

                                if (newKayit.Yag.HasValue && newKayit.Yag.Value != 0)
                                {
                                    agirlikliYagToplami += newKayit.Yag.Value * newKayit.Miktar;
                                    toplamSutForYag += newKayit.Miktar;
                                }

                                if (newKayit.Protein.HasValue && newKayit.Protein.Value != 0)
                                {
                                    agirlikliProteinToplami += newKayit.Protein.Value * newKayit.Miktar;
                                    toplamSutForProtein += newKayit.Miktar;
                                }
                            }

                            decimal ortalamaYag = toplamSutForYag > 0 ? agirlikliYagToplami / toplamSutForYag : 0;
                            decimal ortalamaProtein = toplamSutForProtein > 0 ? agirlikliProteinToplami / toplamSutForProtein : 0;

                            // Update summary labels
                            lblToplamSut.Text = $"Toplam Süt: {toplamSut:N2}";
                            lblOrtalamaYag.Text = $"Ortalama Yağ: {ortalamaYag:N2}";
                            lblOrtalamaProtein.Text = $"Ortalama Protein: {ortalamaProtein:N2}";
                            lblToplamKayit.Text = $"Toplam Kayıt: {toplamKayit}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Alış icmali yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtCariKodu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadAlisIcmal();
            }
        }
    }
}