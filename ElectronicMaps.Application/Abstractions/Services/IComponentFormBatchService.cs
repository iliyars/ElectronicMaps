using ElectronicMaps.Application.DTOs.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Services
{
    public interface IComponentFormBatchService
    {
        Task<IReadOnlyList<ComponentFormResultDto>> BuildFormsAsync(
            IReadOnlyList<string> componentNames,
            CancellationToken ct = default);


    }
}
