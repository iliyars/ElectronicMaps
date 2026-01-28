using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public interface IComponentQueryService
    {
        Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyParameterDefinitionsAsync(CancellationToken ct =default);
        Task<IReadOnlyList<FormTypeDto>> GetAvailableComponentFormsAsync(CancellationToken ct = default);

        Task<IReadOnlyList<ParameterDefinitionDto>> GetComponentParameterDefinitionsAsync(
            string formTypeCode,
            CancellationToken ct = default);
    }
}
