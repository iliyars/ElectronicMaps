using ElectronicMaps.Application.DTO;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Application.Utils;
using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Transactions;

namespace ElectronicMaps.Application.Services
{
    public class ComponentAnalysisService : IComponentAnalysisService
    {
        private readonly IComponentSourceReader _sourceReader;
        private readonly IComponentQueryRepository _query;

        public ComponentAnalysisService(IComponentSourceReader sourceReader, IComponentQueryRepository query)
        {
            _sourceReader = sourceReader;
            _query = query;
        }


        public async Task<IReadOnlyList<AnalyzedComponentDto>> AnalyzeAsync(Stream stream, CancellationToken ct = default)
        {
            var source = await _sourceReader.ReadAsync(stream, ct);
            // Собираем все именя компонентов из xml.
            var canonicalNames = source.Components
                .Select(c => c.CleanName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Загружаем из БД компоненты по имени.
            var existingComponents = await _query.GetByNamesAsync(canonicalNames, ct);

            // Строим словарь существующих компонентов по CanonicalName
            // для быстрого поиска: CanonicalName → Component
            var componentByName = existingComponents
                .Where(c => !string.IsNullOrWhiteSpace(c.CanonicalName))
                .ToDictionary(c => c.CanonicalName!,c => c,StringComparer.OrdinalIgnoreCase);

            // Собираем имена семейств из xml
            var familyNames = source.Components
                .Select(c => c.Family)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingFamilies = await _query.GetFamiliesByNamesAsync(familyNames, ct);

            // Строим словарь существующих семейств по Name
            // для быстрого поиска: CanonicalName → Component.
            var familyByName = existingFamilies
                .ToDictionary(f=>f.Name, StringComparer.OrdinalIgnoreCase);

            var result = new List<AnalyzedComponentDto>(source.Components.Count); 

            //Проверяем наличие компонента в БД.
            foreach (var srcComp in source.Components)
            {

                ct.ThrowIfCancellationRequested();

                var name = srcComp.CleanName;

                componentByName.TryGetValue(name, out var dbComponent);

                // Определяем семейство: либо от компонента, либо по имени из xml
                ComponentFamily? dbFamily = null;

                if (dbComponent != null)
                {
                    dbFamily = dbComponent.ComponentFamily;
                }
                else if (!string.IsNullOrWhiteSpace(srcComp.Family))
                {
                    familyByName.TryGetValue(srcComp.Family, out dbFamily);
                }

                // === Вариант 1: компонента в базе нет ===
                if (dbComponent is null)
                {
                    result.Add(new AnalyzedComponentDto
                    {
                        RawName = srcComp.RawName,
                        Type = srcComp.Type,
                        Family = srcComp.Family,
                        CleanName = srcComp.CleanName,
                        Quantity = srcComp.Quantity,
                        Designators = srcComp.Designators,

                        ExistsInDatabase = false,
                        ExistingComponentId = null,
                        DatabaseName = null,

                        FamilyExistsInDatabase = dbFamily != null,
                        DatabaseFamilyId = dbFamily?.Id,
                        DatabaseFamilyFormCode = dbFamily?.FamilyFormCode,

                        ComponentFormCode = string.Empty,
                        Parameters = Array.Empty<ParameterDto>(),
                        SchematicParameters = Array.Empty<ParameterDto>(),
                        LastUpdatedUtc = DateTimeOffset.UtcNow
                    });

                    continue;
                }

                // === Вариант 2: компонент в базе есть ===
                var values = await _query.GetParameterValuesAsync(dbComponent.Id, ct);
                var formType = await _query.GetFormTypeByCodeAsync(dbComponent.FormCode, ct)
                    ?? throw new InvalidOperationException($"Форма с кодом '{dbComponent.FormCode}' не найдена.");

                var paramDtos = BuildParameterDtos(formType, values);

                dbFamily ??= dbComponent.ComponentFamily;

                result.Add(new AnalyzedComponentDto
                {
                    RawName = srcComp.RawName,
                    Type = srcComp.Type,
                    Family = dbFamily?.Name ?? srcComp.Family,
                    CleanName = srcComp.CleanName,
                    Quantity = srcComp.Quantity,
                    Designators = srcComp.Designators,

                    ExistsInDatabase = true,
                    ExistingComponentId = dbComponent.Id,
                    DatabaseName = dbComponent.Name,

                    FamilyExistsInDatabase = dbFamily != null,
                    DatabaseFamilyId = dbFamily?.Id,
                    DatabaseFamilyFormCode = dbFamily?.FamilyFormCode,

                    ComponentFormCode = dbComponent.FormCode,
                    Parameters = paramDtos,
                    SchematicParameters = Array.Empty<ParameterDto>(),
                    LastUpdatedUtc = DateTimeOffset.UtcNow
                });
            }
        }


        private static IReadOnlyList<ParameterDto> BuildParameterDtos(FormType formType,IReadOnlyList<ParameterValue> values)
        {
            // Быстрое сопоставление ParameterDefinition → ParameterValue
            var valuesByDefId = values.ToDictionary(v => v.ParameterDefinitionId);

            var result = new List<ParameterDto>(formType.Parameters.Count);

            // Проходим по параметрам формы в правильном порядке
            foreach (var def in formType.Parameters.OrderBy(p => p.Order))
            {
                // пробуем найти соответствующее значение в БД
                valuesByDefId.TryGetValue(def.Id, out var val);

                // создаём DTO
                result.Add(new ParameterDto
                {
                    Code = def.Code,
                    DisplayName = def.DisplayName,
                    Unit = def.Unit,

                    StringValue = val?.StringValue,
                    DoubleValue = val?.DoubleValue,
                    IntValue = val?.IntValue,
                    Pins = val?.Pins
                });
            }

            return result;
        }

    }
}
