using ElectronicMaps.Application.DTO.Components;
using ElectronicMaps.Application.DTO.Forms;
using ElectronicMaps.Application.DTO.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Services
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
