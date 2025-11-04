using System;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;

namespace By_ProV2.DataAccess
{
    public class DepoStokRepository
    {
        private readonly string _connectionString;

        public DepoStokRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["db"].ConnectionString;
        }

        public void KaydetStokHareketi(SutKaydi kayit, SqlConnection conn, SqlTransaction tran)
        {
            if (kayit == null || kayit.Miktar <= 0)
                return;

            string query = @"
        INSERT INTO DepoStok (Tarih, TedarikciId, Miktar, Yag, Protein, TKM)
        VALUES (@Tarih, @TedarikciId, @Miktar, @Yag, @Protein, @TKM)";

            using (var cmd = new SqlCommand(query, conn, tran))
            {
                cmd.Parameters.AddWithValue("@Tarih", kayit.Tarih);
                cmd.Parameters.AddWithValue("@TedarikciId", kayit.TedarikciId == 0 ? (object)DBNull.Value : kayit.TedarikciId);
                cmd.Parameters.AddWithValue("@Miktar", kayit.Miktar);
                cmd.Parameters.AddWithValue("@Yag", kayit.Yag ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Protein", kayit.Protein ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TKM", kayit.TKM ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public void GuncelleStokHareketi(SutKaydi kayit, SqlConnection conn, SqlTransaction tran)
        {
            if (kayit == null || kayit.Miktar <= 0)
                return;

            // First delete the existing stock movement for this record if it exists
            string deleteQuery = @"
                DELETE FROM DepoStok 
                WHERE TedarikciId = @TedarikciId AND Tarih = @Tarih";

            using (var deleteCmd = new SqlCommand(deleteQuery, conn, tran))
            {
                deleteCmd.Parameters.AddWithValue("@TedarikciId", kayit.TedarikciId == 0 ? (object)DBNull.Value : kayit.TedarikciId);
                deleteCmd.Parameters.AddWithValue("@Tarih", kayit.Tarih);
                deleteCmd.ExecuteNonQuery();
            }

            // Then add the updated values
            string insertQuery = @"
        INSERT INTO DepoStok (Tarih, TedarikciId, Miktar, Yag, Protein, TKM)
        VALUES (@Tarih, @TedarikciId, @Miktar, @Yag, @Protein, @TKM)";

            using (var cmd = new SqlCommand(insertQuery, conn, tran))
            {
                cmd.Parameters.AddWithValue("@Tarih", kayit.Tarih);
                cmd.Parameters.AddWithValue("@TedarikciId", kayit.TedarikciId == 0 ? (object)DBNull.Value : kayit.TedarikciId);
                cmd.Parameters.AddWithValue("@Miktar", kayit.Miktar);
                cmd.Parameters.AddWithValue("@Yag", kayit.Yag ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Protein", kayit.Protein ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TKM", kayit.TKM ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

    }
}
