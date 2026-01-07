using DocumentFormat.OpenXml.Wordprocessing;
using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.Abstractions.Services;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Domain;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public class ComponentFormBatchService : IComponentFormBatchService
    {
        private readonly IComponentReadRepository _componentRepository;
        private readonly IComponentFamilyReadRepository _familyRepository;
        private readonly IComponentNameParser _nameParser;
        private readonly ILogger<ComponentFormBatchService> _logger;

        public ComponentFormBatchService(
        IComponentReadRepository componentRepository,
        IComponentFamilyReadRepository familyRepository,
        IComponentNameParser nameParser,
        ILogger<ComponentFormBatchService> logger)
        {
            _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            _familyRepository = familyRepository ?? throw new ArgumentNullException(nameof(familyRepository));
            _nameParser = nameParser ?? throw new ArgumentNullException(nameof(nameParser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Построить формы для списка компонентов (батч запрос)
        /// </summary>
        public async Task<IReadOnlyList<ComponentFormResultDto>> BuildFormsAsync(
            IReadOnlyList<string> componentNames,
            CancellationToken ct = default)
        {
            if (componentNames == null || componentNames.Count == 0)
                return Array.Empty<ComponentFormResultDto>();

            _logger.LogDebug("Построение форм для {Count} компонентов", componentNames.Count);

            // 1. Загрузить все компоненты из БД одним батч-запросом 
            var existingComponents = await _componentRepository.GetByNamesAsync(componentNames, ct);
            var componentLookup = existingComponents.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

            // 2. Собрать уникальные имена семейств
            var familyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in componentNames)
            {
                // Если компонент найден в БД - берём его семейство
                if (componentLookup.TryGetValue(name, out var dbComponent))
                {
                    if (!string.IsNullOrWhiteSpace(dbComponent.FamilyName))
                        familyNames.Add(dbComponent.FamilyName);
                }
                else
                {
                    // Если компонент не найден - парсим имя
                    var parsed = _nameParser.Parse(name);
                    if (!string.IsNullOrWhiteSpace(parsed.Family))
                        familyNames.Add(parsed.Family);
                }
            }

            // 3. Загрузить все семейства одним батч-запросом
            var existingFamilies = await _familyRepository.GetByNamesAsync(familyNames, ct);
            var familyLookup = existingFamilies.ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug(
                "Найдено семейств в БД: {Found} из {Total}",
                familyLookup.Count,
                familyNames.Count);

            // 4. Построить результат для каждого компонента
            var results = new List<ComponentFormResultDto>(componentNames.Count);

            foreach (var name in componentNames)
            {
                ct.ThrowIfCancellationRequested();

                var result = BuildFormForComponent(name, componentLookup, familyLookup);
                results.Add(result);
            }

            _logger.LogInformation(
                "Формы построены: компонентов {Total}, с формой компонента {WithComponentForm}, с формой семейства {WithFamilyForm}, неопределённых {Undefined}",
                results.Count,
                results.Count(r => r.ComponentFormCode != null),
                results.Count(r => r.FamilyFormCode != null),
                results.Count(r => r.ComponentFormCode == null && r.FamilyFormCode == null));

            return results;
        }

        /// <summary>
        /// Построить форму для одного компонента
        /// </summary>
        private ComponentFormResultDto BuildFormForComponent(
            string componentName,
            Dictionary<string, ComponentLookupDto> componentLookup,
            Dictionary<string, ComponentFamilyLookupDto> familyLookup)
        {
            // 1. Проверить есть ли компонент в БД
            componentLookup.TryGetValue(componentName, out var dbComponent);

            // 2. Определить семейство
            ComponentFamilyLookupDto? dbFamily = null;

            if (dbComponent != null && !string.IsNullOrWhiteSpace(dbComponent.FamilyName))
            {
                // Семейство из компонента БД
                familyLookup.TryGetValue(dbComponent.FamilyName, out dbFamily);
            }
            else
            {
                // Парсим имя компонента
                var parsed = _nameParser.Parse(componentName);
                if (!string.IsNullOrWhiteSpace(parsed.Family))
                {
                    familyLookup.TryGetValue(parsed.Family, out dbFamily);
                }
            }

            // 3. Создать результат
            return new ComponentFormResultDto
            {
                ComponentName = componentName,

                // Компонент из БД
                ComponentId = dbComponent?.Id,
                ComponentFormCode = dbComponent?.FormCode,
                ComponentFormName = dbComponent?.FormTypeName,

                // Семейство из БД
                FamilyId = dbFamily?.Id,
                FamilyName = dbFamily?.Name,
                FamilyFormCode = dbFamily?.FormCode,
                FamilyFormName = dbFamily?.FormTypeName
            };
        }
    }
}
