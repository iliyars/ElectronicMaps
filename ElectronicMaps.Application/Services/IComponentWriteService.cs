using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentWriteService
    {
        Task<int> CreateFamilyAndComponentAsync(NewFamilyAndComponentDto dto, CancellationToken ct);

        Task<int> CreateComponentInExistingFamilyAsync(NewComponentDto dto, CancellationToken ct);

    }
}
