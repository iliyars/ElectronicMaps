using ElectronicMaps.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistance
{
    public class EfUnitOfWork : IUnitOfWork
    {

        private readonly AppDbContext _db;

        public EfUnitOfWork(AppDbContext db)
        {
            _db = db;
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            await action(ct);

            await tx.CommitAsync();
        }
    }
}
