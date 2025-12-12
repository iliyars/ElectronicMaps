using ElectronicMaps.Application.DTO;
using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public class ComponentWriteService : IComponentWriteService
    {
        private readonly IComponentCommandRepository _commands;
        private readonly IComponentQueryRepository _query;
        private readonly IUnitOfWork _unitOfWork;

        public ComponentWriteService(IComponentCommandRepository commands, IComponentQueryRepository query, IUnitOfWork unitOfWork)
        {
            _commands = commands;
            _query = query;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateComponentInExistingFamilyAsync(NewComponentInFamilyDto dto, CancellationToken ct)
        {
            if (dto.FamilyId <= 0)
                throw new ArgumentException("FamilyId must be positive.", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ComponentName))
                throw new ArgumentException("ComponentName is required.", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ComponentFormCode))
                throw new ArgumentException("ComponentFormCode is required.", nameof(dto));

            var family = await _query.GetFamilyByIdWithFormAsync(dto.FamilyId, ct)
                ?? throw new InvalidOperationException($"Family {dto.FamilyId} not found.");

            var componentForm = await _query.GetFormTypeByCodeAsync(dto.ComponentFormCode, ct)
                ?? throw new InvalidOperationException($"FormType {dto.ComponentFormCode} does not exist");

            //await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // (Опционально) обновляем параметры семейства
            if (dto.UpdateFamilyParameters)
            {
                if (family.FamilyFormType is null)
                    throw new InvalidOperationException($"Family {family.Id} has no FamilyFormType loaded.");

                await UpsertFamilyValuesAsync(family.Id, family.FamilyFormType, dto.UpdatedFamilyParameters, ct);
            }

            // Создаеём компонент
            var component = new Component
            {
                ComponentFamilyId = family.Id,
                Name = dto.ComponentName,
                FormTypeId = componentForm.Id
            };
            // _db.Set<Component>().Add(component);
            // await _db.SaveChangesAsync(ct);

            // Значения параметров компонента
            var componentValues = BuildComponentValues(component.Id, componentForm, dto.ComponentParameters);
            if(componentValues.Count > 0)
            {
                //_db.Set<ParameterValue>().AddRange(componentValues);
                //await _db.SaveChangesAsync(ct);
            }

            //await tx.CommitAsync(ct);
            return component.Id;
        }
     

        public async Task<int> CreateFamilyAndComponentAsync(NewFamilyAndComponentDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.FamilyName))
                throw new ArgumentException("FamilyName is required.", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ComponentName))
                throw new ArgumentException("ComponentName is required.", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ComponentFormCode))
                throw new ArgumentException("ComponentFormCode is required.", nameof(dto));

            // Берём формы (с ParameterDefinition)
            var familyForm = await _query.GetFormTypeByCodeAsync(dto.FamilyFormCode, ct)
                ?? throw new InvalidOperationException($"FormType '{dto.FamilyFormCode}' not found.");

            var componentForm = await _query.GetFormTypeByCodeAsync(dto.ComponentFormCode, ct)
                ?? throw new InvalidOperationException($"FormType '{dto.ComponentFormCode}' not found.");

            //await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Создаём семейство
            var family = new ComponentFamily()
            {
                Name = dto.FamilyName,
                FamilyFormTypeId = familyForm.Id,
            };

            _commands.AddFamilyAsync(family, ct);
            // _db.Set<ComponentFamily>().Add(family);
            // await _db.SaveChangesAsync(ct); // нужен family.Id

            // Значения параметров семейства
            var familyValues = BuildFamilyValues(family.Id, familyForm, dto.FamilyParameters);
            //_db.Set<ParameterValue>().AddRange(familyValues);
            //await _db.SaveChangesAsync(ct);

            // Создаем компонент
            var component = new Component()
            {
                ComponentFamilyId = family.Id,
                Name = dto.ComponentName,
                FormTypeId = componentForm.Id,
            };
            // _db.Components.Add(component);
            // await _db.SaveChangesAsync(ct);



            // Создаём значеня параметров компонента
            var componentValues = BuildComponentValues(component.Id, componentForm, dto.ComponentParameters);
            //_db.ParameterValues.AddRange(componentValues);
            //await _db.SaveChangesAsync(ct);

            //await tx.CommitAsync(ct);

            return component.Id;
        }

        private static List<ParameterValue> BuildFamilyValues(int familyId, FormType familyForm, IReadOnlyList<ParameterDto> dtos)
        {
            if (dtos.Count == 0)
                return new List<ParameterValue>();

            var defByCode = familyForm.Parameters
                .ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

            var result = new List<ParameterValue>(dtos.Count);

            foreach(var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Code))
                    continue;

                if (!defByCode.TryGetValue(dto.Code, out var def))
                    continue;

                result.Add(new ParameterValue
                {
                    ParameterDefinitionId = def.Id,
                    ComponentFamilyId = familyId,
                    ComponentId = null,

                    StringValue = dto.StringValue,
                    DoubleValue = dto.DoubleValue,
                    IntValue = dto.IntValue,
                    Pins = dto.Pins
                });
            }

            return result;
        }

        private static List<ParameterValue> BuildComponentValues(int componentId, FormType componentForm, 
            IReadOnlyList<ParameterDto> dtos)
        {
            if (dtos.Count == 0)
                return new List<ParameterValue>();

            var defByCode = componentForm.Parameters
                .ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

            var result = new List<ParameterValue>(dtos.Count);

            foreach(var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Code))
                    continue;

                if (!defByCode.TryGetValue(dto.Code, out var def))
                    continue;

                result.Add(new ParameterValue
                {
                    ParameterDefinitionId = def.Id,
                    ComponentFamilyId = null,
                    ComponentId = componentId,

                    StringValue = dto.StringValue,
                    DoubleValue = dto.DoubleValue,
                    IntValue = dto.IntValue,
                    Pins = dto.Pins
                });
            }

            return result;
        }

        private async Task UpsertFamilyValuesAsync(int id, FormType familyFormType, IReadOnlyList<ParameterDto> updatedFamilyParameters, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
