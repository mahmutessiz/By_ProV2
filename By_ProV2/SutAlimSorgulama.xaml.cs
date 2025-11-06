using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using By_ProV2.Models;
using Microsoft.Data.SqlClient;
using By_ProV2.Helpers;

namespace By_ProV2
{
    public class SutAlimSorgulamaBelge
    {
        public string BelgeNo { get; set; }
        public DateTime Tarih { get; set; }
        public string IslemTuru { get; set; }
    }

    public partial class SutAlimSorgulama : Window
    {
        private readonly string _connectionString;
        private ObservableCollection<SutAlimSorgulamaBelge> _sutBelgeleri;

        public SutAlimSorgulama()
        {
            InitializeComponent();
            _connectionString = ConfigurationHelper.GetConnectionString("db");
            _sutBelgeleri = new ObservableCollection<SutAlimSorgulamaBelge>();
            dgBelgeler.ItemsSource = _sutBelgeleri;
            Loaded += SutAlimSorgulama_Loaded;
        }

        private void SutAlimSorgulama_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllSutBelgeleri();
        }

        private void BtnSorgula_Click(object sender, RoutedEventArgs e)
        {
            string belgeNo = txtBelgeNo.Text.Trim();
            if (string.IsNullOrWhiteSpace(belgeNo))
            {
                MessageBox.Show("Lütfen bir belge numarası giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            LoadSutBelgeByBelgeNo(belgeNo);
        }

        private void BtnTumunuListele_Click(object sender, RoutedEventArgs e)
        {
            LoadAllSutBelgeleriFull();
        }

        private void LoadAllSutBelgeleri()
        {
            _sutBelgeleri.Clear();
            try
            {
                string sql = @"
                    SELECT TOP 20
                        BelgeNo, 
                        MIN(Tarih) as Tarih, 
                        MIN(IslemTuru) as IslemTuru 
                    FROM SutKayit 
                    GROUP BY BelgeNo 
                    ORDER BY MIN(Tarih) DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var newBelge = new SutAlimSorgulamaBelge
                                {
                                    BelgeNo = reader["BelgeNo"] as string,
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                    IslemTuru = reader["IslemTuru"] as string,
                                };
                                _sutBelgeleri.Add(newBelge);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Belgeler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAllSutBelgeleriFull()
        {
            _sutBelgeleri.Clear();
            try
            {
                string sql = @"
                    SELECT 
                        BelgeNo, 
                        MIN(Tarih) as Tarih, 
                        MIN(IslemTuru) as IslemTuru 
                    FROM SutKayit 
                    GROUP BY BelgeNo 
                    ORDER BY MIN(Tarih) DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var newBelge = new SutAlimSorgulamaBelge
                                {
                                    BelgeNo = reader["BelgeNo"] as string,
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                    IslemTuru = reader["IslemTuru"] as string,
                                };
                                _sutBelgeleri.Add(newBelge);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Belgeler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSutBelgeByBelgeNo(string belgeNo)
        {
            _sutBelgeleri.Clear();
            try
            {
                string sql = @"
                    SELECT 
                        BelgeNo, 
                        MIN(Tarih) as Tarih, 
                        MIN(IslemTuru) as IslemTuru 
                    FROM SutKayit 
                    WHERE BelgeNo LIKE @BelgeNo
                    GROUP BY BelgeNo 
                    ORDER BY MIN(Tarih) DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BelgeNo", $"%{belgeNo}%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var newBelge = new SutAlimSorgulamaBelge
                                {
                                    BelgeNo = reader["BelgeNo"] as string,
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                    IslemTuru = reader["IslemTuru"] as string,
                                };
                                _sutBelgeleri.Add(newBelge);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Belge aranırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTarihSorgula_Click(object sender, RoutedEventArgs e)
        {
            if (dpSearchDate.SelectedDate == null)
            {
                MessageBox.Show("Lütfen bir tarih seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime searchDate = dpSearchDate.SelectedDate.Value;
            LoadSutBelgeByExactDate(searchDate);
        }

        private void LoadSutBelgeByExactDate(DateTime searchDate)
        {
            _sutBelgeleri.Clear();
            try
            {
                string sql = @"
                    SELECT 
                        BelgeNo, 
                        MIN(Tarih) as Tarih, 
                        MIN(IslemTuru) as IslemTuru 
                    FROM SutKayit 
                    WHERE Tarih >= @SearchDate AND Tarih < DATEADD(day, 1, @SearchDate)
                    GROUP BY BelgeNo 
                    ORDER BY MIN(Tarih) DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchDate", searchDate);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var newBelge = new SutAlimSorgulamaBelge
                                {
                                    BelgeNo = reader["BelgeNo"] as string,
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                    IslemTuru = reader["IslemTuru"] as string,
                                };
                                _sutBelgeleri.Add(newBelge);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Belgeler tarihe göre aranırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtBelgeNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSorgula_Click(null, null);
            }
        }

        private void dgBelgeler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgBelgeler.SelectedItem is SutAlimSorgulamaBelge selectedBelge)
            {
                SutAlimFormu sutAlimForm = new SutAlimFormu();
                sutAlimForm.LoadDocumentForViewing(selectedBelge.BelgeNo);
                sutAlimForm.Show();
            }
        }
    }
}