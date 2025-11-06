# By_ProV2 Configuration Guide

## Database Connection Configuration

This project uses a configuration file to manage database connection strings, allowing for environment-specific settings without hardcoding connection strings in the source code.

### Configuration Files

1. **appsettings.json** - The main configuration file that contains the default database connection string
2. **appsettings.json.example** - Example configuration file for reference

### Setting up for Your Environment

1. Copy `appsettings.json.example` to `appsettings.json` if it doesn't exist
2. Modify the connection string in `appsettings.json` to match your database environment
3. The application will automatically use the configured connection string

### Connection String Format

The default connection string format is:
```
Server=localhost;Database=BeryemERP;Trusted_Connection=true;TrustServerCertificate=true;
```

You can modify this to match your specific database setup:
- Change `Server` to your SQL Server instance
- Change `Database` to your database name
- Update authentication (Windows Authentication or SQL Server Authentication)
- Add any additional parameters needed for your environment

### Configuration Helper

The application uses `ConfigurationHelper.cs` to read connection strings from the JSON file using the Microsoft.Extensions.Configuration framework.