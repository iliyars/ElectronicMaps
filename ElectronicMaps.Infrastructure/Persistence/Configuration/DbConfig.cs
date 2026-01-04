using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ElectronicMaps.Infrastructure.Persistence.Configuration
{
    public static class DbConfig
    {

        public static string GetProvider(IConfiguration cfg) =>
            cfg["Database:Provider"]?.Trim() ?? "Sqlite";

        public static string BuildConnectionString(IConfiguration cfg)
        {
            var provider = GetProvider(cfg);

            if(provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                return cfg["Database:ConnectionString"] 
                    ?? throw new InvalidOperationException("Connection string 'Database:ConnectionString' not found.");

            //Sqlite
            var path = cfg["Database:Path"] ?? string.Empty;
            path = ExpandVars(path);

            if(string.IsNullOrWhiteSpace(path))
            {
                var baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ElectronicMaps");
                path = Path.Combine(baseDir, "electronicmaps.db");
            }

            var dir = Path.GetDirectoryName(path);
            if(!string.IsNullOrWhiteSpace(dir)) 
                Directory.CreateDirectory(dir);

            return $"Data Source=\"{path}\"";

        }

        private static string ExpandVars(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            return value.Replace("%LOCALAPPDATA%", 
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase);
        }
    }
}
