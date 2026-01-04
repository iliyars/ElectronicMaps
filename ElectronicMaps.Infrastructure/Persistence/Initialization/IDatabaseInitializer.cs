using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Initialization
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync(CancellationToken ct = default);
    }
}
