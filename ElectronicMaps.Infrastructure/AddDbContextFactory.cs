using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure
{
    public class AddDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {

            // При миграциях стартовым проектом будет WPF, поэтому
            // CurrentDirectory будет указывать на папку Wpf-проекта, где лежит appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("AppDb")
                ?? "Data Source=electronicmaps.db"; // запасной вариант

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Здесь указываешь тот же провайдер и строку подключения,
            // что будешь использовать в приложении
            optionsBuilder.UseSqlite(connectionString);
            // или:
            // optionsBuilder.UseSqlServer("Server=...;Database=RcCards;Trusted_Connection=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
