using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public class ComponentParameterService : IComponentParameterService
    {
        private readonly IComponentQueryRepository _queries;
        private readonly IComponentCommandRepository _comands;

        public ComponentParameterService(IComponentQueryRepository queries, IComponentCommandRepository comands)
        {
            _queries = queries;
            _comands = comands;
        }

        public async Task<ComponentFormResultDto> GetComponentFormAsync(int componentId, CancellationToken ct)
        {
            var component = await _queries.GetByIdAsync(componentId, ct) ?? throw new InvalidOperationException($"Компонент с Id = {componentId} не найден.");

            var formType = await _queries.GetFormTypeByCodeAsync(component.FormCode, ct) ?? throw new InvalidOperationException($"Форма с кодом '{component.FormCode}' не найдена.");

            //Текущие значения параметров компонента
            var values = await _queries.GetParameterValuesAsync(componentId, ct);
            var valuesByDefId = values.ToDictionary(v => v.ParameterDefinitionId);

            //Собираем список ParameterDto (Definition + Value)
            var parameters = formType.Parameters.OrderBy(p => p.Order).Select(def =>
            {
                valuesByDefId.TryGetValue(def.Id, out var value);

                return new ParameterDto
                {
                    Code =def.Code,
                    DisplayName = def.DisplayName,
                    Unit = def.Unit,
                    StringValue = value?.StringValue,
                    DoubleValue = value?.DoubleValue,
                    IntValue = value?.IntValue,
                    Pins = value?.Pins
                };
            }).ToArray();

            return new ComponentFormResultDto
            {
                ComponentName = component.Name,
                Found = true,
                Parameters = parameters,
                ComponentId = component.Id,
            };
        }

        public async Task UpdateComponentParameterAsync(ComponentParameterUpdateDto dto, CancellationToken ct)
        {
            if(dto is null)
                throw new ArgumentNullException(nameof(dto));

            var component = await _queries.GetByIdAsync(dto.ComponentId, ct) ?? throw new InvalidOperationException($"Компонент с Id = {dto.ComponentId} не найден.");

            var formType = await _queries.GetFormTypeByCodeAsync(component.FormCode, ct) ?? throw new InvalidOperationException($"Форма с кодом '{component.FormCode}' не найдена.");

            var defsByCode = formType.Parameters.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

            //Текщие значения БД
            var existingValues = await _queries.GetParameterValuesAsync(dto.ComponentId, ct);
            var valuesByDefId = existingValues.ToDictionary(v => v.ParameterDefinitionId);

            var newValeus = new List<ParameterValue>();

            foreach (var dtoParam in dto.Parameters)
            {
                if(!defsByCode.TryGetValue(dtoParam.Code, out var definition))
                {
                    throw new InvalidOperationException(
                        $"Параметр с кодом '{dtoParam.Code}' не определён для формы '{component.FormCode}'.");
                }

                valuesByDefId.TryGetValue(definition.Id, out var entity);

                var hasAnyValue = dtoParam.StringValue is not null ||
                    dtoParam.DoubleValue is not null ||
                    dtoParam.IntValue is not null ||
                    dtoParam.Pins is not null;

                if (entity is null && !hasAnyValue)
                    continue;

                if(entity is null)
                {
                    entity = new ParameterValue
                    {
                        ParameterDefinitionId = definition.Id,
                        ComponentId = component.Id
                    };
                    newValeus.Add(entity);
                }

                switch(definition.ValueKind)
                {
                    case Domain.Enums.ParameterValueKind.String:
                        entity.StringValue = dtoParam.StringValue;
                        entity.DoubleValue = null;
                        entity.IntValue = null;
                        break;
                    case Domain.Enums.ParameterValueKind.Double:
                        entity.DoubleValue = dtoParam.DoubleValue;
                        entity.StringValue = null;
                        entity.IntValue = null;
                        break;
                    case Domain.Enums.ParameterValueKind.Int:
                        entity.IntValue = dtoParam.IntValue;
                        entity.StringValue = null;
                        entity.DoubleValue = null;
                        break;
                    case Domain.Enums.ParameterValueKind.WithPins:
                        entity.StringValue = dtoParam.StringValue;
                        entity.Pins = dtoParam.Pins;
                        entity.DoubleValue = null;
                        entity.IntValue = null;
                        break;
                    default:
                        throw new InvalidOperationException($"Неизвестный вид значения параметра: {definition.ValueKind}.");
                }
            }

            if(newValeus.Count > 0)
            {
                await _comands.AddParameterValuesAsync(newValeus, ct);
            }

            await _comands.SaveChangesAsync(ct);


        }
    }
}
