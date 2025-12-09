using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Repositories
{
    public interface IComponentCommandRepository
    {
        Task AddAsync(Entities.Component component, CancellationToken ct);

        Task AddFamilyAsync(Entities.ComponentFamily family, CancellationToken ct);

        Task AddParameterValuesAsync(IEnumerable<ParameterValue> values, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
