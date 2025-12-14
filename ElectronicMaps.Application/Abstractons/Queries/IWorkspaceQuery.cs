using ElectronicMaps.Application.DTO.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IWorkspaceQuery
    {
        Task<IReadOnlyList<ComponentListItemDto>> GetAllComponentsAsync(CancellationToken ct);

        Task<IReadOnlyList<ComponentListItemDto>> GetComponentsByFormAsync(
            string formCode,
            CancellationToken ct);
    }
}
