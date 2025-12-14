using ElectronicMaps.Application.DTO.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IComponentDetailsQuery
    {
        Task<ComponentDetailsDto> GetAsync(int componentId, CancellationToken ct);
    }
}
