using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

public static class NumaratorService
{
    public static string GenerateIncrementalCode(string type) // "S" veya "P"
    {
        string year = DateTime.Now.ToString("yy");
        int nextNumber = GetNextNumberFromDb(year, type);
        return $"{year}{type}{nextNumber:D5}";
    }

    private static int GetNextNumberFromDb(string year, string type)
    {
        int nextNumber = 1;

        string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT SonNumara FROM Numarator 
                        WITH (UPDLOCK, ROWLOCK)
                        WHERE Yil = @Yil AND Tip = @Tip", conn, tran);

                    cmd.Parameters.AddWithValue("@Yil", year);
                    cmd.Parameters.AddWithValue("@Tip", type);

                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        nextNumber = (int)result + 1;

                        SqlCommand updateCmd = new SqlCommand(@"
                            UPDATE Numarator 
                            SET SonNumara = @NextNumber 
                            WHERE Yil = @Yil AND Tip = @Tip", conn, tran);

                        updateCmd.Parameters.AddWithValue("@NextNumber", nextNumber);
                        updateCmd.Parameters.AddWithValue("@Yil", year);
                        updateCmd.Parameters.AddWithValue("@Tip", type);

                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        SqlCommand insertCmd = new SqlCommand(@"
                            INSERT INTO Numarator (Yil, Tip, SonNumara)
                            VALUES (@Yil, @Tip, @NextNumber)", conn, tran);

                        insertCmd.Parameters.AddWithValue("@Yil", year);
                        insertCmd.Parameters.AddWithValue("@Tip", type);
                        insertCmd.Parameters.AddWithValue("@NextNumber", nextNumber);

                        insertCmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        return nextNumber;
    }
}
