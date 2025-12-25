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
using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Families;
using ElectronicMaps.Application.DTO.Components;
using ElectronicMaps.Application.WorkspaceProject.Models;

namespace ElectronicMaps.Application.Services
{
    public class ComponentAnalysisService : IComponentAnalysisService
    {
        private readonly IComponentSourceReader _sourceReader;
        private readonly IComponentReadRepository _components;
        private readonly IComponentFamilyReadRepository _families;

        public ComponentAnalysisService(IComponentSourceReader sourceReader, IComponentReadRepository components, IComponentFamilyReadRepository families)
        {
            _sourceReader = sourceReader;
            _components = components;
            _families = families;
        }




        //TODO: N+1 запросов. Получать Component и Family для компонента одним запросом.
        public async Task<IReadOnlyList<ImportedRow>> AnalyzeAsync(Stream stream, CancellationToken ct = default)
        {
            // Читаем исходные данные из XML.
            var source = await _sourceReader.ReadAsync(stream, ct);
            var now = DateTimeOffset.UtcNow;


            // Готовим словари по данным из БД.
            var componentLookUp = await BuildComponentLookUp(source, ct);
            var familyLookUp = await BuildFamilyLookUp(source, ct);

            // Анализируем каждый компонент из XML.
            var result = new List<ImportedRow>(source.Components.Count);
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

        private ImportedRow AnalyzeSingleComponent(SourceComponentDto srcComponent, Dictionary<string, ComponentLookUpDto> componentLookUp, Dictionary<string, ComponentFamilyLookupDto> familyLookUp, DateTimeOffset now, CancellationToken ct)
        {
            var name = (srcComponent.CleanName ?? string.Empty).Trim();

            // Определяем семейство либо от компонента из БД либо ро имени из XML.
            componentLookUp.TryGetValue(name, out var dbComponent);

            ComponentFamilyLookupDto? dbFamily = null;

            // Если компонент найден - семейство берём из компонента
            if (dbComponent is not null && !string.IsNullOrWhiteSpace(dbComponent.FamilyName))
            {
                familyLookUp.TryGetValue(dbComponent.FamilyName, out dbFamily);
            }
            // иначе находим по family из xml
            else if(!string.IsNullOrWhiteSpace(srcComponent.Family))
            {
                familyLookUp.TryGetValue(srcComponent.Family, out dbFamily);
            }

            var designatorsList = SplitDesignators(srcComponent.Designators);

                var analyzed = new ImportedRow(
                    RowId: Guid.NewGuid(),
                    // --- XML --- //
                    RawName: srcComponent.RawName,
                    CleanName: srcComponent.CleanName,
                    Family: dbFamily?.Name ?? srcComponent.Family,
                    Type: srcComponent.Type,
                    Quantity: srcComponent.Quantity,
                    Designator: srcComponent.Designators,
                    Designators: designatorsList,

                    // --- DB --- //
                    ComponentExistsInDatabase: dbComponent is not null,
                    ExistingComponentId: dbComponent?.Id,
                    DatabaseComponentName: dbComponent?.Name,

                    // --- Family --- //
                    FamilyExistsInDatabase: dbFamily is not null,
                    DatabaseFamilyId: dbFamily?.Id,
                    DatabaseFamilyName: dbFamily?.Name,

                    // --- Family form --- //
                    FamilyFormId: dbFamily?.FormTypeId,
                    FamilyFormTypeCode: dbFamily?.FormCode,
                    FamilyFormDisplayName: dbFamily?.Name,

                    // --- Component form --- //
                    ComponentFormId: dbComponent?.FormTypeId,
                    ComponentFormCode: dbComponent?.FormCode,
                    ComponentFormDisplayName: dbComponent?.FormName,

                    LastUpdatedUtc: now
                    );
            return analyzed;
        }

        private async Task<Dictionary<string, ComponentLookUpDto>> BuildComponentLookUp(ComponentSourceFileDto source, CancellationToken ct)
        {
            // Собираем все имена компонентов из XML.
            var names = source.Components
                .Select(c => c.CleanName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            // Загружаем компоненты из БД
            var existingComponents = await _components.GetLookupByNamesAsync(names!, ct);

            // Строим словарь существующих компонентов по CanonicalName:
            // CanonicalName → Component
            var componentLookUp = existingComponents
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

            return componentLookUp;
        }

        private async Task<Dictionary<string, ComponentFamilyLookupDto>> BuildFamilyLookUp(ComponentSourceFileDto source, CancellationToken ct)
        {
            // Собираем все имена семейств из XML.
            var familyNames = source.Components
                .Select(c => c.Family)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            //Загружаем существующие семейства из БД.
            var existingFamilies = await _families.GetLookupByNamesAsync(familyNames, ct);

            var familyLookUp = existingFamilies.ToDictionary(f=>f.Name, f=>f, StringComparer.OrdinalIgnoreCase);

            return familyLookUp;
        }
        //TODO: Разделение компонентов по "," и "-"
        private IReadOnlyList<string> SplitDesignators(string? designators)
        {
            if(string.IsNullOrWhiteSpace(designators))
                return Array.Empty<string>();

            return designators
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        }
    }
}
