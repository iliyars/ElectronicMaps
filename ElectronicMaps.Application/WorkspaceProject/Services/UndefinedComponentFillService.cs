using ElectronicMaps.Application.Abstractons.Commands;
using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Components;
using ElectronicMaps.Application.DTO.Forms;
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
    public class UndefinedComponentFillService : IUndefinedComponentFillService
    {
        private readonly IComponentStore _store;
        private readonly ISaveComponent _saveComponent;

        private readonly IFormTypeReadRepository _formTypes;
        private readonly IParameterDefinitionReadRepository _paramDefs;

        public UndefinedComponentFillService(
            IComponentStore store,
            ISaveComponent saveComponent,
            IFormTypeReadRepository formTypes,
            IParameterDefinitionReadRepository paramDefs)
        {
            _store = store;
            _saveComponent = saveComponent;
            _formTypes = formTypes;
            _paramDefs = paramDefs;
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
