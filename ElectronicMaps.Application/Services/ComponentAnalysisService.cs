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
            using (var fw = new StreamWriter("out.txt"))
            {
                foreach(var comp in source.Components)
                {
                    fw.WriteLine($"Тип:{comp.Type} \t\tИмя:{comp.CleanName} \t\t src: {comp.RawName}");
                }
            }
                var canonicalNames = source.Components
                    .Select(c => NameNormalizer.Normalize(c.CleanName))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingComponents = await _query.GetByNamesAsync(canonicalNames, ct);

            var compByCanonical = existingComponents.ToDictionary(c => c.CanonicalName!, c => c, StringComparer.OrdinalIgnoreCase);

            var result = new List<AnalyzedComponentDto>(source.Components.Count); // TODO: preallocate);

            foreach (var srcComp in source.Components)
            {
                var canonical = NameNormalizer.Normalize(srcComp.CleanName);
                
                if(!compByCanonical.TryGetValue(canonical, out var dbComponent))
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
                        ComponentFormCode = string.Empty,
                        Parameters = Array.Empty<ParameterDto>(),
                        SchematicParameters = Array.Empty<ParameterDto>(),

                        LastUpdatedUtc = DateTimeOffset.UtcNow
                    });

                    continue;
                }

                var values = await _query.GetParameterValuesAsync(dbComponent.Id, ct);
                var formType = await _query.GetFormTypeByCodeAsync(dbComponent.FormCode, ct) ?? throw new InvalidOperationException($"Форма с кодом '{dbComponent.FormCode}' не найдена.");

                var paramDtos = BuildParameterDtos(formType, values);

                result.Add(new AnalyzedComponentDto
                {
                    RawName = srcComp.RawName,
                    Type = srcComp.Type,
                    Family = dbComponent.ComponentFamily.Name ?? srcComp.Family,
                    CleanName = srcComp.CleanName,
                    Quantity = srcComp.Quantity,
                    Designators = srcComp.Designators,

                    ExistsInDatabase = true,
                    ExistingComponentId = dbComponent.Id,
                    DatabaseName = dbComponent.Name,
                    ComponentFormCode = dbComponent.FormCode,
                    DatabaseFamilyId = dbComponent.ComponentFamily.Id,
                    DatabaseFamilyFormCode = dbComponent.ComponentFamily.FamilyFormCode,
                    Parameters = paramDtos,
                    SchematicParameters = Array.Empty<ParameterDto>(),
                    LastUpdatedUtc = DateTimeOffset.UtcNow
                });
            }
            return result;

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
