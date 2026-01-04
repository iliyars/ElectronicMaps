using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Workspace.Services
{
    public interface IUndefinedComponentFillService
    {
        Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyDefinitionsAsync(CancellationToken ct); // форма 4
        Task<IReadOnlyList<FormTypeDto>> GetComponentFormTypesAsync(CancellationToken ct);
        Task<IReadOnlyList<ParameterDefinitionDto>> GetComponentDefinitionsAsync(string formTypeCode, CancellationToken ct);

        Task<SaveComponentResult> SaveAsync(
            Guid draftId,
            string familyName,
            IReadOnlyList<ParameterValueInput> familyParameters,
            string componentFormTypeCode,
            IReadOnlyList<ParameterValueInput> componentParameters,
            CancellationToken ct);
    }
}
