using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Repositories
{
    public interface IComponentRepository
    {
        Task<Component?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Component?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task AddAsync(Component component, CancellationToken cancellationToken);

        void Update(Component component);
        void Remove(Component component);
    }
}
