using ElectronicMaps.Application.DTO.Families;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IComponentFamilyReadRepository
    {
        Task<ComponentFamilyLookupDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ComponentFamilyLookupDto?> GetByNameAsync(string name, CancellationToken ct);

        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct);
    }
}
