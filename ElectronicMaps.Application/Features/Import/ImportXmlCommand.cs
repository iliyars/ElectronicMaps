using ElectronicMaps.Application.Features.Components.Services;
using ElectronicMaps.Application.Features.Import.Services;
using ElectronicMaps.Application.Stores;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Import
{
    /// <summary>
    /// Команда: Импортировать XML файл
    /// </summary>
    public record ImportXmlCommand(string FilePath) : IRequest<ImportXmlResult>;

    /// <summary>
    /// Результат импорта XML
    /// </summary>
    public record ImportXmlResult
    {
        public required bool IsSuccess { get; init; }
        public required int ComponentsImported { get; init; }
        public string? ErrorMessage { get; init; }

        public static ImportXmlResult Success(int count) => new()
        {
            IsSuccess = true,
            ComponentsImported = count
        };

        public static ImportXmlResult Failure(string error) => new()
        {
            IsSuccess = false,
            ComponentsImported = 0,
            ErrorMessage = error
        };
    }

    /// <summary>
    /// Обработчик команды импорта XML
    /// </summary>
    public class ImportXmlCommandHandler : IRequestHandler<ImportXmlCommand, ImportXmlResult>
    {
        private readonly IFileImportService _importService;
        private readonly IComponentAnalysisService _analysisService;
        private readonly IComponentStore _store;
        private readonly ILogger<ImportXmlCommandHandler> _logger;

        public ImportXmlCommandHandler(
            IFileImportService importService,
            IComponentAnalysisService analysisService,
            IComponentStore store,
            ILogger<ImportXmlCommandHandler> logger)
        {
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ImportXmlResult> Handle(ImportXmlCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало импорта XML из файла: {FilePath}", request.FilePath);

            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(request.FilePath))
                {
                    _logger.LogWarning("Пустой путь к файлу");
                    return ImportXmlResult.Failure("Не указан путь к файлу");
                }

                if (!File.Exists(request.FilePath))
                {
                    _logger.LogWarning("Файл не найден: {FilePath}", request.FilePath);
                    return ImportXmlResult.Failure($"Файл не найден: {request.FilePath}");
                }

                // Импорт файла
                _logger.LogDebug("Чтение XML файла...");
                using var stream = File.OpenRead(request.FilePath);

                var analyzed = await _analysisService.AnalyzeAsync(stream, cancellationToken);

                _logger.LogInformation("XML файл прочитан, найдено компонентов: {Count}", analyzed.Count);

                // Загрузка в store
                _logger.LogDebug("Инициализация рабочих данных в Store...");
                _store.InitializeWorking(analyzed);

                _logger.LogInformation(
                    "Импорт завершён успешно. Импортировано компонентов: {Count}",
                    analyzed.Count);

                return ImportXmlResult.Success(analyzed.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при импорте XML файла: {FilePath}", request.FilePath);
                return ImportXmlResult.Failure($"Ошибка импорта: {ex.Message}");
            }
        }
    }



}
