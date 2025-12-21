using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Initialization
{
    public class DatabaseInitializer : IDatabaseInitializer
    {

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public DatabaseInitializer(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await _db.Database.MigrateAsync(ct);

            if (DbConfig.GetProvider(_config).Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                await _db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", ct);
                await _db.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;", ct);
                await _db.Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;", ct);
            }
        }
    }
}
