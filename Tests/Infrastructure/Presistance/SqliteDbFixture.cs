using ElectronicMaps.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Tests.Infrastructure.Presistance
{
    public class SqliteDbFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public DbContextOptions<AppDbContext> Options { get; }

        public SqliteDbFixture()
        {
            //1) In-Memory Sqlite живет только пока открыто соединение
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            //2) Настраиваем DbContext на Sqlite
            Options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .EnableSensitiveDataLogging()
                .Options;
        }

        public async Task<AppDbContext> CreateDbAsync()
        {
            var db = new AppDbContext(Options);


            //3) Создаём схему по OnModelCreating
            await db.Database.EnsureCreatedAsync();
            return db;
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.CloseAsync();
        }
    }
}
