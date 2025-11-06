using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2.DataAccess
{
    public class AuditTrailRepository
    {
        private readonly string _connectionString;

        public AuditTrailRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        public List<AuditTrailEntry> GetSutKayitAuditTrail()
        {
            var entries = new List<AuditTrailEntry>();
            
            // Query to get history of changes for SutKayit records
            string sql = @"
                SELECT 
                    sk.SutKayitId as RecordId,
                    sk.CreatedAt as Timestamp,
                    sk.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'SutKayit' as TableName,
                    'INSERT' as Operation,
                    sk.BelgeNo as RecordDescription
                FROM SutKayit sk
                LEFT JOIN Users u ON sk.CreatedBy = u.Id
                WHERE sk.CreatedBy IS NOT NULL
                UNION ALL
                SELECT 
                    sk.SutKayitId as RecordId,
                    sk.ModifiedAt as Timestamp,
                    sk.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'SutKayit' as TableName,
                    'UPDATE' as Operation,
                    sk.BelgeNo as RecordDescription
                FROM SutKayit sk
                LEFT JOIN Users u ON sk.ModifiedBy = u.Id
                WHERE sk.ModifiedBy IS NOT NULL AND sk.CreatedAt != sk.ModifiedAt
                UNION ALL
                SELECT 
                    c.ID as RecordId,
                    c.CreatedAt as Timestamp,
                    c.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'CASABIT' as TableName,
                    'INSERT' as Operation,
                    c.CARIKOD + ' - ' + ISNULL(c.CARIADI, '') as RecordDescription
                FROM CASABIT c
                LEFT JOIN Users u ON c.CreatedBy = u.Id
                WHERE c.CreatedBy IS NOT NULL
                UNION ALL
                SELECT 
                    c.ID as RecordId,
                    c.ModifiedAt as Timestamp,
                    c.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'CASABIT' as TableName,
                    'UPDATE' as Operation,
                    c.CARIKOD + ' - ' + ISNULL(c.CARIADI, '') as RecordDescription
                FROM CASABIT c
                LEFT JOIN Users u ON c.ModifiedBy = u.Id
                WHERE c.ModifiedBy IS NOT NULL AND c.CreatedAt != c.ModifiedAt
                UNION ALL
                SELECT 
                    s.STOKID as RecordId,
                    s.CreatedAt as Timestamp,
                    s.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'STOKSABITKART' as TableName,
                    'INSERT' as Operation,
                    s.STOKKODU + ' - ' + ISNULL(s.STOKADI, '') as RecordDescription
                FROM STOKSABITKART s
                LEFT JOIN Users u ON s.CreatedBy = u.Id
                WHERE s.CreatedBy IS NOT NULL
                UNION ALL
                SELECT 
                    s.STOKID as RecordId,
                    s.ModifiedAt as Timestamp,
                    s.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'STOKSABITKART' as TableName,
                    'UPDATE' as Operation,
                    s.STOKKODU + ' - ' + ISNULL(s.STOKADI, '') as RecordDescription
                FROM STOKSABITKART s
                LEFT JOIN Users u ON s.ModifiedBy = u.Id
                WHERE s.ModifiedBy IS NOT NULL AND s.CreatedAt != s.ModifiedAt
                ORDER BY Timestamp DESC";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new AuditTrailEntry
                            {
                                RecordId = reader.GetInt32("RecordId"),
                                TableName = reader["TableName"].ToString(),
                                Operation = reader["Operation"].ToString(),
                                Timestamp = reader.GetDateTime("Timestamp"),
                                UserId = reader["UserId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["UserId"]) : null,
                                Username = reader["Username"] as string,
                                FullName = reader["FullName"] as string,
                                RecordDescription = reader["RecordDescription"] as string
                            };
                            
                            // Only add entries with valid user information
                            if (entry.UserId.HasValue) 
                            {
                                entries.Add(entry);
                            }
                        }
                    }
                }
            }
            
            return entries;
        }

        public List<AuditTrailEntry> GetSutKayitAuditTrailByUser(int userId)
        {
            var entries = new List<AuditTrailEntry>();
            
            // Query to get history of changes made by a specific user
            string sql = @"
                SELECT 
                    sk.SutKayitId as RecordId,
                    sk.CreatedAt as Timestamp,
                    sk.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'SutKayit' as TableName,
                    'INSERT' as Operation,
                    sk.BelgeNo as RecordDescription
                FROM SutKayit sk
                LEFT JOIN Users u ON sk.CreatedBy = u.Id
                WHERE sk.CreatedBy = @UserId
                UNION ALL
                SELECT 
                    sk.SutKayitId as RecordId,
                    sk.ModifiedAt as Timestamp,
                    sk.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'SutKayit' as TableName,
                    'UPDATE' as Operation,
                    sk.BelgeNo as RecordDescription
                FROM SutKayit sk
                LEFT JOIN Users u ON sk.ModifiedBy = u.Id
                WHERE sk.ModifiedBy = @UserId AND sk.CreatedAt != sk.ModifiedAt
                UNION ALL
                SELECT 
                    c.ID as RecordId,
                    c.CreatedAt as Timestamp,
                    c.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'CASABIT' as TableName,
                    'INSERT' as Operation,
                    c.CARIKOD + ' - ' + ISNULL(c.CARIADI, '') as RecordDescription
                FROM CASABIT c
                LEFT JOIN Users u ON c.CreatedBy = u.Id
                WHERE c.CreatedBy = @UserId
                UNION ALL
                SELECT 
                    c.ID as RecordId,
                    c.ModifiedAt as Timestamp,
                    c.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'CASABIT' as TableName,
                    'UPDATE' as Operation,
                    c.CARIKOD + ' - ' + ISNULL(c.CARIADI, '') as RecordDescription
                FROM CASABIT c
                LEFT JOIN Users u ON c.ModifiedBy = u.Id
                WHERE c.ModifiedBy = @UserId AND c.CreatedAt != c.ModifiedAt
                UNION ALL
                SELECT 
                    s.STOKID as RecordId,
                    s.CreatedAt as Timestamp,
                    s.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'STOKSABITKART' as TableName,
                    'INSERT' as Operation,
                    s.STOKKODU + ' - ' + ISNULL(s.STOKADI, '') as RecordDescription
                FROM STOKSABITKART s
                LEFT JOIN Users u ON s.CreatedBy = u.Id
                WHERE s.CreatedBy = @UserId
                UNION ALL
                SELECT 
                    s.STOKID as RecordId,
                    s.ModifiedAt as Timestamp,
                    s.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'STOKSABITKART' as TableName,
                    'UPDATE' as Operation,
                    s.STOKKODU + ' - ' + ISNULL(s.STOKADI, '') as RecordDescription
                FROM STOKSABITKART s
                LEFT JOIN Users u ON s.ModifiedBy = u.Id
                WHERE s.ModifiedBy = @UserId AND s.CreatedAt != s.ModifiedAt
                ORDER BY Timestamp DESC";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new AuditTrailEntry
                            {
                                RecordId = reader.GetInt32("RecordId"),
                                TableName = reader["TableName"].ToString(),
                                Operation = reader["Operation"].ToString(),
                                Timestamp = reader.GetDateTime("Timestamp"),
                                UserId = reader["UserId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["UserId"]) : null,
                                Username = reader["Username"] as string,
                                FullName = reader["FullName"] as string,
                                RecordDescription = reader["RecordDescription"] as string
                            };
                            entries.Add(entry);
                        }
                    }
                }
            }
            
            return entries;
        }

        public List<AuditTrailEntry> GetAuditTrailByDateRange(DateTime startDate, DateTime endDate)
        {
            var entries = new List<AuditTrailEntry>();
            
            // Query to get history of changes within a date range
            string sql = @"
                SELECT 
                    sk.SutKayitId as RecordId,
                    sk.CreatedAt as Timestamp,
                    sk.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'SutKayit' as TableName,
                    'INSERT' as Operation,
                    sk.BelgeNo as RecordDescription
                FROM SutKayit sk
                LEFT JOIN Users u ON sk.CreatedBy = u.Id
                WHERE sk.CreatedBy IS NOT NULL AND sk.CreatedAt BETWEEN @StartDate AND @EndDate
                UNION ALL
                SELECT 
                    sk.SutKayitId as RecordId,
                    sk.ModifiedAt as Timestamp,
                    sk.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'SutKayit' as TableName,
                    'UPDATE' as Operation,
                    sk.BelgeNo as RecordDescription
                FROM SutKayit sk
                LEFT JOIN Users u ON sk.ModifiedBy = u.Id
                WHERE sk.ModifiedBy IS NOT NULL AND sk.ModifiedAt BETWEEN @StartDate AND @EndDate AND sk.CreatedAt != sk.ModifiedAt
                UNION ALL
                SELECT 
                    c.ID as RecordId,
                    c.CreatedAt as Timestamp,
                    c.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'CASABIT' as TableName,
                    'INSERT' as Operation,
                    c.CARIKOD + ' - ' + ISNULL(c.CARIADI, '') as RecordDescription
                FROM CASABIT c
                LEFT JOIN Users u ON c.CreatedBy = u.Id
                WHERE c.CreatedBy IS NOT NULL AND c.CreatedAt BETWEEN @StartDate AND @EndDate
                UNION ALL
                SELECT 
                    c.ID as RecordId,
                    c.ModifiedAt as Timestamp,
                    c.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'CASABIT' as TableName,
                    'UPDATE' as Operation,
                    c.CARIKOD + ' - ' + ISNULL(c.CARIADI, '') as RecordDescription
                FROM CASABIT c
                LEFT JOIN Users u ON c.ModifiedBy = u.Id
                WHERE c.ModifiedBy IS NOT NULL AND c.ModifiedAt BETWEEN @StartDate AND @EndDate AND c.CreatedAt != c.ModifiedAt
                UNION ALL
                SELECT 
                    s.STOKID as RecordId,
                    s.CreatedAt as Timestamp,
                    s.CreatedBy as UserId,
                    u.Username,
                    u.FullName,
                    'STOKSABITKART' as TableName,
                    'INSERT' as Operation,
                    s.STOKKODU + ' - ' + ISNULL(s.STOKADI, '') as RecordDescription
                FROM STOKSABITKART s
                LEFT JOIN Users u ON s.CreatedBy = u.Id
                WHERE s.CreatedBy IS NOT NULL AND s.CreatedAt BETWEEN @StartDate AND @EndDate
                UNION ALL
                SELECT 
                    s.STOKID as RecordId,
                    s.ModifiedAt as Timestamp,
                    s.ModifiedBy as UserId,
                    u.Username,
                    u.FullName,
                    'STOKSABITKART' as TableName,
                    'UPDATE' as Operation,
                    s.STOKKODU + ' - ' + ISNULL(s.STOKADI, '') as RecordDescription
                FROM STOKSABITKART s
                LEFT JOIN Users u ON s.ModifiedBy = u.Id
                WHERE s.ModifiedBy IS NOT NULL AND s.ModifiedAt BETWEEN @StartDate AND @EndDate AND s.CreatedAt != s.ModifiedAt
                ORDER BY Timestamp DESC";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new AuditTrailEntry
                            {
                                RecordId = reader.GetInt32("RecordId"),
                                TableName = reader["TableName"].ToString(),
                                Operation = reader["Operation"].ToString(),
                                Timestamp = reader.GetDateTime("Timestamp"),
                                UserId = reader["UserId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["UserId"]) : null,
                                Username = reader["Username"] as string,
                                FullName = reader["FullName"] as string,
                                RecordDescription = reader["RecordDescription"] as string
                            };
                            
                            // Only add entries with valid user information
                            if (entry.UserId.HasValue) 
                            {
                                entries.Add(entry);
                            }
                        }
                    }
                }
            }
            
            return entries;
        }
    }
}