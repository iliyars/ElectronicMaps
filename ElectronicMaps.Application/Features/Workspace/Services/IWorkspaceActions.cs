using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Workspace.Services
{
    public interface IWorkspaceActions
    {
        Task SaveDraftToDatabaseAsync(Guid draftId, CancellationToken ct);

        Task<IReadOnlyList<ParameterDefinitionDto>> LoadDefinitionsForDraftAsync(Guid draftId, CancellationToken ct);   
    }
}
