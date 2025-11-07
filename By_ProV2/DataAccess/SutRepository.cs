using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.DataAccess;
using By_ProV2.Services;
using By_ProV2.Helpers;


namespace By_ProV2.DataAccess
{
    public class SutRepository
    {
        private readonly string _connectionString;

        public SutRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        private int? GetCariIdByKod(string kod)
        {
            if (string.IsNullOrWhiteSpace(kod))
                return null;

            string query = "SELECT CariId FROM Cari WHERE CariKod = @Kod";
            SqlParameter[] parameters = { new SqlParameter("@Kod", kod) };

            object result = DatabaseHelper.ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : (int?)null;
        }

        public void KaydetSutKaydi(SutKaydi kayit)
        {
            if (kayit == null)
                throw new ArgumentNullException(nameof(kayit));

            // Set user tracking fields
            var currentUser = App.AuthService?.CurrentUser;
            if (currentUser != null)
            {
                kayit.CreatedBy = currentUser.Id;
                kayit.ModifiedBy = currentUser.Id;
            }
            
            // Set timestamps for new records
            DateTime now = DateTime.Now;
            kayit.CreatedAt = now;
            kayit.ModifiedAt = now;

            var cariRepo = new CariRepository();
            var stokRepo = new DepoStokRepository();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Cari’leri transaction içinde çöz
                        if (!string.IsNullOrWhiteSpace(kayit.TedarikciKod))
                            kayit.TedarikciId = cariRepo.GetOrCreateCari(kayit.TedarikciKod, kayit.TedarikciAdi, "Tedarikçi", conn, tran);

                        if (!string.IsNullOrWhiteSpace(kayit.MusteriKod))
                            kayit.MusteriId = cariRepo.GetOrCreateCari(kayit.MusteriKod, kayit.MusteriAdi, "Müşteri", conn, tran);

                        // 2️⃣ SutKayit tablosuna ekle
                        string insertQuery = @"
                    INSERT INTO SutKayit (BelgeNo, Tarih, IslemTuru, TedarikciId, MusteriId, Miktar, Yag, Protein, Laktoz, NetMiktar,
                     TKM, YKM, pH, Iletkenlik, Sicaklik, Yogunluk, Kesinti, Antibiyotik, Arac, Plaka,
                     DonmaN, Bakteri, Somatik, Durumu, Aciklama, CreatedBy, ModifiedBy, CreatedAt, ModifiedAt)
                    VALUES (@BelgeNo, @Tarih, @IslemTuru, @TedarikciId, @MusteriId, @Miktar, @Yag, @Protein, @Laktoz, @NetMiktar,
                     @TKM, @YKM, @pH, @Iletkenlik, @Sicaklik, @Yogunluk, @Kesinti, @Antibiyotik, @Arac, @Plaka,
                     @DonmaN, @Bakteri, @Somatik, @Durumu, @Aciklama, @CreatedBy, @ModifiedBy, @CreatedAt, @ModifiedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (var cmd = new SqlCommand(insertQuery, conn, tran))
                        {
                            // Parametreler...
                            cmd.Parameters.AddWithValue("@BelgeNo", kayit.BelgeNo ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Tarih", kayit.Tarih);
                            cmd.Parameters.AddWithValue("@IslemTuru", kayit.IslemTuru ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TedarikciId", kayit.TedarikciId == 0 ? (object)DBNull.Value : kayit.TedarikciId);
                            cmd.Parameters.AddWithValue("@MusteriId", kayit.MusteriId == 0 ? (object)DBNull.Value : kayit.MusteriId);
                            cmd.Parameters.AddWithValue("@Miktar", kayit.Miktar);
                            cmd.Parameters.AddWithValue("@Yag", kayit.Yag ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Protein", kayit.Protein ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Laktoz", kayit.Laktoz ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@NetMiktar", kayit.NetMiktar);
                            cmd.Parameters.AddWithValue("@TKM", kayit.TKM ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@YKM", kayit.YKM ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pH", kayit.pH ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Iletkenlik", kayit.Iletkenlik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Sicaklik", kayit.Sicaklik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Yogunluk", kayit.Yogunluk ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Kesinti", kayit.Kesinti);
                            cmd.Parameters.AddWithValue("@Antibiyotik", kayit.Antibiyotik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Arac", kayit.AracTemizlik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Plaka", kayit.Plaka ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DonmaN", kayit.DonmaN ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Bakteri", kayit.Bakteri ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Somatik", kayit.Somatik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Durumu", kayit.Durumu ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Aciklama", kayit.Aciklama ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", kayit.CreatedBy ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", kayit.ModifiedBy ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedAt", kayit.CreatedAt);
                            cmd.Parameters.AddWithValue("@ModifiedAt", kayit.ModifiedAt);

                            kayit.SutKayitId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 3️⃣ Depo stok hareketi (aynı transaction içinde)
                        if (kayit.IslemTuru == "Depoya Alım" || kayit.IslemTuru == "Depodan Sevk")
                            stokRepo.KaydetStokHareketi(kayit, conn, tran);

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public void GuncelleSutKaydi(SutKaydi kayit)
        {
            if (kayit == null)
                throw new ArgumentNullException(nameof(kayit));

            // Set user tracking fields
            var currentUser = App.AuthService?.CurrentUser;
            if (currentUser != null)
            {
                kayit.ModifiedBy = currentUser.Id;
            }
            kayit.ModifiedAt = DateTime.Now;

            var cariRepo = new CariRepository();
            var stokRepo = new DepoStokRepository();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Cari’leri transaction içinde çöz
                        if (!string.IsNullOrWhiteSpace(kayit.TedarikciKod))
                            kayit.TedarikciId = cariRepo.GetOrCreateCari(kayit.TedarikciKod, kayit.TedarikciAdi, "Tedarikçi", conn, tran);

                        if (!string.IsNullOrWhiteSpace(kayit.MusteriKod))
                            kayit.MusteriId = cariRepo.GetOrCreateCari(kayit.MusteriKod, kayit.MusteriAdi, "Müşteri", conn, tran);

                        // 2️⃣ SutKayit tablosunu güncelle
                        string updateQuery = @"
                    UPDATE SutKayit SET 
                        BelgeNo = @BelgeNo,
                        Tarih = @Tarih,
                        IslemTuru = @IslemTuru,
                        TedarikciId = @TedarikciId,
                        MusteriId = @MusteriId,
                        Miktar = @Miktar,
                        Yag = @Yag,
                        Protein = @Protein,
                        Laktoz = @Laktoz,
                        NetMiktar = @NetMiktar,
                        TKM = @TKM,
                        YKM = @YKM,
                        pH = @pH,
                        Iletkenlik = @Iletkenlik,
                        Sicaklik = @Sicaklik,
                        Yogunluk = @Yogunluk,
                        Kesinti = @Kesinti,
                        Antibiyotik = @Antibiyotik,
                        Arac = @Arac,
                        Plaka = @Plaka,
                        DonmaN = @DonmaN,
                        Bakteri = @Bakteri,
                        Somatik = @Somatik,
                        Durumu = @Durumu,
                        Aciklama = @Aciklama,
                        ModifiedBy = @ModifiedBy,
                        ModifiedAt = @ModifiedAt
                    WHERE SutKayitId = @SutKayitId";

                        using (var cmd = new SqlCommand(updateQuery, conn, tran))
                        {
                            // Parametreler...
                            cmd.Parameters.AddWithValue("@SutKayitId", kayit.SutKayitId);
                            cmd.Parameters.AddWithValue("@BelgeNo", kayit.BelgeNo ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Tarih", kayit.Tarih);
                            cmd.Parameters.AddWithValue("@IslemTuru", kayit.IslemTuru ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TedarikciId", kayit.TedarikciId == 0 ? (object)DBNull.Value : kayit.TedarikciId);
                            cmd.Parameters.AddWithValue("@MusteriId", kayit.MusteriId == 0 ? (object)DBNull.Value : kayit.MusteriId);
                            cmd.Parameters.AddWithValue("@Miktar", kayit.Miktar);
                            cmd.Parameters.AddWithValue("@Yag", kayit.Yag ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Protein", kayit.Protein ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Laktoz", kayit.Laktoz ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@NetMiktar", kayit.NetMiktar);
                            cmd.Parameters.AddWithValue("@TKM", kayit.TKM ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@YKM", kayit.YKM ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pH", kayit.pH ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Iletkenlik", kayit.Iletkenlik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Sicaklik", kayit.Sicaklik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Yogunluk", kayit.Yogunluk ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Kesinti", kayit.Kesinti);
                            cmd.Parameters.AddWithValue("@Antibiyotik", kayit.Antibiyotik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Arac", kayit.AracTemizlik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Plaka", kayit.Plaka ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DonmaN", kayit.DonmaN ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Bakteri", kayit.Bakteri ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Somatik", kayit.Somatik ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Durumu", kayit.Durumu ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Aciklama", kayit.Aciklama ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", kayit.ModifiedBy ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedAt", kayit.ModifiedAt);

                            cmd.ExecuteNonQuery();
                        }

                        // 3️⃣ Depo stok hareketini güncelle (aynı transaction içinde)
                        if (kayit.IslemTuru == "Depoya Alım" || kayit.IslemTuru == "Depodan Sevk")
                            stokRepo.GuncelleStokHareketi(kayit, conn, tran);

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<SutKaydi> GetSutKayitlariByBelgeNo(string belgeNo)
        {
            var kayitlar = new List<SutKaydi>();
            try
            {
                string sql = @"
                    SELECT 
                        sk.SutKayitId,
                        sk.BelgeNo,
                        sk.Tarih,
                        sk.IslemTuru,
                        sk.TedarikciId,
                        sk.MusteriId,
                        sk.Miktar,
                        sk.Yag,
                        sk.Protein,
                        sk.Laktoz,
                        sk.NetMiktar,
                        sk.TKM,
                        sk.YKM,
                        sk.pH,
                        sk.Iletkenlik,
                        sk.Sicaklik,
                        sk.Yogunluk,
                        sk.Kesinti,
                        sk.Antibiyotik,
                        sk.Arac,
                        sk.Plaka,
                        sk.DonmaN,
                        sk.Bakteri,
                        sk.Somatik,
                        sk.Durumu,
                        sk.Aciklama,
                        sk.CreatedBy,
                        sk.ModifiedBy,
                        sk.CreatedAt,
                        sk.ModifiedAt,
                        c1.CariKod AS TedarikciKod,
                        c1.CariAdi AS TedarikciAdi,
                        c2.CariKod AS MusteriKod,
                        c2.CariAdi AS MusteriAdi
                    FROM SutKayit sk
                    LEFT JOIN Cari c1 ON sk.TedarikciId = c1.CariId
                    LEFT JOIN Cari c2 ON sk.MusteriId = c2.CariId
                    WHERE sk.BelgeNo = @BelgeNo
                    ORDER BY sk.SutKayitId";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BelgeNo", belgeNo);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var kayit = new SutKaydi
                                {
                                    SutKayitId = reader.IsDBNull("SutKayitId") ? 0 : Convert.ToInt32(reader["SutKayitId"]),
                                    BelgeNo = reader.IsDBNull("BelgeNo") ? null : reader["BelgeNo"] as string,
                                    Tarih = reader.IsDBNull("Tarih") ? DateTime.Now : Convert.ToDateTime(reader["Tarih"]),
                                    IslemTuru = reader.IsDBNull("IslemTuru") ? null : reader["IslemTuru"] as string,
                                    TedarikciId = reader.IsDBNull("TedarikciId") ? 0 : Convert.ToInt32(reader["TedarikciId"]),
                                    MusteriId = reader.IsDBNull("MusteriId") ? 0 : Convert.ToInt32(reader["MusteriId"]),
                                    TedarikciKod = reader.IsDBNull("TedarikciKod") ? null : reader["TedarikciKod"] as string,
                                    TedarikciAdi = reader.IsDBNull("TedarikciAdi") ? null : reader["TedarikciAdi"] as string,
                                    MusteriKod = reader.IsDBNull("MusteriKod") ? null : reader["MusteriKod"] as string,
                                    MusteriAdi = reader.IsDBNull("MusteriAdi") ? null : reader["MusteriAdi"] as string,
                                    Miktar = reader.IsDBNull("Miktar") ? 0 : Convert.ToDecimal(reader["Miktar"]),
                                    NetMiktar = reader.IsDBNull("NetMiktar") ? 0 : Convert.ToDecimal(reader["NetMiktar"]),
                                    Yag = reader.IsDBNull("Yag") ? (decimal?)null : Convert.ToDecimal(reader["Yag"]),
                                    Protein = reader.IsDBNull("Protein") ? (decimal?)null : Convert.ToDecimal(reader["Protein"]),
                                    Laktoz = reader.IsDBNull("Laktoz") ? (decimal?)null : Convert.ToDecimal(reader["Laktoz"]),
                                    TKM = reader.IsDBNull("TKM") ? (decimal?)null : Convert.ToDecimal(reader["TKM"]),
                                    YKM = reader.IsDBNull("YKM") ? (decimal?)null : Convert.ToDecimal(reader["YKM"]),
                                    pH = reader.IsDBNull("pH") ? (decimal?)null : Convert.ToDecimal(reader["pH"]),
                                    Iletkenlik = reader.IsDBNull("Iletkenlik") ? (decimal?)null : Convert.ToDecimal(reader["Iletkenlik"]),
                                    Sicaklik = reader.IsDBNull("Sicaklik") ? (decimal?)null : Convert.ToDecimal(reader["Sicaklik"]),
                                    Yogunluk = reader.IsDBNull("Yogunluk") ? (decimal?)null : Convert.ToDecimal(reader["Yogunluk"]),
                                    Kesinti = reader.IsDBNull("Kesinti") ? 0 : Convert.ToDecimal(reader["Kesinti"]),
                                    Antibiyotik = reader.IsDBNull("Antibiyotik") ? null : reader["Antibiyotik"] as string,
                                    AracTemizlik = reader.IsDBNull("Arac") ? null : reader["Arac"] as string,
                                    Plaka = reader.IsDBNull("Plaka") ? null : reader["Plaka"] as string,
                                    DonmaN = reader.IsDBNull("DonmaN") ? (decimal?)null : Convert.ToDecimal(reader["DonmaN"]),
                                    Bakteri = reader.IsDBNull("Bakteri") ? (decimal?)null : Convert.ToDecimal(reader["Bakteri"]),
                                    Somatik = reader.IsDBNull("Somatik") ? (decimal?)null : Convert.ToDecimal(reader["Somatik"]),
                                    Durumu = reader.IsDBNull("Durumu") ? null : reader["Durumu"] as string,
                                    Aciklama = reader.IsDBNull("Aciklama") ? null : reader["Aciklama"] as string,
                                    CreatedBy = reader.IsDBNull("CreatedBy") ? (int?)null : Convert.ToInt32(reader["CreatedBy"]),
                                    ModifiedBy = reader.IsDBNull("ModifiedBy") ? (int?)null : Convert.ToInt32(reader["ModifiedBy"]),
                                    CreatedAt = reader.IsDBNull("CreatedAt") ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedAt"]),
                                    ModifiedAt = reader.IsDBNull("ModifiedAt") ? DateTime.MinValue : Convert.ToDateTime(reader["ModifiedAt"])
                                };
                                kayitlar.Add(kayit);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                throw;
            }
            return kayitlar;
        }

        public bool SilSutKaydi(int sutKayitId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // First, delete any related stock movements if needed
                            var stokRepo = new DepoStokRepository();
                            // We need to get the record to know its details before deletion
                            string selectQuery = "SELECT IslemTuru, TedarikciId, Tarih FROM SutKayit WHERE SutKayitId = @SutKayitId";
                            SutKaydi kayitTemp = null;
                            
                            using (var selectCmd = new SqlCommand(selectQuery, conn, trans))
                            {
                                selectCmd.Parameters.AddWithValue("@SutKayitId", sutKayitId);
                                using (var reader = selectCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        kayitTemp = new SutKaydi
                                        {
                                            IslemTuru = reader["IslemTuru"].ToString(),
                                            TedarikciId = reader["TedarikciId"] != DBNull.Value ? Convert.ToInt32(reader["TedarikciId"]) : 0,
                                            Tarih = Convert.ToDateTime(reader["Tarih"])
                                        };
                                    }
                                }
                            }

                            // Delete stock movement if needed
                            if (kayitTemp != null && (kayitTemp.IslemTuru == "Depoya Alım" || kayitTemp.IslemTuru == "Depodan Sevk"))
                            {
                                stokRepo.SilStokHareketiByTedarikciVeTarih(kayitTemp.TedarikciId, kayitTemp.Tarih, conn, trans);
                            }

                            // Delete the main record
                            string deleteQuery = "DELETE FROM SutKayit WHERE SutKayitId = @SutKayitId";
                            using (var cmd = new SqlCommand(deleteQuery, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SutKayitId", sutKayitId);
                                int rowsAffected = cmd.ExecuteNonQuery();
                            }
                            
                            trans.Commit();
                            return true;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                throw;
            }
        }
    }
}
