using ElectronicMaps.Application.Utils;
using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElectronicMaps.Application.Utils;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    public class ComponentCreationService : IComponentCreationService
    {
        private readonly IComponentCommandRepository _commands;
        private readonly IComponentQueryRepository _queries;

        public ComponentCreationService(IComponentCommandRepository commands, IComponentQueryRepository queries)
        {
            _commands = commands;
            _queries = queries;
        }

        public async Task<ComponentDto> AddComponentAsync(ComponentCreateDto dto, CancellationToken ct = default)
        {

            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            // 1. Нормализуем имя для поиска / дедупликации
            var canonicalName = NameNormalizer.Normalize(dto.Name);

            var exists = await _queries.ExistsAsync(canonicalName, ct);
            if(exists)
            {
                throw new InvalidOperationException(
                    $"Компонент с именем '{dto.Name}' (каноничное '{canonicalName}') и формой '{dto.FormCode}' уже существует.");
            }

            // 3. Находим или создаём семейство
            var family = await ResolveFamilyAsync(dto, ct);

            // 4. Получаем тип формы для КОМПОНЕНТА (63, 64, ...)
            var formType = await _queries.GetFormTypeByCodeAsync(dto.FormCode, ct)
                ?? throw new InvalidOperationException($"Форма с кодом '{dto.FormCode}' не найдена.");

            // 5. Создаём сущность компонента
            var component = new Component
            {
                Name = dto.Name,
                CanonicalName = canonicalName,
                ComponentFamilyId = family.Id,
                FormCode = dto.FormCode,
            };

            // 6. Собираем словарь определений параметров по коду
            var defsByCode = formType.Parameters
                .ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

            var parameterValues = new List<ParameterValue>();

            foreach(var dtoParam in dto.Parameters)
            {
                if(!defsByCode.TryGetValue(dtoParam.Code, out var definition))
                {
                    throw new InvalidOperationException(
                        $"Параметр с кодом '{dtoParam.Code}' не определён для формы '{dto.FormCode}'.");
                }

                var value = new ParameterValue
                {
                    ParameterDefinitionId = definition.Id,
                    Component = component
                };

                switch(definition.ValueKind)
                {
                    case Domain.Enums.ParameterValueKind.String:
                        value.StringValue = dtoParam.StringValue;
                        break;

                    case Domain.Enums.ParameterValueKind.Double:
                        value.DoubleValue = dtoParam.DoubleValue;
                        break;

                    case Domain.Enums.ParameterValueKind.Int:
                        value.IntValue = dtoParam.IntValue;
                        break;

                    case Domain.Enums.ParameterValueKind.WithPins:
                        value.StringValue = dtoParam.StringValue;
                        value.Pins = dtoParam.Pins;
                        break;

                    default:
                        throw new NotSupportedException(
                            $"Тип значения '{definition.ValueKind}' пока не поддерживается в сервисе создания компонентов.");
                }

                parameterValues.Add(value);
            }

            await _commands.AddAsync(component, ct);

            if(parameterValues.Count > 0)
            {
                await _commands.AddParameterValuesAsync(parameterValues, ct);
            }

            await _commands.SaveChangesAsync(ct);

            return new ComponentDto
            {
                Id = component.Id,
                Name = component.Name,
                FamilyName = family.Name,
                FormCode = component.FormCode
            };
        }

        private async Task<ComponentFamily> ResolveFamilyAsync(ComponentCreateDto dto, CancellationToken ct)
        {
            ComponentFamily? family = null;

            if (dto.ComponentFamilyId != 0)
            {
                family = await _queries.GetFamilyByIdAsync(dto.ComponentFamilyId.Value, ct);
                if (family == null)
                {
                    throw new InvalidOperationException(
                        $"Семейство с Id = {dto.ComponentFamilyId} не найдено.");
                }

                return family;
            }

            if(!string.IsNullOrWhiteSpace(dto.FamilyName))
            {
                family = await _queries.GetFamilyByNameAsync(dto.FamilyName!, ct);
                if(family != null)
                {
                    return family;
                }

                family = new ComponentFamily
                {
                    Name = dto.FamilyName!,
                    FamilyFormCode = "FORM4" // Общая форма для семейств
                };

                await _commands.AddFamilyAsync(family, ct);
                await _commands.SaveChangesAsync(ct); // чтобы появился Id

                return family;
            }

            throw new InvalidOperationException(
                "Не задано семейство компонента. Укажите либо ComponentFamilyId, либо FamilyName.");
        }
    }
}
