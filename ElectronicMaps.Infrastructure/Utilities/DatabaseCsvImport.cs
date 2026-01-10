using DocumentFormat.OpenXml.Wordprocessing;
using ElectronicMaps.Application.DTOs.Domain;
using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Enums;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace ElectronicMaps.Infrastructure.Utilities
{
    public class DatabaseCsvImport
    {

        private const string Separator = ";";
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseCsvImport> _logger;

        public DatabaseCsvImport(AppDbContext context, ILogger<DatabaseCsvImport> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        #region FormTypes Import

        public async Task<ImportResult> ImportFormTypesAsync(string csvFilePath)
        {
            var result = new ImportResult { EntityType = "FormType" };

            try
            {
                if(!File.Exists(csvFilePath))
                {
                    result.Errors.Add($"Файл не найден: {csvFilePath}");
                    return result;
                }

                var lines = File.ReadAllLines(csvFilePath);
                if (lines.Length < 2)
                {
                    result.Errors.Add("CSV файл пуст или содержит только заголовок");
                    return result;
                }

                // Проверить заголовок
                var header = lines[0];
                if(!header.Contains("Code") || !header.Contains("DisplayName") || !header.Contains("Description"))
                {
                    result.Errors.Add($"Неверный формат заголовка. Ожидается: Code;DisplayName;Description. Получено: {header}");
                    return result;
                }

                // Загрузить соответсвующие коды (для проверки дубликатов)
                var existingCodes = await _context.Set<FormType>()
                    .Select(f => f.Code)
                    .ToHashSetAsync();

                var formTypes = new List<FormType>();
                int id = 4;
                int lineNumber = 1;

                foreach(var line in lines.Skip(1))
                {
                    lineNumber++;

                    if(string.IsNullOrWhiteSpace(line))
                    {
                        _logger.LogDebug("Строка {LineNumber}: пустая, пропущено", lineNumber);
                        continue;
                    }

                    try
                    {
                        var parts = line.Split(Separator);

                        if (parts.Length < 2)
                        {
                            result.Warnings.Add($"Строка {lineNumber}: недостаточно колонок (минимум 2)");
                            continue;
                        }

                        var code = parts[0].Trim();
                        var displayName = parts[1].Trim();
                        var description = parts[2].Trim();

                        // Валидация
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: пустой код, пропущено");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: пустое имя для '{code}', пропущено");
                            continue;
                        }

                        // Проверка дубликатов
                        if (existingCodes.Contains(code))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: форма '{code}' уже существует, пропущено");
                            continue;
                        }

                        if (formTypes.Any(f => f.Code == code))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: дубликат '{code}' в CSV, пропущено");
                            continue;
                        }

                        formTypes.Add(new FormType
                        {
                            Id = id++,
                            Code = code,
                            DisplayName = displayName,
                            Description = description
                        });
                        result.ProcessedLines++;
                    }
                    catch(Exception ex)
                    {
                        result.Errors.Add($"Строка {lineNumber}: {ex.Message}");
                        _logger.LogError(ex, "Ошибка обработки строки {LineNumber}", lineNumber);
                    }
                }

                // сохранение в БД
                if(formTypes.Count > 0)
                {
                    _context.Set<FormType>().AddRange(formTypes);
                    await _context.SaveChangesAsync();
                    result.ImportedCount = formTypes.Count;
                    result.Success = true;

                    _logger.LogInformation("Импортировано FormTypes: {Count}", formTypes.Count);
                }
                else
                {
                    result.Warnings.Add("Нет данных для импорта");
                }   
            }
            catch(Exception ex)
            {
                result.Errors.Add($"Критическая ошибка: {ex.Message}");
                _logger.LogError(ex, "Ошибка импорта FormTypes");
            }
            return result;
        }

        #endregion

        #region ParameterDefinitions Import

        public async Task<ImportResult> ImportParametersDefinitionsAsync(
            string csvFilePath,
            int startId = 1000)
        {
            var result = new ImportResult { EntityType = "ParameterDefinitions" };

            try
            {
                if (!File.Exists(csvFilePath))
                {
                    result.Errors.Add($"Файл не найден: {csvFilePath}");
                    return result;
                }

                var lines = File.ReadAllLines(csvFilePath);
                if (lines.Length < 2)
                {
                    result.Errors.Add("CSV файл пуст или содержит только заголовок");
                    return result;
                }

                // Определить формат CSV
                var header = lines[0].Split(Separator);
                var isExtendedFormat = header.Length > 8; // Расширенный формат с валидацией
                var hasDataType = Array.Exists(header, h => h.Trim().Equals("DataType", StringComparison.OrdinalIgnoreCase));

                _logger.LogInformation("Формат CSV: {Format} ({Columns} колонок)",
                    isExtendedFormat ? "Расширенный" : "Минимальный",
                    header.Length);

                // Загрузить FormTypes
                var formTypes = await _context.Set<FormType>()
                    .ToDictionaryAsync(f => f.Code, f => f.Id);

                if (formTypes.Count == 0)
                {
                    result.Errors.Add("В БД нет FormTypes! Сначала импортируйте формы.");
                    return result;
                }

                var definitions = new List<ParameterDefinition>();
                int id = startId;
                int lineNumber = 1;

                foreach (var line in lines.Skip(1))
                {
                    lineNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        _logger.LogDebug("Строка {LineNumber}: пустая, пропущено", lineNumber);
                        continue;
                    }

                    try
                    {
                        var parts = line.Split(Separator);

                        if (parts.Length < 6)
                        {
                            result.Warnings.Add($"Строка {lineNumber}: недостаточно колонок (минимум 6)");
                            continue;
                        }

                        // ========== ОБЯЗАТЕЛЬНЫЕ ПОЛЯ ==========

                        var formCode = parts[0].Trim();
                        var paramCode = parts[1].Trim();
                        var displayName = parts[2].Trim();
                        var unit = parts[3].Trim();
                        var valueKindStr = parts[4].Trim();
                        var orderStr = parts[5].Trim();

                        // Валидация
                        if (string.IsNullOrWhiteSpace(formCode))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: пустой код формы, пропущено");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(paramCode))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: пустой код параметра, пропущено");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: пустое имя параметра, пропущено");
                            continue;
                        }

                        // Проверка FormType
                        if (!formTypes.TryGetValue(formCode, out int formId))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: форма '{formCode}' не найдена в БД, пропущено");
                            continue;
                        }

                        // Парсинг ValueKind
                        if (!Enum.TryParse<ParameterValueKind>(valueKindStr, true, out var valueKind))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: неверный ValueKind '{valueKindStr}', пропущено");
                            continue;
                        }

                        // Парсинг Order
                        if (!int.TryParse(orderStr, out int order))
                        {
                            result.Warnings.Add($"Строка {lineNumber}: неверный Order '{orderStr}', пропущено");
                            continue;
                        }

                        // ========== СОЗДАНИЕ ОБЪЕКТА ==========

                        var def = new ParameterDefinition
                        {
                            Id = id++,
                            FormTypeId = formId,
                            Code = paramCode,
                            DisplayName = displayName,
                            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit,
                            ValueKind = valueKind,
                            Order = order
                        };

                        // ========== ОПЦИОНАЛЬНЫЕ ПОЛЯ ВАЛИДАЦИИ (расширенный формат) ==========

                        if (isExtendedFormat && parts.Length >= 11)
                        {
                            // DataType (nullable)
                            var dataTypeStr = parts[6].Trim();
                            if (!string.IsNullOrWhiteSpace(dataTypeStr))
                            {
                                if (Enum.TryParse<ParameterDataType>(dataTypeStr, true, out var dataType))
                                {
                                    def.DataType = dataType;
                                }
                                else
                                {
                                    result.Warnings.Add($"Строка {lineNumber}: неверный DataType '{dataTypeStr}', игнорировано");
                                }
                            }

                            // MinValue (nullable)
                            var minValueStr = parts[7].Trim();
                            if (!string.IsNullOrWhiteSpace(minValueStr))
                            {
                                if (decimal.TryParse(minValueStr, out var minValue))
                                {
                                    def.MinValue = minValue;
                                }
                                else
                                {
                                    result.Warnings.Add($"Строка {lineNumber}: неверный MinValue '{minValueStr}', игнорировано");
                                }
                            }

                            // MaxValue (nullable)
                            var maxValueStr = parts[8].Trim();
                            if (!string.IsNullOrWhiteSpace(maxValueStr))
                            {
                                if (decimal.TryParse(maxValueStr, out var maxValue))
                                {
                                    def.MaxValue = maxValue;
                                }
                                else
                                {
                                    result.Warnings.Add($"Строка {lineNumber}: неверный MaxValue '{maxValueStr}', игнорировано");
                                }
                            }

                            // ValidationPattern (nullable)
                            var validationPattern = parts[9].Trim();
                            if (!string.IsNullOrWhiteSpace(validationPattern))
                            {
                                def.ValidationPattern = validationPattern;
                            }

                            // IsRequired
                            if (parts.Length >= 11)
                            {
                                var isRequiredStr = parts[10].Trim();
                                if (bool.TryParse(isRequiredStr, out var isRequired))
                                {
                                    def.IsRequired = isRequired;
                                }
                            }
                        }

                        definitions.Add(def);
                        result.ProcessedLines++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Строка {lineNumber}: {ex.Message}");
                        _logger.LogError(ex, "Ошибка обработки строки {LineNumber}", lineNumber);
                    }
                }

                // Сохранить в БД
                if (definitions.Count > 0)
                {
                    _context.Set<ParameterDefinition>().AddRange(definitions);
                    await _context.SaveChangesAsync();
                    result.ImportedCount = definitions.Count;
                    result.Success = true;

                    _logger.LogInformation("Импортировано ParameterDefinitions: {Count}", definitions.Count);
                }
                else
                {
                    result.Warnings.Add("Нет данных для импорта");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Критическая ошибка: {ex.Message}");
                _logger.LogError(ex, "Ошибка импорта ParameterDefinitions");
            }

            return result;
        }

        #endregion

        #region Export

        public  async Task<ExportResult> ExportParameterDefinitionsAsync(string csvFilePath)
        {
            var result = new ExportResult { EntityType = "ParameterDefinitions" };

            try
            {
                var parameters = await _context.Set<ParameterDefinition>()
                    .Include(p => p.FormType)
                    .OrderBy(p => p.FormType.Code)
                    .ThenBy(p => p.Order)
                    .ToListAsync();

                if (parameters.Count == 0)
                {
                    result.Warnings.Add("Нет данных для экспорта");
                    return result;
                }

                using var writer = new StreamWriter(csvFilePath);

                // Заголовок (расширенный формат)
                await writer.WriteLineAsync("FormCode;ParameterCode;DisplayName;Unit;ValueKind;DataType;MinValue;MaxValue;ValidationPattern;IsRequired;Order");

                foreach (var param in parameters)
                {
                    var line = string.Join(Separator,
                        param.FormType.Code,
                        param.Code,
                        param.DisplayName,
                        param.Unit ?? "",
                        param.ValueKind,
                        param.DataType?.ToString() ?? "",
                        param.MinValue?.ToString() ?? "",
                        param.MaxValue?.ToString() ?? "",
                        param.ValidationPattern ?? "",
                        param.IsRequired,
                        param.Order
                    );

                    await writer.WriteLineAsync(line);
                    result.ExportedCount++;
                }

                result.Success = true;
                _logger.LogInformation("Экспортировано параметров: {Count}", result.ExportedCount);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Ошибка экспорта: {ex.Message}");
                _logger.LogError(ex, "Ошибка экспорта ParameterDefinitions");
            }

            return result;
        }

        #endregion

        #region Utility Method

        /// <summary>
        /// Получить следующий доступный ID для сущности
        /// </summary>
        private async Task<int> GetNextIdAsync<T>() where T : class
        {
            var maxId = await _context.Set<T>()
                .Select(e => EF.Property<int>(e, "Id"))
                .DefaultIfEmpty(0)
                .MaxAsync();

            return maxId + 1;
        }

        /// <summary>
        /// Очистить таблицу в БД
        /// </summary>
        public async Task ClearTableAsync(string tableName)
        {
            _logger.LogWarning("ОЧИСТКА ТАБЛИЦЫ: {TableName}", tableName);

            if (_context.Database.IsSqlite())
            {
                // SQLite: DELETE
                await _context.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
                await _context.Database.ExecuteSqlRawAsync(
                    $"DELETE FROM sqlite_sequence WHERE name = '{tableName}'");
            }
            else
            {
                // PostgreSQL: TRUNCATE
                await _context.Database.ExecuteSqlRawAsync(
                    $"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE");
            }

            _logger.LogInformation("✅ Таблица {TableName} очищена", tableName);
        }

        #endregion
    }

    #region Resut Classes
    
    public class ImportResult
    {
        public string EntityType { get; set; } = "";
        public bool Success { get; set; }
        public int ProcessedLines { get; set; }
        public int ImportedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void PrintSummary()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            Console.WriteLine($"\n========== Import {EntityType} ==========");
            Console.WriteLine($"Status: {(Success ? "✅ Success" : "❌ Error")}");
            Console.WriteLine($"Calculated rows: {ProcessedLines}");
            Console.WriteLine($"imported: {ImportedCount}");

            if (Warnings.Count > 0)
            {
                Console.WriteLine($"\n⚠️  Warning ({Warnings.Count}):");
                foreach (var warning in Warnings.Take(10))
                {
                    Console.WriteLine($"  - {warning}");
                }
                if (Warnings.Count > 10)
                    Console.WriteLine($"  ... and {Warnings.Count - 10}");
            }

            if (Errors.Count > 0)
            {
                Console.WriteLine($"\n❌ Error ({Errors.Count}):");
                foreach (var error in Errors.Take(10))
                {
                    Console.WriteLine($"  - {error}");
                }
                if (Errors.Count > 10)
                    Console.WriteLine($"  ... end {Errors.Count - 10}");
            }

            Console.WriteLine("=====================================\n");
        }
    }

    public class ExportResult
    {
        public string EntityType { get; set; } = "";
        public bool Success { get; set; }
        public int ExportedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void PrintSummary()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.WriteLine($"\n========== ЭКСПОРТ {EntityType} ==========");
            Console.WriteLine($"Статус: {(Success ? "✅ Успешно" : "❌ Ошибка")}");
            Console.WriteLine($"Экспортировано: {ExportedCount}");

            if (Errors.Count > 0)
            {
                Console.WriteLine($"\n❌ Ошибки:");
                foreach (var error in Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Console.WriteLine("======================================\n");
        }
    }

    #endregion
}
