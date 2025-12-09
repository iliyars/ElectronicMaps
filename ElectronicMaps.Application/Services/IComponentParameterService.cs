using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public interface IComponentParameterService
    {

        Task<ComponentFormResultDto> GetComponentFormAsync(int componentId, CancellationToken ct);

        Task UpdateComponentParameterAsync(ComponentParameterUpdateDto dto, CancellationToken ct);



    }
}
