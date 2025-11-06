using System;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2.DataAccess
{
    public class CariRepository
    {
        private readonly string _connectionString;

        public CariRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        public int GetOrCreateCari(string kod, string ad, string tipi, SqlConnection conn, SqlTransaction tran)
        {
            if (string.IsNullOrWhiteSpace(kod))
                return 0;

            // 1️⃣ Cari kontrolü
            string checkQuery = "SELECT CariId FROM Cari WHERE CariKod = @Kod";
            using (var checkCmd = new SqlCommand(checkQuery, conn, tran))
            {
                checkCmd.Parameters.AddWithValue("@Kod", kod);
                var result = checkCmd.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
            }

            // 2️⃣ Yoksa yeni ekle
            string insertQuery = @"
        INSERT INTO Cari (CariKod, CariAdi, Tipi)
        VALUES (@Kod, @Adi, @Tipi);
        SELECT SCOPE_IDENTITY();";

            using (var insertCmd = new SqlCommand(insertQuery, conn, tran))
            {
                insertCmd.Parameters.AddWithValue("@Kod", kod);
                insertCmd.Parameters.AddWithValue("@Adi", ad ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Tipi", tipi ?? (object)DBNull.Value);
                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }

    }
}
