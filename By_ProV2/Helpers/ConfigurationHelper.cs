using Microsoft.Extensions.Configuration;
using System.IO;

namespace By_ProV2.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _configuration;

        static ConfigurationHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            _configuration = builder.Build();
        }

        public static string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

        public static string GetAppSetting(string key)
        {
            return _configuration[key];
        }
    }
}