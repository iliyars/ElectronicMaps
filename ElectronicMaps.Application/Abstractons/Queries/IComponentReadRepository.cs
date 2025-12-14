using ElectronicMaps.Application.DTO.Components;
using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IComponentReadRepository
    {
        Task<ComponentLookUpDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ComponentLookUpDto?> GetByNameAsync(string name, CancellationToken ct);
        // TODO: Create DTO
        Task<IReadOnlyList<ComponentLookUpDto>> GetAllAsync(CancellationToken ct);

        Task<IReadOnlyList<ComponentLookUpDto>> GetByFormCodeAsync(string formCode, CancellationToken ct);
        Task<IReadOnlyList<ComponentLookUpDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct);

        Task<bool> ExistsAsync(string name, CancellationToken ct);


    }
}
