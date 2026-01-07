using ElectronicMaps.Application.Abstractions.Queries.Components;
using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.Abstractions.Services;
using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Domain;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Application.Features.Workspace.Models;
using Microsoft.Extensions.Logging;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public class ComponentAnalysisService : IComponentAnalysisService
    {
        private readonly IComponentSourceReader _sourceReader;
        private readonly IComponentReadRepository _components;
        private readonly IComponentFamilyReadRepository _families;
        private readonly ILogger<ComponentAnalysisService> _logger;

        public ComponentAnalysisService(
            IComponentSourceReader sourceReader,
            IComponentReadRepository components,
            IComponentFamilyReadRepository families,
            ILogger<ComponentAnalysisService> logger)
        {
            _sourceReader = sourceReader;
            _components = components;
            _families = families;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ImportedRow>> AnalyzeAsync(
            Stream stream,
            CancellationToken ct = default)
        {

            _logger.LogInformation("Начало анализа компонентов из файла");

            try
            {

                // 1. Читаем исходные данные из XML
                var source = await _sourceReader.ReadAsync(stream, ct);
                var now = DateTimeOffset.UtcNow;

                _logger.LogInformation(
                    "Прочитано {Count} компонентов из исходного файла",
                    source.Components.Count);

                // 2. Собираем все уникальные имена компонентов и семейств
                var componentNames = ExtractComponentNames(source);
                var familyNames = ExtractFamilyNames(source);

                // 3. Загружаем данные из БД батчем (оптимизация N+1)
                var componentLookup = await BuildComponentLookupAsync(componentNames, ct);
                var familyLookup = await BuildFamilyLookupAsync(familyNames, ct);

                _logger.LogDebug(
                   "Уникальных имён компонентов: {ComponentCount}, семейств: {FamilyCount}",
                   componentNames.Count,
                   familyNames.Count);

                // 4. Анализируем каждый компонент
                var result = new List<ImportedRow>(source.Components.Count);

                foreach (var srcComponent in source.Components)
                {
                    ct.ThrowIfCancellationRequested();

                var analyzed = AnalyzeSingleComponent(
                    srcComponent,
                    componentLookup,
                    familyLookup,
                    now);

                    result.Add(analyzed);
                }

                _logger.LogInformation(
                   "Анализ завершён. Обработано {Count} компонентов",
                   result.Count);


                return result;
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при анализе компонентов");
                throw;
            }
        }

        #region Extract Names

        /// <summary>
        /// Извлечь все уникальные имена компонентов из источника
        /// </summary>
        private HashSet<string> ExtractComponentNames(ComponentSourceFileDto source)
        {
            return source.Components
                .Select(c => c.CleanName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
        }

        /// <summary>
        /// Извлечь все уникальные имена семейств из источника
        /// </summary>
        private HashSet<string> ExtractFamilyNames(ComponentSourceFileDto source)
        {
            return source.Components
                .Select(c => c.Family)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
        }

        #endregion

        #region Build Lookups (Batch Queries)

        private async Task<Dictionary<string, ComponentLookupDto>> BuildComponentLookupAsync(
        HashSet<string> names,
        CancellationToken ct)
        {
            if (names.Count == 0)
                return new Dictionary<string, ComponentLookupDto>(StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("Загрузка {Count} компонентов из БД", names.Count);

            // Батч запрос: загружаем все компоненты одним запросом
            var existingComponents = await _components.GetByNamesAsync(names, ct);

            // Строим словарь: Name → Component
            var lookup = existingComponents
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug(
                "Загружено компонентов из БД: {Found} из {Total}",
                lookup.Count,
                names.Count);

            return lookup;
        }

        /// <summary>
        /// Построить словарь семейств из БД (батч запрос)
        /// </summary>
        private async Task<Dictionary<string, ComponentFamilyLookupDto>> BuildFamilyLookupAsync(
            HashSet<string> names,
            CancellationToken ct)
        {
            if (names.Count == 0)
                return new Dictionary<string, ComponentFamilyLookupDto>(StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("Загрузка {Count} семейств из БД", names.Count);

            // Батч запрос: загружаем все семейства одним запросом
            var existingFamilies = await _families.GetByNamesAsync(names, ct);

            // Строим словарь: Name → Family
            var lookup = existingFamilies
                .ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug(
                "Загружено семейств из БД: {Found} из {Total}",
                lookup.Count,
                names.Count);

            return lookup;
        }

        #endregion

        #region Single Component Analysis

        /// <summary>
        /// Анализ одного компонента из источника
        /// </summary>
        private ImportedRow AnalyzeSingleComponent(
            SourceComponentDto srcComponent,
            Dictionary<string, ComponentLookupDto> componentLookup,
            Dictionary<string, ComponentFamilyLookupDto> familyLookup,
            DateTimeOffset now)
        {
            var name = (srcComponent.CleanName ?? string.Empty).Trim();

            // 1. Ищем компонент в БД
            componentLookup.TryGetValue(name, out var dbComponent);

            // 2. Определяем семейство
            var dbFamily = DetermineFamily(srcComponent, dbComponent, familyLookup);

            // 3. Разбиваем designators на список
            var designatorsList = SplitDesignators(srcComponent.Designators);

            // 4. Создаём результат анализа
            return new ImportedRow(
                RowId: Guid.NewGuid(),

                // XML данные
                RawName: srcComponent.RawName,
                CleanName: srcComponent.CleanName,
                Family: dbFamily?.Name ?? srcComponent.Family,
                Type: srcComponent.Type,
                Quantity: srcComponent.Quantity,
                Designator: srcComponent.Designators,
                Designators: designatorsList,

                // Компонент из БД
                ComponentExistsInDatabase: dbComponent is not null,
                ExistingComponentId: dbComponent?.Id,
                DatabaseComponentName: dbComponent?.Name,

                // Семейство из БД
                FamilyExistsInDatabase: dbFamily is not null,
                DatabaseFamilyId: dbFamily?.Id,
                DatabaseFamilyName: dbFamily?.Name,

                // Форма семейства
                FamilyFormId: dbFamily?.FormTypeId,
                FamilyFormTypeCode: dbFamily?.FormCode,
                FamilyFormDisplayName: dbFamily?.FormTypeName,

                // Форма компонента
                ComponentFormId: dbComponent?.FormTypeId,
                ComponentFormCode: dbComponent?.FormCode,
                ComponentFormDisplayName: dbComponent?.FormTypeName,

                LastUpdatedUtc: now
            );
        }

        /// <summary>
        /// Определить семейство компонента
        /// Приоритет: семейство из найденного компонента → семейство из XML
        /// </summary>
        private ComponentFamilyLookupDto? DetermineFamily(
            SourceComponentDto srcComponent,
            ComponentLookupDto? dbComponent,
            Dictionary<string, ComponentFamilyLookupDto> familyLookup)
        {
            // Случай 1: Компонент найден в БД → берём его семейство
            if (dbComponent is not null && !string.IsNullOrWhiteSpace(dbComponent.FamilyName))
            {
                familyLookup.TryGetValue(dbComponent.FamilyName, out var familyFromComponent);
                return familyFromComponent;
            }

            // Случай 2: Компонент не найден → ищем семейство по имени из XML
            if (!string.IsNullOrWhiteSpace(srcComponent.Family))
            {
                familyLookup.TryGetValue(srcComponent.Family, out var familyFromXml);
                return familyFromXml;
            }

            // Случай 3: Семейство не определено
            return null;
        }

        #endregion

        #region Designators Split

        /// <summary>
        /// Разделить строку designators на список
        /// Поддерживает: запятые, точки с запятой, пробелы
        /// TODO: Добавить поддержку диапазонов (R1-R5)
        /// </summary>
        private IReadOnlyList<string> SplitDesignators(string? designators)
        {
            if (string.IsNullOrWhiteSpace(designators))
                return Array.Empty<string>();

            return designators
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
        }

        #endregion

    }
}
