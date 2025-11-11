# Migration from SQL Server to PostgreSQL Guide

This document provides a comprehensive guide for migrating the By_ProV2 application from SQL Server to PostgreSQL. This is a significant undertaking that requires careful planning and execution.

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Database Schema Migration](#database-schema-migration)
4. [Code Changes Required](#code-changes-required)
5. [NuGet Package Updates](#nuget-package-updates)
6. [Connection String Configuration](#connection-string-configuration)
7. [SQL Query Modifications](#sql-query-modifications)
8. [Data Type Mappings](#data-type-mappings)
9. [Function and Syntax Differences](#function-and-syntax-differences)
10. [Testing Strategy](#testing-strategy)
11. [Deployment Considerations](#deployment-considerations)

## Overview

The By_ProV2 application currently uses Microsoft SQL Server with raw ADO.NET commands. Migrating to PostgreSQL requires:
- Updating NuGet packages
- Modifying connection strings
- Converting SQL queries
- Updating data access code
- Reconfiguring database schema
- Thorough testing

## Prerequisites

### 1. PostgreSQL Installation
- Install PostgreSQL (version 12 or higher recommended)
- Install pgAdmin or another PostgreSQL management tool
- Create a database user with appropriate permissions

### 2. Development Environment
- Install Npgsql NuGet packages
- Update development database connection
- Ensure PostgreSQL service is running

### 3. Backup Strategy
- Create a full backup of current SQL Server database
- Keep the backup available for rollback purposes
- Document the current database schema with data types and relationships

## Database Schema Migration

### 1. Schema Analysis
First, analyze the current SQL Server schema by examining the database creation scripts and data access files:

```sql
-- Example table structure in SQL Server
CREATE TABLE CASABIT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    CARIKOD NVARCHAR(50) NOT NULL UNIQUE,
    CARIADI NVARCHAR(255) NOT NULL,
    ADRES NVARCHAR(MAX),
    TELEFON NVARCHAR(20),
    YETKILIKISI NVARCHAR(100),
    BAGLICARIKOD NVARCHAR(50),
    VERGIDAIRESI NVARCHAR(100),
    VERGINO NVARCHAR(50),
    ISK1 DECIMAL(18,2) DEFAULT 0,
    ISK2 DECIMAL(18,2) DEFAULT 0,
    ISK3 DECIMAL(18,2) DEFAULT 0,
    ISK4 DECIMAL(18,2) DEFAULT 0,
    KKISK1 DECIMAL(18,2) DEFAULT 0,
    KKISK2 DECIMAL(18,2) DEFAULT 0,
    KKISK3 DECIMAL(18,2) DEFAULT 0,
    KKISK4 DECIMAL(18,2) DEFAULT 0,
    NAKISK DECIMAL(18,2) DEFAULT 0,
    PLAKA1 NVARCHAR(20),
    PLAKA2 NVARCHAR(20),
    PLAKA3 NVARCHAR(20),
    SOFORADSOYAD NVARCHAR(100),
    KAYITTARIHI DATETIME,
    SUTFIYATI DECIMAL(18,2) DEFAULT 0,
    NAKFIYATI DECIMAL(18,2) DEFAULT 0,
    CreatedBy INT,
    ModifiedBy INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    ModifiedAt DATETIME2
);
```

### 2. PostgreSQL Schema Creation
Convert the schema to PostgreSQL format:

```sql
CREATE TABLE casabit (
    id SERIAL PRIMARY KEY,
    carikod VARCHAR(50) NOT NULL UNIQUE,
    cariadi TEXT NOT NULL,
    adres TEXT,
    telefon VARCHAR(20),
    yetkilikisi VARCHAR(100),
    baglicarikod VARCHAR(50),
    vergidaire VARCHAR(100),
    vergino VARCHAR(50),
    isk1 NUMERIC(18,2) DEFAULT 0,
    isk2 NUMERIC(18,2) DEFAULT 0,
    isk3 NUMERIC(18,2) DEFAULT 0,
    isk4 NUMERIC(18,2) DEFAULT 0,
    kkisk1 NUMERIC(18,2) DEFAULT 0,
    kkisk2 NUMERIC(18,2) DEFAULT 0,
    kkisk3 NUMERIC(18,2) DEFAULT 0,
    kkisk4 NUMERIC(18,2) DEFAULT 0,
    nakisk NUMERIC(18,2) DEFAULT 0,
    plaka1 VARCHAR(20),
    plaka2 VARCHAR(20),
    plaka3 VARCHAR(20),
    soforyadsyad VARCHAR(100),
    kayittarihi TIMESTAMP,
    sutfiyati NUMERIC(18,2) DEFAULT 0,
    nakfiyati NUMERIC(18,2) DEFAULT 0,
    createdby INTEGER,
    modifiedby INTEGER,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modifiedat TIMESTAMP
);
```

### 3. Additional Tables
Identify and convert all other tables in your database schema following the same pattern.

## Code Changes Required

### 1. Namespace Updates
Replace all Microsoft.Data.SqlClient references with Npgsql:

```csharp
// Before
using Microsoft.Data.SqlClient;

// After 
using Npgsql;
```

### 2. Connection Class Updates
Update your ConfigurationHelper.cs file:

```csharp
public static class ConfigurationHelper
{
    private static readonly string _connectionString = 
        ConfigurationManager.ConnectionStrings["db"].ConnectionString;

    public static string GetConnectionString(string configName)
    {
        return _connectionString;
    }
}
```

### 3. SqlCommand to NpgsqlCommand
Convert all SqlCommand usages:

```csharp
// Before
using (SqlConnection conn = new SqlConnection(connectionString))
{
    conn.Open();
    string sql = "SELECT * FROM CASABIT WHERE CARIKOD = @CARIKOD";
    using (SqlCommand cmd = new SqlCommand(sql, conn))
    {
        cmd.Parameters.AddWithValue("@CARIKOD", cariKod);
        // ... execute query
    }
}

// After
using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
{
    conn.Open();
    string sql = "SELECT * FROM casabit WHERE carikod = $1";
    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
    {
        cmd.Parameters.AddWithValue(cariKod);
        // ... execute query
    }
}
```

## NuGet Package Updates

### 1. Remove Old Packages
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="..." />
```

### 2. Add PostgreSQL Packages
```xml
<PackageReference Include="Npgsql" Version="6.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="6.0.0" />
```

## Connection String Configuration

### 1. Update App.config
Replace SQL Server connection string with PostgreSQL format:

```xml
<!-- Before -->
<connectionStrings>
    <add name="db" 
         connectionString="Server=localhost;Database=By_ProV2;Trusted_Connection=true;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>

<!-- After -->
<connectionStrings>
    <add name="db" 
         connectionString="Host=localhost;Database=by_prov2;Username=your_username;Password=your_password;" 
         providerName="Npgsql" />
</connectionStrings>
```

### 2. Alternative Connection String Formats
```text
# With specific port
Host=localhost;Port=5432;Database=by_prov2;Username=your_username;Password=your_password;

# With SSL
Host=localhost;Database=by_prov2;Username=your_username;Password=your_password;SSL Mode=Prefer;
```

## SQL Query Modifications

### 1. Parameter Syntax
- SQL Server: `@param`
- PostgreSQL: `$1, $2, $3`

```csharp
// Before
string sql = @"SELECT * FROM CASABIT 
               WHERE CARIKOD = @CARIKOD AND CARIADI LIKE @CARIADI";
cmd.Parameters.AddWithValue("@CARIKOD", cariKod);
cmd.Parameters.AddWithValue("@CARIADI", $"%{searchTerm}%");

// After
string sql = @"SELECT * FROM casabit 
               WHERE carikod = $1 AND cariadi LIKE $2";
cmd.Parameters.AddWithValue(cariKod);
cmd.Parameters.AddWithValue($"%{searchTerm}%");
```

### 2. Common Query Conversions

#### SELECT with LIMIT
```sql
-- SQL Server (Before)
SELECT TOP 10 * FROM CASABIT

-- PostgreSQL (After)
SELECT * FROM casabit LIMIT 10
```

#### String Functions
```sql
-- SQL Server (Before)
WHERE CARIKOD LIKE '%' + @CARIKOD + '%'

-- PostgreSQL (After)
WHERE carikod LIKE $1
-- (Parameter value would be '%value%')
```

#### Date Functions
```sql
-- SQL Server (Before)
WHERE KAYITTARIHI >= GETDATE()

-- PostgreSQL (After)
WHERE kayittarihi >= CURRENT_TIMESTAMP
```

#### Case Insensitive Comparison
```sql
-- SQL Server (Before)
WHERE UPPER(CARIADI) = UPPER(@CARIADI)

-- PostgreSQL (After)
WHERE cariadi ILIKE $1
-- Or
WHERE UPPER(cariadi) = UPPER($1)
```

## Data Type Mappings

### 1. Basic Type Conversions
| SQL Server | PostgreSQL | Notes |
|------------|------------|-------|
| INT IDENTITY | SERIAL | Auto-incrementing integer |
| BIGINT IDENTITY | BIGSERIAL | Auto-incrementing big integer |
| NVARCHAR(n) | VARCHAR(n) | Variable length string |
| NVARCHAR(MAX) | TEXT | Long text field |
| DATETIME | TIMESTAMP | Date and time |
| DATETIME2 | TIMESTAMP | Precise date and time |
| DECIMAL(p,s) | NUMERIC(p,s) | Exact numeric |
| BIT | BOOLEAN | True/false |
| VARBINARY | BYTEA | Binary data |

### 2. Update Model Classes
If you have any model classes mapping to database types, update them accordingly.

## Function and Syntax Differences

### 1. String Functions
```sql
-- SQL Server
SELECT UPPER(CARIADI), LEN(CARIKOD)

-- PostgreSQL
SELECT UPPER(cariadi), LENGTH(carikod)
```

### 2. Date Functions
```sql
-- SQL Server
SELECT GETDATE(), DATEADD(day, 1, KAYITTARIHI)

-- PostgreSQL
SELECT CURRENT_TIMESTAMP, kayittarihi + INTERVAL '1 day'
```

### 3. Null Handling
```sql
-- SQL Server
SELECT ISNULL(CARIADI, 'Unknown')

-- PostgreSQL
SELECT COALESCE(cariadi, 'Unknown')
```

### 4. Conditional Logic
```sql
-- SQL Server
SELECT CASE WHEN ISNULL(ISK1, 0) > 0 THEN 'Yes' ELSE 'No' END

-- PostgreSQL
SELECT CASE WHEN isk1 IS NOT NULL AND isk1 > 0 THEN 'Yes' ELSE 'No' END
```

## Testing Strategy

### 1. Unit Testing
Create unit tests for each data access method:
- Test connection establishment
- Test CRUD operations
- Test parameter binding
- Test error handling

### 2. Integration Testing
- Test all forms that interact with the database
- Verify all reports work correctly
- Test data validation
- Test transaction handling

### 3. Data Migration Testing
- Verify data integrity after migration
- Test all relationships
- Verify computed values
- Test all business logic

### 4. Performance Testing
- Compare query performance
- Test concurrent access
- Verify memory usage
- Check for connection leaks

## Deployment Considerations

### 1. Production Migration
- Plan a maintenance window
- Create full backup before migration
- Test migration process in staging environment
- Prepare rollback plan

### 2. Data Migration
- Export data from SQL Server in a compatible format
- Import data into PostgreSQL
- Verify data integrity
- Update any auto-incrementing sequences

### 3. Application Deployment
- Update production connection strings
- Deploy updated application files
- Monitor application logs
- Test all functionality in production

### 4. Rollback Plan
- Keep SQL Server database and application
- Document how to revert to SQL Server
- Test rollback process before going live

## Entity Framework Alternative (Recommended)

Consider migrating to Entity Framework Core with PostgreSQL provider for easier maintenance:

### 1. Install Packages
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="6.0.0" />
```

### 2. Create DbContext
```csharp
public class ByProV2Context : DbContext
{
    public DbSet<Cari> Cari { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}
```

### 3. Create Entity Models
```csharp
public class Cari
{
    public int Id { get; set; }
    public string CariKod { get; set; }
    public string CariAdi { get; set; }
    // ... other properties
}
```

This approach is more complex upfront but will make future database changes much easier.

## Conclusion

Migrating from SQL Server to PostgreSQL is a significant undertaking that requires careful planning and thorough testing. The raw SQL approach used in your current application will require extensive code changes throughout the project. Consider the Entity Framework Core approach for easier maintenance in the future.

Key success factors:
- Thorough testing of all functionality
- Careful data migration
- Proper connection handling
- Performance monitoring
- Rollback planning