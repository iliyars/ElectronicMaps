using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Persistence
{
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(
           Func<CancellationToken, Task> action,
           CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
