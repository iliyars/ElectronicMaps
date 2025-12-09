using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentCreationService
    {
        Task<ComponentDto> AddComponentAsync(ComponentCreateDto dto, CancellationToken ct = default);
    }
}
