using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Configuration
{
    public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {

            var basePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "ElectronicMaps.WPF"
    );

            // При миграциях стартовым проектом будет WPF, поэтому
            // CurrentDirectory будет указывать на папку Wpf-проекта, где лежит appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            var provider = DbConfig.GetProvider(config);
            var connectionString = DbConfig.BuildConnectionString(config);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)) 
            {

                // optionsBuilder.UseSqlServer(connectionString);
            }

            else
                optionsBuilder.UseSqlite(connectionString);


            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
