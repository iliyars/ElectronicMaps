using ElectronicMaps.Application.DTOs.Families;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Families
{
    public interface IComponentFamilyReadRepository
    {
        Task<ComponentFamilyLookupDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ComponentFamilyLookupDto?> GetByNameAsync(string name, CancellationToken ct);

        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct);
    }
}
