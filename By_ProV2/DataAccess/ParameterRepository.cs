using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.DataAccess;
using By_ProV2.Helpers;

namespace By_ProV2.DataAccess
{
    public class ParameterRepository
    {
        private readonly string _connectionString;

        public ParameterRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        public void KaydetParametre(Parameter param)
        {
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // First, check if there are any existing records in the table
                        string countQuery = "SELECT COUNT(1) FROM Parametreler";
                        using (var countCmd = new SqlCommand(countQuery, conn, tran))
                        {
                            int recordCount = Convert.ToInt32(countCmd.ExecuteScalar());
                            
                            if (recordCount > 0)
                            {
                                // If records exist, update the existing ones instead of inserting
                                string updateQuery = @"
                                    UPDATE Parametreler 
                                    SET YagKesintiParametresi = @YagKesintiParametresi,
                                        ProteinParametresi = @ProteinParametresi,
                                        DizemBasiTl = @DizemBasiTl,
                                        CreatedAt = @CreatedAt
                                ";
                                using (var updateCmd = new SqlCommand(updateQuery, conn, tran))
                                {
                                    updateCmd.Parameters.AddWithValue("@YagKesintiParametresi", param.YagKesintiParametresi ?? (object)DBNull.Value);
                                    updateCmd.Parameters.AddWithValue("@ProteinParametresi", param.ProteinParametresi ?? (object)DBNull.Value);
                                    updateCmd.Parameters.AddWithValue("@DizemBasiTl", param.DizemBasiTl ?? (object)DBNull.Value);
                                    updateCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // If no records exist, insert a new one
                                string insertQuery = @"
                                    INSERT INTO Parametreler (YagKesintiParametresi, ProteinParametresi, DizemBasiTl, CreatedAt)
                                    VALUES (@YagKesintiParametresi, @ProteinParametresi, @DizemBasiTl, @CreatedAt)
                                ";
                                using (var insertCmd = new SqlCommand(insertQuery, conn, tran))
                                {
                                    insertCmd.Parameters.AddWithValue("@YagKesintiParametresi", param.YagKesintiParametresi ?? (object)DBNull.Value);
                                    insertCmd.Parameters.AddWithValue("@ProteinParametresi", param.ProteinParametresi ?? (object)DBNull.Value);
                                    insertCmd.Parameters.AddWithValue("@DizemBasiTl", param.DizemBasiTl ?? (object)DBNull.Value);
                                    insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                                    insertCmd.ExecuteNonQuery();
                                }
                            }
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
        }

        public List<Parameter> GetAllParametreler()
        {
            var parametreler = new List<Parameter>();
            try
            {
                string sql = @"
                    SELECT 
                        ParametreId,
                        YagKesintiParametresi,
                        ProteinParametresi,
                        DizemBasiTl,
                        CreatedAt
                    FROM Parametreler
                    ORDER BY CreatedAt DESC"; // Most recent first

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var param = new Parameter
                                {
                                    ParametreId = reader.GetInt32("ParametreId"),
                                    YagKesintiParametresi = reader.IsDBNull(reader.GetOrdinal("YagKesintiParametresi")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("YagKesintiParametresi")),
                                    ProteinParametresi = reader.IsDBNull(reader.GetOrdinal("ProteinParametresi")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("ProteinParametresi")),
                                    DizemBasiTl = reader.IsDBNull(reader.GetOrdinal("DizemBasiTl")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DizemBasiTl")),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };
                                parametreler.Add(param);
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
            return parametreler;
        }

        // Method to get the most recent parameters
        public Parameter GetLatestParametreler()
        {
            try
            {
                string sql = @"
                    SELECT TOP 1
                        ParametreId,
                        YagKesintiParametresi,
                        ProteinParametresi,
                        DizemBasiTl,
                        CreatedAt
                    FROM Parametreler
                    ORDER BY CreatedAt DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Parameter
                                {
                                    ParametreId = reader.GetInt32("ParametreId"),
                                    YagKesintiParametresi = reader.IsDBNull(reader.GetOrdinal("YagKesintiParametresi")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("YagKesintiParametresi")),
                                    ProteinParametresi = reader.IsDBNull(reader.GetOrdinal("ProteinParametresi")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("ProteinParametresi")),
                                    DizemBasiTl = reader.IsDBNull(reader.GetOrdinal("DizemBasiTl")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DizemBasiTl")),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };
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
            return null;
        }
    }
}