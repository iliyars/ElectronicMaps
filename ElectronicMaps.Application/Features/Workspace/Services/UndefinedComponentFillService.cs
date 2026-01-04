using ElectronicMaps.Application.Abstractions.Commands;
using ElectronicMaps.Application.Abstractions.Queries;
using ElectronicMaps.Application.Abstractions.Queries.Forms;
using ElectronicMaps.Application.Abstractions.Queries.Parameters;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Application.Stores;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Workspace.Services
{
    public class UndefinedComponentFillService : IUndefinedComponentFillService
    {
        private readonly IComponentStore _store;
        private readonly ISaveComponent _saveComponent;
        private readonly IFormTypeReadRepository _formTypes;
        private readonly IParameterDefinitionReadRepository _paramDefs;
        private readonly ILogger<UndefinedComponentFillService> _logger;

        public UndefinedComponentFillService(
            IComponentStore store,
            ISaveComponent saveComponent,
            IFormTypeReadRepository formTypes,
            IParameterDefinitionReadRepository paramDefs,
            ILogger<UndefinedComponentFillService> logger)
        {
            _store = store;
            _saveComponent = saveComponent;
            _formTypes = formTypes;
            _paramDefs = paramDefs;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyDefinitionsAsync(CancellationToken ct)
        {
            var familyForm = await _formTypes.GetByCodeAsync(WorkspaceViewKeys.FamilyFormCode, ct);
            return await _paramDefs.GetByFormTypeIdAsync(familyForm.Id, ct);
        }

        public Task<IReadOnlyList<FormTypeDto>> GetComponentFormTypesAsync(CancellationToken ct)
        {
            return _formTypes.GetAllAsync(ct);
        }

        public async Task<IReadOnlyList<ParameterDefinitionDto>> GetComponentDefinitionsAsync(string formTypeCode, CancellationToken ct)
        {
            var form = await _formTypes.GetByCodeAsync(formTypeCode, ct);

            return await _paramDefs.GetByFormTypeIdAsync(form.Id, ct);
        }

        public async Task<SaveComponentResult> SaveAsync(
            Guid draftId,
            string familyName,
            IReadOnlyList<ParameterValueInput> familyParameters,
            string componentFormTypeCode,
            IReadOnlyList<ParameterValueInput> componentParameters,
            CancellationToken ct)
        {
            var draft = _store.TryGetWorking(draftId);

            // Готовим request для БД
            var request = new SaveComponentRequest(
                ComponentName: draft.Name,
                ComponentFormTypeCode: componentFormTypeCode,
                ExistingFamilyId: draft.DbFamilyId,
                FamilyName: familyName,
                ComponentParameters: componentParameters,
                FamilyParameters: familyParameters
                );

            // Сохраняем в БД
            var result = await _saveComponent.ExecuteAsync(request, ct);

            // Обновляем Store
            var savedForm = await _formTypes.GetByCodeAsync(request.ComponentFormTypeCode, ct);

            _store.UpdateWorking(draftId, d => d with
            {
                DBComponentId = result.ComponentId,
                DbFamilyId = result.ComponentFamilyId,

                FormCode = savedForm.Code,
                FormName = savedForm.Name,

                LocalFillStatus = LocalFillStatus.Filled
            });

            PatchFamilyAgregateIfExists(draft, result.ComponentFamilyId);

            return result;
        }

        private void PatchFamilyAgregateIfExists(
            ComponentDraft draft,
            int dbFamilyId)
        {
            var familyDrafts = _store.GetWorkingForView(WorkspaceViewKeys.FamilyFormCode);
            var familyAgregate = familyDrafts.FirstOrDefault(d =>
                d.Kind == DraftKind.FamilyAgregate && string.Equals(d.FamilyKey, draft.FamilyKey, StringComparison.OrdinalIgnoreCase));

            if(familyAgregate is null)
            {
                return;
            }


            _store.UpdateWorking(familyAgregate.Id, d => d with
            {
                DbFamilyId = dbFamilyId,
                LocalFillStatus = LocalFillStatus.Filled
            });
               
        }
    }
}
