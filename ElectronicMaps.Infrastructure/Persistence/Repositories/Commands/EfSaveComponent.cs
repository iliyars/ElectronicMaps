using ElectronicMaps.Application.Abstractions.Commands;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Repositories.Commands
{
    public class EfSaveComponent : ISaveComponent
    {
        private readonly AppDbContext _db;

        public EfSaveComponent(AppDbContext db)
        {
            _db = db;
        }

        public async Task<SaveComponentResult> ExecuteAsync(
            SaveComponentRequest request,
            CancellationToken ct)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct); // Открываем транзакцию, чтобы всё сохранилось атомарно


            var componentFormTypeId = await ResolveFormTypeByCodeAsync(request.ComponentFormTypeCode, ct);
            var familyFormTypeId = await ResolveFormTypeByCodeAsync(WorkspaceViewKeys.FamilyFormCode, ct);

            var (family, familyWasCreated) = await GetOrCreateFamilyAsync(request, familyFormTypeId, ct); // Находим или создаём семейство

            var (component, componentWasCreated) = await GetOrCreateComponentAsync(request, family.Id, componentFormTypeId, ct); // Находим или создаём семейство

            await ValidateDefinitionsBelongToForm(componentFormTypeId, request.ComponentParameters, ct); // Проверяем корректность параметров 
            await ValidateDefinitionsBelongToForm(family.FamilyFormTypeId,request.FamilyParameters,ct);

            await UpsertComponentValueAsync(component.Id, request.ComponentParameters, ct); //Сохраняет значения параметров компонента
            await UpsertFamilyValueAsync(family.Id, request.FamilyParameters, ct);


            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new SaveComponentResult(
                ComponentId: component.Id,
                ComponentWasCreated: componentWasCreated,
                ComponentVerificationStatus: component.VerificationStatus,

                ComponentFamilyId: family.Id,
                FamilyWasCreated: familyWasCreated,
                FamilyVerificationStatus: family.VerificationStatus
                );
        }

       
        private async Task<int> ResolveFormTypeByCodeAsync(
            string formTypeCode,
            CancellationToken ct)
        {
            var id = await _db.FormTypes.AsNoTracking()
               .Where(x => x.Code == formTypeCode)
               .Select(x => (int?)x.Id)
               .SingleOrDefaultAsync(ct);

            if (id is null)
                throw new InvalidOperationException($"FormType with Code='{formTypeCode}' not found.");

            return id.Value;
        }

        private async Task<(ComponentFamily family, bool familyWasCreated)> GetOrCreateFamilyAsync(
            SaveComponentRequest request,
            int familyFormTypeId,
            CancellationToken ct)
        {
            ComponentFamily? family = null;

            if(request.ExistingFamilyId is int familyId)
            {
                family = await _db.ComponentFamilies
                    .FirstOrDefaultAsync(f => f.Id == familyId, ct);

                if (family is null)
                    throw new InvalidOperationException($"ComponentFamily with Id={familyId} not found.");
            }
            else
            {
                    family = new ComponentFamily
                    {
                        Name = request.FamilyName,
                        FamilyFormTypeId = familyFormTypeId,
                        VerificationStatus = Domain.Enums.VerificationStatus.Unverified
                    };

                    _db.ComponentFamilies.Add(family);
                    return (family, true);
            }

            return (family, false);
        }

        private async Task<(Component component, bool created)> GetOrCreateComponentAsync(
            SaveComponentRequest request,
            int familyId, 
            int componentFormTypeId,
            CancellationToken ct)
        {
            var component = await _db.Components
                .FirstOrDefaultAsync(c => c.Name == request.ComponentName, ct);

            if (component is not null)
                return (component, false);

            component = new Component
            {
                Name = request.ComponentName,
                ComponentFamilyId = familyId,
                FormTypeId = componentFormTypeId,
                VerificationStatus = Domain.Enums.VerificationStatus.Unverified,
            };

            _db.Components.Add(component);
            return (component, true);
        }


        private async Task ValidateDefinitionsBelongToForm(
            int formTypeId,
            IReadOnlyList<ParameterValueInput> inputs,
            CancellationToken ct)
        {
            if (inputs.Count == 0)
                return;

            var defIds = inputs.Select(x => x.ParameterDefinitionId).Distinct().ToArray();

            var validCount = await _db.ParameterDefinitions.AsNoTracking()
                .Where(d => d.FormTypeId == formTypeId && defIds.Contains(d.Id))
                .CountAsync(ct);

            if (validCount != defIds.Length)
                throw new InvalidOperationException("Some ParameterDefinitionId do not belong to the selected form.");
        }

        private async Task UpsertComponentValueAsync(
            int componentId,
            IReadOnlyList<ParameterValueInput> inputs,
            CancellationToken ct)
        {
            UpsertValue(componentId, null, inputs, ct);
            //if (inputs.Count == 0) return; 

            //var defIds = inputs.Select(x => x.ParameterDefinitionId).Distinct().ToArray();

            //var existing = await _db.ParameterValues
            //    .Where(v => v.ComponentId == componentId && v.ComponentFamilyId == null && defIds.Contains(v.ParameterDefinitionId))
            //    .ToListAsync(ct);

            //var byDef = existing.ToDictionary(v => v.ParameterDefinitionId);

            //foreach(var input in inputs)
            //{
            //    if(byDef.TryGetValue(input.ParameterDefinitionId, out var value))
            //    {
            //        ApplyValue(value, input);
            //    }
            //    else
            //    {
            //        value = new ParameterValue
            //        {
            //            ParameterDefinitionId = input.ParameterDefinitionId,
            //            ComponentId = componentId,
            //            ComponentFamilyId = null
            //        };
            //        ApplyValue(value, input);
            //        _db.ParameterValues.Add(value);
            //    }
            //}
        }

        private async Task UpsertFamilyValueAsync(int familyId,
            IReadOnlyList<ParameterValueInput> inputs,
            CancellationToken ct)
        {
            UpsertValue(null, familyId, inputs, ct);
            //if (inputs.Count == 0)
            //    return;

            //var defIds = inputs.Select(x => x.ParameterDefinitionId).Distinct().ToArray();

            //var existing = await _db.ParameterValues
            //    .Where(v => v.ComponentFamilyId == familyId && v.ComponentId == null && defIds.Contains(v.ParameterDefinitionId))
            //    .ToListAsync(ct);

            //var byDef = existing.ToDictionary(v => v.ParameterDefinitionId);

            //foreach(var input in inputs)
            //{
            //    if(byDef.TryGetValue(input.ParameterDefinitionId, out var value))
            //    {
            //        ApplyValue(value, input);
            //    }
            //    else
            //    {
            //        value = new ParameterValue
            //        {
            //            ParameterDefinitionId = input.ParameterDefinitionId,
            //            ComponentFamilyId = familyId,
            //            ComponentId = null
            //        };
            //        ApplyValue(value, input);
            //        _db.ParameterValues.Add(value);
            //    }
            //}
        }
        
        private async Task UpsertValue(
            int? componentId,
            int? familyId,
            IReadOnlyList<ParameterValueInput> inputs,
            CancellationToken ct)
        {
            if (inputs.Count == 0) return;

            var defIds = inputs.Select(x=>x.ParameterDefinitionId).Distinct().ToArray();

            IQueryable<ParameterValue> query = _db.ParameterValues;
            if(componentId is not null)
            {
                query = query.Where(v =>
                    v.ComponentId == componentId.Value && v.ComponentFamilyId == null && defIds.Contains(v.ParameterDefinitionId));
            }
            else
            {
                query = query.Where(v =>
                    v.ComponentFamilyId == familyId!.Value && v.ComponentId == null && defIds.Contains(v.ParameterDefinitionId));
            }

            var existing = await query.ToListAsync();
            var byDef = existing.ToDictionary(x => x.ParameterDefinitionId);

            foreach(var input in inputs)
            {
                if(!byDef.TryGetValue(input.ParameterDefinitionId, out var value))
                {
                    value = new ParameterValue
                    {
                        ParameterDefinitionId = input.ParameterDefinitionId,
                        ComponentId = componentId,
                        ComponentFamilyId = familyId
                    };
                    _db.ParameterValues.Add(value);
                }

                ApplyValue(value, input);
            }
        }

        private static void ApplyValue(
            ParameterValue entity,
            ParameterValueInput input) 
        {
            entity.StringValue = input.StringValue;
            entity.DoubleValue = input.DoubleValue;
            entity.IntValue = input.IntValue;
            entity.Pins = input.Pins;
        }
    }
}
