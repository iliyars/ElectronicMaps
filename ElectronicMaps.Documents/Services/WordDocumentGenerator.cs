using DocumentFormat.OpenXml.Packaging;
using ElectronicMaps.Documents.Configuration;
using ElectronicMaps.Documents.Helpers;
using ElectronicMaps.Documents.Models;
using ElectronicMaps.Documents.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ElectronicMaps.Documents.Services
{
    /// <summary>
    /// Сервис генерации Word документов из компонентов
    /// </summary>
    public class WordDocumentGenerator : IDocumentGenerator
    {
        private readonly ILogger<WordDocumentGenerator> _logger;
        private readonly ITemplateSchemaStore _schemaStorage;
        private readonly DocumentGeneratorOptions _options;

        public WordDocumentGenerator(ILogger<WordDocumentGenerator> logger, ITemplateSchemaStore schemaStorage, DocumentGeneratorOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _schemaStorage = schemaStorage ?? throw new ArgumentNullException(nameof(schemaStorage));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<GenerationResult> GenerateAsync(DocumentGenerationRequest request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Начало генерации документа. Форма: {FormCode}, Компонентов: {ComponentCount}",
                request.FormCode,
                request.Components.Count);

            try
            {
                // ═══════════════════════════════════════════════════════════
                // ШАГ 1: Валидация запроса
                // ═══════════════════════════════════════════════════════════
                ValidateRequest(request);

                // ═══════════════════════════════════════════════════════════
                // ШАГ 2: Загрузить JSON схему
                // ═══════════════════════════════════════════════════════════
                _logger.LogDebug("Загрузка JSON схемы для формы {FormCode}", request.FormCode);

                var schema = await _schemaStorage.GetSchemaAsync(request.FormCode, cancellationToken);

                _logger.LogInformation(
                    "Схема загружена: {DisplayName}, Компонентов на таблицу: {ComponentsPerTable}",
                    schema.TemplateName,
                    schema.ComponentsPerTable);

                // ═══════════════════════════════════════════════════════════
                // ШАГ 3: Найти путь к Word шаблону
                // ═══════════════════════════════════════════════════════════
                var templatesDir = request.TemplatesDirectory ?? _options.TemplatesDirectory;
                var templatePath = Path.Combine(templatesDir, schema.TemplateName);

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("Шаблон не найден: {TemplatePath}", templatePath);
                    return GenerationResult.Failure($"Шаблон не найден: {templatePath}");
                }

                _logger.LogDebug("Шаблон найден: {TemplatePath}", templatePath);

                // ═══════════════════════════════════════════════════════════
                // ШАГ 4: Подготовить выходной путь
                // ═══════════════════════════════════════════════════════════
                var outputPath = PrepareOutputPath(request.OutputPath);

                _logger.LogDebug("Выходной файл: {OutputPath}", outputPath);

                // ═══════════════════════════════════════════════════════════
                // ШАГ 5: Скопировать шаблон в выходной файл
                // ═══════════════════════════════════════════════════════════
                File.Copy(templatePath, outputPath, overwrite: true);

                _logger.LogDebug("Шаблон скопирован в выходной файл");

                // ═══════════════════════════════════════════════════════════
                // ШАГ 6: Вычислить сколько таблиц нужно
                // ═══════════════════════════════════════════════════════════
                int componentsPerTable = schema.ComponentsPerTable;
                int totalTables = (int)Math.Ceiling((double)request.Components.Count / componentsPerTable);

                _logger.LogInformation(
                    "Требуется таблиц: {TotalTables} ({ComponentCount} компонентов ÷ {PerTable} = {Ratio:F2})",
                    totalTables,
                    request.Components.Count,
                    componentsPerTable,
                    (double)request.Components.Count / componentsPerTable);

                int writtenCells = 0;

                // ═══════════════════════════════════════════════════════════
                // ШАГ 7-9: Открыть документ, клонировать таблицы, заполнить
                // ═══════════════════════════════════════════════════════════
                using (var doc = WordprocessingDocument.Open(outputPath, true))
                {
                    _logger.LogDebug("Документ открыт для редактирования");

                    // ШАГ 7: Клонировать таблицы
                    _logger.LogDebug("Клонирование таблиц...");

                    var tables = TableCloner.CloneTables(
                        doc,
                        totalTables,
                        _options.InsertPageBreaks);

                    _logger.LogInformation("Таблицы клонированы: {TableCount}", tables.Count);

                    // ШАГ 8: Заполнить каждую таблицу
                    int componentIndex = 0;

                    for (int tableIdx = 0; tableIdx < tables.Count; tableIdx++)
                    {
                        var table = tables[tableIdx];

                        _logger.LogDebug(
                            "Обработка таблицы {TableIndex}/{TotalTables}",
                            tableIdx + 1,
                            tables.Count);

                        // Заполнить до N компонентов в эту таблицу
                        for (int colIdx = 0; colIdx < componentsPerTable && componentIndex < request.Components.Count; colIdx++)
                        {
                            var component = request.Components[componentIndex];

                            _logger.LogTrace(
                                "Заполнение компонента {ComponentIndex} в таблицу {TableIndex}, колонка {ColumnIndex}",
                                componentIndex,
                                tableIdx,
                                colIdx);

                            // Заполнить все параметры компонента
                            int cellsInComponent = 0;

                            foreach (var mapping in schema.FieldMappings)
                            {
                                var value = component.GetValue(mapping.FieldCode) ?? " ";

                                // Вычислить колонку для этого компонента
                                var column = mapping.GetColumnForComponent(colIdx, schema.ColumnOffset);

                                // Записать в ячейку
                                if (CellWriter.WriteCell(table, mapping.Row, column, value))
                                {
                                    writtenCells++;
                                    cellsInComponent++;
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "Не удалось записать в ячейку [{Row},{Column}] для поля {FieldCode}",
                                        mapping.Row,
                                        column,
                                        mapping.FieldCode);
                                }
                            }

                            _logger.LogTrace(
                                "Компонент {ComponentIndex} записан: {CellCount} ячеек",
                                componentIndex,
                                cellsInComponent);

                            componentIndex++;
                        }
                    }

                    // ШАГ 9: Сохраняем изменения
                    doc.Save();
                    _logger.LogDebug("Изменения сохранены в документ");
                }

                stopwatch.Stop();

                // ═══════════════════════════════════════════════════════════
                // ШАГ 10: Вернуть результат
                // ═══════════════════════════════════════════════════════════
                var result = GenerationResult.Ok(
                    outputPath: outputPath,
                    processedComponents: request.Components.Count,
                    createdTables: totalTables,
                    writtenCells: writtenCells
                );

                _logger.LogInformation(
                    "Генерация завершена успешно за {ElapsedMs} мс. Таблиц: {Tables}, Ячеек: {Cells}, Файл: {OutputPath}",
                    stopwatch.ElapsedMilliseconds,
                    result.CreatedTables,
                    result.WrittenCells,
                    Path.GetFileName(outputPath));

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Ошибка генерации документа для формы {FormCode} после {ElapsedMs} мс",
                    request.FormCode,
                    stopwatch.ElapsedMilliseconds);

                return GenerationResult.Failure($"Ошибка генерации: {ex.Message}");


            }
        }

        /// <summary>
        /// Валидация запроса
        /// </summary>
        private void ValidateRequest(DocumentGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FormCode))
            {
                throw new ArgumentException("FormCode не может быть пустым", nameof(request));
            }

            if (request.Components == null || request.Components.Count == 0)
            {
                throw new ArgumentException("Список компонентов не может быть пустым", nameof(request));
            }


            if (string.IsNullOrWhiteSpace(request.OutputPath))
            {
                throw new ArgumentException("OutputPath не может быть пустым", nameof(request));
            }

            _logger.LogDebug("Валидация запроса пройдена");
        }

        /// <summary>
        /// Подготовить выходной путь (создать папку если нужно)
        /// </summary>
        private string PrepareOutputPath(string outputPath)
        {
            var directory = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                if (_options.CreateOutputDirectoryIfNotExists)
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug("Создана выходная папка: {Directory}", directory);
                }
                else
                {
                    throw new DirectoryNotFoundException($"Выходная папка не существует: {directory}");
                }
            }

            return outputPath;
        }

    }
}
