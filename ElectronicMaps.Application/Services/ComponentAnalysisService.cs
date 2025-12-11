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
using System.Net.WebSockets;

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

        //TODO: N+1 запросов. Получать Component и Family для компонента одним запросом.
        public async Task<IReadOnlyList<AnalyzedComponentDto>> AnalyzeAsync(Stream stream, CancellationToken ct = default)
        {
            // Читаем исходные данные из XML.
            var source = await _sourceReader.ReadAsync(stream, ct);
            var now = DateTimeOffset.UtcNow;


            // Готовим словари по данным из БД.
            var componentLookUp = await BuildComponentLookUp(source, ct);
            var familyLookUp = await BuildFamilyLookUp(source, ct);

            // Анализируем каждый компонент из XML.
            var result = new List<AnalyzedComponentDto>(source.Components.Count);
            foreach(var srcComponent in source.Components)
            {
                ct.ThrowIfCancellationRequested();

                var analyzed = AnalyzeSingleComponent(
                    srcComponent,
                    componentLookUp,
                    familyLookUp,
                    now,
                    ct);

                result.Add(analyzed);
            }

            return result;
        }

        private AnalyzedComponentDto AnalyzeSingleComponent(SourceComponentDto srcComponent, Dictionary<string, Component> componentLookUp, Dictionary<string, ComponentFamily> familyLookUp, DateTimeOffset now, CancellationToken ct)
        {
            var name = srcComponent.CleanName ?? string.Empty;

            componentLookUp.TryGetValue(name, out var dbComponent);

            // Определяем семейство либо от компонента из БД либо ро имени из XML.
            ComponentFamily? dbFamily = null;
            if (dbComponent != null)
            {
                dbFamily = dbComponent.ComponentFamily;
            }
            else if(!string.IsNullOrWhiteSpace(srcComponent.Family))
            {
                familyLookUp.TryGetValue(srcComponent.Family, out dbFamily);
            }

            var familyFormType = dbFamily?.FamilyFormType;
            var componentFormType = dbComponent?.FormType;

            var dto = new AnalyzedComponentDto
            {
                // --- XML ---
                RawName = srcComponent.RawName,
                CleanName = srcComponent.CleanName,
                Family = dbFamily?.Name ?? srcComponent.Family,
                Type = srcComponent.Type,
                Quantity = srcComponent.Quantity,
                Designator = srcComponent.Designators,

                // --- Компонент в БД ---
                ComponentExistsInDatabase = dbComponent != null,
                ExistingComponentId = dbComponent?.Id,
                DatabaseComponentName = dbComponent?.Name,

                // --- Семейство в БД ---
                FamilyExistsInDatabase = dbFamily != null,
                DatabaseFamilyId = dbFamily?.Id,
                DatabaseFamilyName = dbFamily?.Name,

                // --- Форма семейства ---
                FamilyFormTypeId = dbFamily?.FamilyFormTypeId,
                FamilyFormTypeCode = familyFormType?.Code,
                FamilyFormDisplayName = familyFormType?.DisplayName,

                // --- Форма компонента ---
                ComponentFormTypeId = dbComponent?.FormTypeId,
                ComponentFormTypeCode = componentFormType?.Code,
                ComponentFormDisplayName = componentFormType?.DisplayName,

                LastUpdatedUtc = now
            };

            return dto;
            
        }

        private async Task<Dictionary<string, Component>> BuildComponentLookUp(ComponentSourceFileDto source, CancellationToken ct)
        {
            // Собираем все имена компонентов из XML.
            var names = source.Components
                .Select(c => c.CleanName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            // Загружаем компоненты из БД
            var existingComponents = await _query.GetByNamesAsync(names, ct);

            // Строим словарь существующих компонентов по CanonicalName:
            // CanonicalName → Component
            var componentLookUp = existingComponents
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

            return componentLookUp;
        }

        private async Task<Dictionary<string, ComponentFamily>> BuildFamilyLookUp(ComponentSourceFileDto source, CancellationToken ct)
        {
            // Собираем все имена семейств из XML.
            var familyNames = source.Components
                .Select(c => c.Family)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            //Загружаем существующие семейства из БД.
            var existingFamilies = await _query.GetFamiliesByNamesAsync(familyNames, ct);

            var familyLookUp = existingFamilies.ToDictionary(f=>f.Name, f=>f, StringComparer.OrdinalIgnoreCase);

            return familyLookUp;
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
