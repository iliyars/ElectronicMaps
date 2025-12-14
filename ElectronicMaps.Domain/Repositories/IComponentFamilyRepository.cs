using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Repositories
{
    public interface IComponentFamilyRepository
    {
        Task<ComponentFamily?> GetByIdAsync(int id, CancellationToken ct);
        Task<ComponentFamily?> GetByNameAsync(string name, CancellationToken ct);

        Task AddAsync(ComponentFamily componentFamily, CancellationToken ct);

        void Update(ComponentFamily componentFamily);

        void Remove(ComponentFamily componentFamily);
    }
}
