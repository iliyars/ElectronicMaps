using ElectronicMaps.Application.Abstractons.Commands;
using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Components;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Application.WorkspaceProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Services
{
    internal class WorkspaceActions : IWorkspaceActions
    {
        private readonly IComponentStore _store;
        private readonly ISaveComponent _saveComponent;
        private readonly IParameterDefinitionReadRepository _paramDefs;
        private readonly IFormTypeReadRepository _formTypes;

        public WorkspaceActions(IComponentStore store, ISaveComponent saveComponent, IParameterDefinitionReadRepository paramDefs, IFormTypeReadRepository formTypes)
        {
            _store = store;
            _saveComponent = saveComponent;
            _paramDefs = paramDefs;
            _formTypes = formTypes;
        }



        public async Task<IReadOnlyList<ParameterDefinitionDto>> LoadDefinitionsForDraftAsync(Guid draftId, CancellationToken ct)
        {
            var draft = _store.TryGetWorking(draftId)
            ?? throw new InvalidOperationException($"Draft {draftId} not found.");

            return await _paramDefs.GetByFormCodeAsync(draft.FormCode,ct);
        }

        public async Task SaveDraftToDatabaseAsync(Guid draftId, CancellationToken ct)
        {
            var draft = _store.TryGetWorking(draftId)
           ?? throw new InvalidOperationException($"Draft {draftId} not found.");

            var request = MapDraftToSaveRequest(draft);

            var result = await _saveComponent.ExecuteAsync(request, ct);
        }

        private SaveComponentRequest MapDraftToSaveRequest(ComponentDraft draft)
        {
            IReadOnlyList<ParameterValueInput> parameters = draft.NdtParametersOverrides.Values.Select(MapParam).ToList();

            var familyName = draft.Family ?? draft.FamilyKey;

            return new SaveComponentRequest(
                ComponentName: draft.Name,
                ComponentFormTypeCode: draft.FormCode,
                FamilyName: familyName,
                ExistingFamilyId: draft.DbFamilyId,
                ComponentParameters: parameters,
                FamilyParameters:)





         //  var componentParams = draft.NdtParametersOverrides.Values.Select(MapParam).ToList();
          //  var familyParams = draft.Pa


            return new SaveComponentRequest(
                ComponentName: draft.Name,
                ComponentFormTypeCode: draft.FormCode,
                ExistingFamilyId: draft.DbFamilyId,
                FamilyName: draft.Family,
                ComponentParameters: )
        }
    }
}
