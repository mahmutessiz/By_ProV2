using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2.DataAccess
{
    public class SutRaporRepository
    {
        private readonly string _connectionString;

        public SutRaporRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        /// <summary>
        /// Seçilen tarihe göre tüm günlük süt kayıtlarını getirir.
        /// </summary>
        public List<SutKaydi> GetGunlukSutKayit(DateTime tarih)
        {
            List<SutKaydi> kayitlar = new List<SutKaydi>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT s.SutKayitId,
                           s.Tarih,
                           s.IslemTuru,
                           s.TedarikciId,
                           s.MusteriId,
                           t.CariKod AS TedarikciKod,
                           t.CariAdi AS TedarikciAdi,
                           m.CariKod AS MusteriKod,
                           m.CariAdi AS MusteriAdi,
                           s.Miktar,
                           s.Yag,
                           s.Protein,
                           s.Laktoz,
                           s.NetMiktar,
                           s.TKM,
                           s.YKM,
                           s.pH,
                           s.Iletkenlik,
                           s.Sicaklik,
                           s.Yogunluk,
                           s.Kesinti,
                           s.Antibiyotik,
                           s.Arac,
                           s.Plaka,
                           s.DonmaN,
                           s.Bakteri,
                           s.Somatik,
                           s.Durumu,
                           s.Aciklama
                    FROM SutKayit s
                    LEFT JOIN Cari t ON s.TedarikciId = t.CariId
                    LEFT JOIN Cari m ON s.MusteriId = m.CariId
                    WHERE CAST(s.Tarih AS DATE) = @Tarih";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Tarih", tarih.Date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SutKaydi kayit = new SutKaydi
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("SutKayitId")),
                                Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                IslemTuru = reader["IslemTuru"].ToString(),
                                TedarikciId = reader["TedarikciId"] != DBNull.Value ? Convert.ToInt32(reader["TedarikciId"]) : 0,
                                MusteriId = reader["MusteriId"] != DBNull.Value ? Convert.ToInt32(reader["MusteriId"]) : 0,
                                TedarikciKod = reader["TedarikciKod"]?.ToString(),
                                TedarikciAdi = reader["TedarikciAdi"]?.ToString(),
                                MusteriKod = reader["MusteriKod"]?.ToString(),
                                MusteriAdi = reader["MusteriAdi"]?.ToString(),
                                Miktar = reader["Miktar"] != DBNull.Value ? Convert.ToDecimal(reader["Miktar"]) : 0,
                                Yag = reader["Yag"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Yag"]) : null,
                                Protein = reader["Protein"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Protein"]) : null,
                                Laktoz = reader["Laktoz"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Laktoz"]) : null,
                                NetMiktar = reader["NetMiktar"] != DBNull.Value ? Convert.ToDecimal(reader["NetMiktar"]) : 0,
                                TKM = reader["TKM"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["TKM"]) : null,
                                YKM = reader["YKM"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["YKM"]) : null,
                                pH = reader["pH"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["pH"]) : null,
                                Iletkenlik = reader["Iletkenlik"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Iletkenlik"]) : null,
                                Sicaklik = reader["Sicaklik"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Sicaklik"]) : null,
                                Yogunluk = reader["Yogunluk"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Yogunluk"]) : null,
                                Kesinti = reader["Kesinti"] != DBNull.Value ? Convert.ToDecimal(reader["Kesinti"]) : 0,
                                Antibiyotik = reader["Antibiyotik"]?.ToString(),
                                AracTemizlik = reader["Arac"]?.ToString(),
                                Plaka = reader["Plaka"]?.ToString(),
                                DonmaN = reader["DonmaN"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["DonmaN"]) : null,
                                Bakteri = reader["Bakteri"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Bakteri"]) : null,
                                Somatik = reader["Somatik"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Somatik"]) : null,
                                Durumu = reader["Durumu"]?.ToString(),
                                Aciklama = reader["Aciklama"]?.ToString()
                            };

                            kayitlar.Add(kayit);
                        }
                    }
                }
            }

            return kayitlar;
        }
    }
}