using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2.DataAccess
{
    public class SutEnvanteriRepository
    {
        private readonly string _connectionString;

        public SutEnvanteriRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        public void KaydetEnvanter(SutEnvanteri envanter)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO SutEnvanteri (Tarih, DevirSut, GunlukAlinanSut, GunlukSatilanSut, KalanSut, Aciklama, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                    VALUES (@Tarih, @DevirSut, @GunlukAlinanSut, @GunlukSatilanSut, @KalanSut, @Aciklama, @CreatedBy, @CreatedAt, @ModifiedBy, @ModifiedAt)", conn))
                {
                    cmd.Parameters.AddWithValue("@Tarih", envanter.Tarih);
                    cmd.Parameters.AddWithValue("@DevirSut", envanter.DevirSut);
                    cmd.Parameters.AddWithValue("@GunlukAlinanSut", envanter.GunlukAlinanSut);
                    cmd.Parameters.AddWithValue("@GunlukSatilanSut", envanter.GunlukSatilanSut);
                    cmd.Parameters.AddWithValue("@KalanSut", envanter.KalanSut);
                    cmd.Parameters.AddWithValue("@Aciklama", envanter.Aciklama ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", envanter.CreatedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", envanter.CreatedAt);
                    cmd.Parameters.AddWithValue("@ModifiedBy", envanter.ModifiedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedAt", envanter.ModifiedAt);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void GuncelleEnvanter(SutEnvanteri envanter)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE SutEnvanteri 
                    SET Tarih = @Tarih, 
                        DevirSut = @DevirSut, 
                        GunlukAlinanSut = @GunlukAlinanSut, 
                        GunlukSatilanSut = @GunlukSatilanSut, 
                        KalanSut = @KalanSut, 
                        Aciklama = @Aciklama, 
                        ModifiedBy = @ModifiedBy,
                        ModifiedAt = @ModifiedAt
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", envanter.Id);
                    cmd.Parameters.AddWithValue("@Tarih", envanter.Tarih);
                    cmd.Parameters.AddWithValue("@DevirSut", envanter.DevirSut);
                    cmd.Parameters.AddWithValue("@GunlukAlinanSut", envanter.GunlukAlinanSut);
                    cmd.Parameters.AddWithValue("@GunlukSatilanSut", envanter.GunlukSatilanSut);
                    cmd.Parameters.AddWithValue("@KalanSut", envanter.KalanSut);
                    cmd.Parameters.AddWithValue("@Aciklama", envanter.Aciklama ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedBy", envanter.ModifiedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedAt", envanter.ModifiedAt);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<SutEnvanteri> GetAllEnvanter()
        {
            var envanterler = new List<SutEnvanteri>();
            
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT Id, Tarih, DevirSut, GunlukAlinanSut, GunlukSatilanSut, KalanSut, Aciklama, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt
                    FROM SutEnvanteri 
                    ORDER BY Tarih DESC", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var envanter = new SutEnvanteri
                            {
                                Id = reader.GetInt32("Id"),
                                Tarih = reader.GetDateTime("Tarih"),
                                DevirSut = reader.GetDecimal("DevirSut"),
                                GunlukAlinanSut = reader.GetDecimal("GunlukAlinanSut"),
                                GunlukSatilanSut = reader.GetDecimal("GunlukSatilanSut"),
                                KalanSut = reader.GetDecimal("KalanSut"),
                                Aciklama = reader["Aciklama"]?.ToString(),
                                CreatedBy = reader["CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["CreatedBy"]) : (int?)null,
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                ModifiedBy = reader["ModifiedBy"] != DBNull.Value ? Convert.ToInt32(reader["ModifiedBy"]) : (int?)null,
                                ModifiedAt = reader.GetDateTime("ModifiedAt")
                            };
                            envanterler.Add(envanter);
                        }
                    }
                }
            }
            
            return envanterler;
        }

        public SutEnvanteri GetEnvanterByTarih(DateTime tarih)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT Id, Tarih, DevirSut, GunlukAlinanSut, GunlukSatilanSut, KalanSut, Aciklama, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt
                    FROM SutEnvanteri 
                    WHERE CAST(Tarih AS DATE) = @Tarih", conn))
                {
                    cmd.Parameters.AddWithValue("@Tarih", tarih.Date);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SutEnvanteri
                            {
                                Id = reader.GetInt32("Id"),
                                Tarih = reader.GetDateTime("Tarih"),
                                DevirSut = reader.GetDecimal("DevirSut"),
                                GunlukAlinanSut = reader.GetDecimal("GunlukAlinanSut"),
                                GunlukSatilanSut = reader.GetDecimal("GunlukSatilanSut"),
                                KalanSut = reader.GetDecimal("KalanSut"),
                                Aciklama = reader["Aciklama"]?.ToString(),
                                CreatedBy = reader["CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["CreatedBy"]) : (int?)null,
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                ModifiedBy = reader["ModifiedBy"] != DBNull.Value ? Convert.ToInt32(reader["ModifiedBy"]) : (int?)null,
                                ModifiedAt = reader.GetDateTime("ModifiedAt")
                            };
                        }
                    }
                }
            }
            
            return null;
        }

        public void SilEnvanter(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM SutEnvanteri WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}