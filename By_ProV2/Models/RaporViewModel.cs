using System;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using By_ProV2.Helpers;

namespace By_ProV2.Models
{
    public class RaporViewModel : BaseViewModel
    {
        public ObservableCollection<RaporSatiri> Rapor { get; set; } = new ObservableCollection<RaporSatiri>();


        private DateTime _baslangicTarihi = DateTime.Today.AddDays(-7);
        public DateTime BaslangicTarihi
        {
            get => _baslangicTarihi;
            set { _baslangicTarihi = value; OnPropertyChanged(); }
        }

        private DateTime _bitisTarihi = DateTime.Today;
        public DateTime BitisTarihi
        {
            get => _bitisTarihi;
            set { _bitisTarihi = value; OnPropertyChanged(); }
        }

        private string _cariKodBaslangic = "000";
        public string CariKodBaslangic
        {
            get => _cariKodBaslangic;
            set { _cariKodBaslangic = value; OnPropertyChanged(); }
        }

        private string _cariKodBitis = "ZZZ";
        public string CariKodBitis
        {
            get => _cariKodBitis;
            set { _cariKodBitis = value; OnPropertyChanged(); }
        }
        
        private int _toplamAdet;
        public int ToplamAdet
        {
            get => _toplamAdet;
            set { _toplamAdet = value; OnPropertyChanged(); }
        }

        private decimal _toplamAlisTutari;
        public decimal ToplamAlisTutari
        {
            get => _toplamAlisTutari;
            set { _toplamAlisTutari = value; OnPropertyChanged(); }
        }

        private decimal _toplamSatisTutari;
        public decimal ToplamSatisTutari
        {
            get => _toplamSatisTutari;
            set { _toplamSatisTutari = value; OnPropertyChanged(); }
        }

        private decimal _toplamBrutKar;
        public decimal ToplamBrutKar
        {
            get => _toplamBrutKar;
            set { _toplamBrutKar = value; OnPropertyChanged(); }
        }

        public async Task YukleAsync()
        {
            ToplamAdet = 0;
            ToplamAlisTutari = 0;
            ToplamSatisTutari = 0;
            ToplamBrutKar = 0;
            Rapor.Clear();

            string connStr = ConfigurationHelper.GetConnectionString("db");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                string sql = @"
            SELECT
                BelgeKodu,
                SevkTarihi,
                TeslimKod AS CariKodu,
                TeslimIsim AS CariAdi,
                CAST(AlisToplamTutar AS decimal(18,2)) AS AlisTutari,
                CAST(SatisToplamTutar AS decimal(18,2)) AS SatisTutari
            FROM 
                SiparisMaster
            WHERE 
                SevkTarihi >= @Baslangic AND SevkTarihi < DATEADD(day, 1, @Bitis)
                AND TeslimKod BETWEEN @CariKodBaslangic AND @CariKodBitis
            ORDER BY SevkTarihi ASC, BelgeKodu ASC
        ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", BaslangicTarihi);
                    cmd.Parameters.AddWithValue("@Bitis", BitisTarihi);
                    cmd.Parameters.AddWithValue("@CariKodBaslangic", CariKodBaslangic);
                    cmd.Parameters.AddWithValue("@CariKodBitis", CariKodBitis);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var satir = new RaporSatiri
                            {
                                BelgeKodu = reader["BelgeKodu"]?.ToString(),
                                TeslimTarihi = reader.GetDateTime(1),
                                CariKodu = reader.GetString(2),
                                CariAdi = reader.GetString(3),
                                AlisTutari = reader.GetDecimal(4),
                                SatisTutari = reader.GetDecimal(5)
                            };

                            Rapor.Add(satir);

                            // Toplamları güncelle
                            ToplamAdet++;
                            ToplamAlisTutari += satir.AlisTutari;
                            ToplamSatisTutari += satir.SatisTutari;
                            ToplamBrutKar += satir.BrutKar;
                        }
                    }
                }
            }
        }

    }
}
